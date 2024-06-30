import { SQSBatchItemFailure, SQSBatchResponse, SQSEvent } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { OrderConfirmedEventHandler } from "../commands/orderConfirmedEventHandler";
import { EventBridgeEventPublisher } from "../adapters/eventBridgeEventPublisher";
import { RecipeService } from "../adapters/recipeService";
import { CloudEventV1, HTTP } from "cloudevents";
import { OrderConfirmedEvent } from "../integration-events/orderConfirmedEvent";
import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { SpanContext, tracer } from "dd-trace";
import { getParameter } from '@aws-lambda-powertools/parameters/ssm';
import { GetParameterCommand, SSMClient } from "@aws-sdk/client-ssm";

tracer.init();

const eventBridgeClient = new EventBridgeClient();

var kitchenRepository: KitchenRequestRepository | undefined = undefined;

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);
const recipeService = new RecipeService();

var eventHandler: OrderConfirmedEventHandler | undefined = undefined;

export const handler = async (event: SQSEvent): Promise<SQSBatchResponse> => {
  if (kitchenRepository === undefined || eventHandler === undefined) {
    console.log('Initializating');
    const mongoConnectionString = await getParameter(process.env.CONN_STRING_PARAM!, {
      decrypt: true
    });
    kitchenRepository = new KitchenRequestRepository(mongoConnectionString!, "PlantBasedPizza", "kitchen");
    eventHandler = new OrderConfirmedEventHandler(kitchenRepository, eventPublisher, recipeService);
  }

  const batchItemFailures: SQSBatchItemFailure[] = [];

  const mainSpan = tracer.scope().active();

  const headers = {
    "content-type": "application/cloudevents+json",
  };

  for (const sqsMessage of event.Records) {
    try {
      console.log(sqsMessage.messageId);

      const cloudEvent = HTTP.toEvent({ body: sqsMessage.body, headers }) as CloudEventV1<OrderConfirmedEvent>;

      const traceId = cloudEvent.ddtraceid;
      const spanId = cloudEvent.ddspanid;

      await eventHandler.handle(cloudEvent.data!);
    } catch (e) {
      console.log(e);
      batchItemFailures.push({
        itemIdentifier: sqsMessage.messageId,
      });
    }
  }

  return {
    batchItemFailures,
  };
};
