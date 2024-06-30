import { SQSBatchItemFailure, SQSBatchResponse, SQSEvent } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { OrderConfirmedEventHandler } from "../commands/orderConfirmedEventHandler";
import { EventBridgeEventPublisher } from "../adapters/eventBridgeEventPublisher";
import { RecipeService } from "../adapters/recipeService";
import { CloudEventV1, HTTP } from "cloudevents";
import { OrderConfirmedEvent } from "../integration-events/orderConfirmedEvent";
import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { tracer } from "dd-trace";
import { getParameter } from '@aws-lambda-powertools/parameters/ssm';
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";

tracer.init();

const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);
const recipeService = new RecipeService();

const kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);
const eventHandler = new OrderConfirmedEventHandler(kitchenRepository, eventPublisher, recipeService);

export const handler = async (event: any): Promise<SQSBatchResponse> => {
  const batchItemFailures: SQSBatchItemFailure[] = [];

  const headers = {
    "content-type": "application/cloudevents+json",
  };

  for (const sqsMessage of event.Records) {
    try {
      const cloudEvent = HTTP.toEvent({ body: sqsMessage.body, headers }) as CloudEventV1<OrderConfirmedEvent>;

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
