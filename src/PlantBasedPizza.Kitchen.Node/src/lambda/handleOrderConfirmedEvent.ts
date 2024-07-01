import { SQSBatchItemFailure, SQSBatchResponse, SQSEvent } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { OrderConfirmedEventHandler } from "../commands/orderConfirmedEventHandler";
import { EventBridgeEventPublisher } from "../adapters/eventBridgeEventPublisher";
import { RecipeService } from "../adapters/recipeService";
import { CloudEventV1, HTTP } from "cloudevents";
import { OrderConfirmedEvent } from "../integration-events/orderConfirmedEvent";
import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { SpanContext, tracer } from "dd-trace";
import { getParameter } from "@aws-lambda-powertools/parameters/ssm";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { SpanContextWrapper } from "datadog-lambda-js/dist/trace/span-context-wrapper";
import { EventBridgeWrapper } from "../adapters/eventBridgeWrapper";

tracer.init();

const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);
const recipeService = new RecipeService();

const kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);
const eventHandler = new OrderConfirmedEventHandler(kitchenRepository, eventPublisher, recipeService);

export const handler = async (event: SQSEvent): Promise<SQSBatchResponse> => {
  const activeSpan = tracer.scope().active();
  const batchItemFailures: SQSBatchItemFailure[] = [];

  const headers = {
    "content-type": "application/cloudevents+json",
  };

  for (var sqsMessage of event.Records) {
    await tracer.trace("processing message", {
      childOf: activeSpan?.context(),
    }, async (span) => {
      try {
        const eventBridgeWrapper: EventBridgeWrapper = JSON.parse(sqsMessage.body);

        const cloudEvent = HTTP.toEvent({ body: eventBridgeWrapper.detail, headers }) as CloudEventV1<OrderConfirmedEvent>;

        console.log(cloudEvent.ddtraceid as string);
        console.log(cloudEvent.ddspanid as string);

        tracer.trace("child-span", {
          childOf: new ManualSpanContext(cloudEvent.ddtraceid as string, cloudEvent.ddspanid as string)
        }, () => {
          console.log('trace some stuff');
        });

        await eventHandler.handle(cloudEvent.data!);
      } catch (e) {
        span?.addTags({
          error: true,
          errorMessage: e,
        });
        
        console.log(e);

        batchItemFailures.push({
          itemIdentifier: sqsMessage.messageId,
        });
      }

      span!.finish();
    });
  }

  activeSpan?.addTags({
    "sqs.messagesInBatch": event.Records.length,
    "sqs.failedMessages": batchItemFailures.length,
  });

  if (batchItemFailures.length > 0) {
    activeSpan?.addTags({
      error: true,
      errorMessage: 'There is at least one failure in the batch.'
    });
  }

  return {
    batchItemFailures,
  };
};

export class ManualSpanContext implements SpanContext {

  traceId: string;
  spanId: string;
  constructor(traceId: string, spanId: string) {
    this.traceId = traceId;
    this.spanId = spanId;
  }

  toTraceId(): string {
    return this.traceId;
  }
  toSpanId(): string {
    return this.spanId;
  }
  toTraceparent(): string {
    return '';
  }
}