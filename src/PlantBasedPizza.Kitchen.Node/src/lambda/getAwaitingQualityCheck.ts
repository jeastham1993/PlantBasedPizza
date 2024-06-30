import { ALBEvent, ALBResult } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { getParameter } from "@aws-lambda-powertools/parameters/ssm";
import { tracer } from "dd-trace";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";

tracer.init();

const dynamoDbClient = new DynamoDBClient();

var kitchenRepository = new KitchenRequestRepository(dynamoDbClient, process.env.TABLE_NAME!);

export const handler = async (event: ALBEvent): Promise<ALBResult> => {
  const request = await kitchenRepository.getAwaitingQualityCheck();
  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(request),
  };
};
