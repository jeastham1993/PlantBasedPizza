import { ALBEvent, ALBResult } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { EventBridgeEventPublisher } from "../adapters/eventBridgeEventPublisher";
import { tracer } from "dd-trace";
import { SetKitchenRequestPreparingCommand } from "../commands/setKitchenRequestPreparingHandler";
import { OrderState } from "../entities/kitchenRequest";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";

tracer.init();

const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);

var kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);

export const handler = async (event: ALBEvent): Promise<ALBResult> => {
  const parsedBody: SetKitchenRequestPreparingCommand = JSON.parse(event.body!);

  const kitchenRequest = await kitchenRepository.retrieve(parsedBody.orderIdentifier);

  if (kitchenRequest === null) {
    return {
      statusCode: 404,
      headers: { "content-type": "application/json" },
      body: "{}",
    };
  }

  kitchenRequest.orderState = OrderState.PREPARING;
  
  await kitchenRepository.update(kitchenRequest);
  await eventPublisher.publishOrderPreparingEventV1({
    orderIdentifier: kitchenRequest.orderIdentifier,
    kitchenIdentifier: kitchenRequest.kitchenRequestId,
  });

  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(kitchenRequest),
  };
};
