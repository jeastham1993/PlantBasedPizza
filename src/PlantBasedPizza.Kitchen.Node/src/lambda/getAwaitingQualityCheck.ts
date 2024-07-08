import { ALBEvent, ALBResult } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { getParameter } from "@aws-lambda-powertools/parameters/ssm";
import { tracer } from "dd-trace";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { Authorizer } from "../authorization/authorizer";

tracer.init();

const secretKey = getParameter(process.env.JWT_SSM_PARAM!, {
  decrypt: true
});

const authorizer: Authorizer = new Authorizer(secretKey);

const dynamoDbClient = new DynamoDBClient();

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

  const request = await kitchenRepository.getAwaitingQualityCheck();
  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(request),
  };
};
