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
import { SetKitchenRequestCompleteCommand, SetKitchenRequestCompleteCommandHandler } from "../commands/setKitchenRequestCompleteHandler";

tracer.init();

const secretKey = getParameter(process.env.JWT_SSM_PARAM!, {
  decrypt: true
});

const authorizer: Authorizer = new Authorizer(secretKey);

const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();

const eventPublisher = new EventBridgeEventPublisher(eventBridgeClient);
const kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);

const commandHandler = new SetKitchenRequestCompleteCommandHandler(kitchenRepository, eventPublisher);

export const handler = async (event: ALBEvent): Promise<ALBResult> => {
  const isAuthorized = await authorizer.authorizeRequest(event, ["staff", "admin"]);

  if (!isAuthorized) {
    return {
      statusCode: 401,
      headers: { "content-type": "application/json" },
      body: "{}",
    };
  }

  const parsedBody: SetKitchenRequestCompleteCommand = JSON.parse(event.body!);

  const result = await commandHandler.handle(parsedBody);

  if (result === null) {
    return {
      statusCode: 404,
      headers: { "content-type": "application/json" },
      body: "{}",
    };
  }

  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(result.kitchenRequest),
  };
};
