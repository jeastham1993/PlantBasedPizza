import { ALBEvent, ALBResult } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { getParameter } from "@aws-lambda-powertools/parameters/ssm";
import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { EventBridgeEventPublisher } from "../adapters/eventBridgeEventPublisher";
import { tracer } from "dd-trace";
import { SetKitchenRequestPreparingCommand } from "../commands/setKitchenRequestPreparingHandler";
import { OrderState } from "../entities/kitchenRequest";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { Authorizer } from "../authorization/authorizer";

tracer.init();

const secretKey = getParameter(process.env.JWT_SSM_PARAM!);

const authorizer: Authorizer = new Authorizer(secretKey);

const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);

var kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);

export const handler = async (event: ALBEvent): Promise<ALBResult> => {
  const isAuthorized = await authorizer.authorizeRequest(event, ["staff", "admin"]);

  if (!isAuthorized){
    return {
      statusCode: 401,
      headers: { "content-type": "application/json" },
      body: '{}',
    };  
  }
  
  const parsedBody: SetKitchenRequestPreparingCommand = JSON.parse(event.body!);

  const kitchenRequest = await kitchenRepository.retrieve(parsedBody.orderIdentifier);

  if (kitchenRequest === null) {
    return {
      statusCode: 404,
      headers: { "content-type": "application/json" },
      body: "{}",
    };
  }

  kitchenRequest.orderState = OrderState.QUALITYCHECK;
  kitchenRequest.bakeCompleteOn = new Date();
  
  await kitchenRepository.update(kitchenRequest);
  await eventPublisher.publishOrderBakedEventV1({
    orderIdentifier: kitchenRequest.orderIdentifier,
    kitchenIdentifier: kitchenRequest.kitchenRequestId,
  });

  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(kitchenRequest),
  };
};
