import { ALBEvent, ALBResult } from "aws-lambda";
import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { getParameter } from "@aws-lambda-powertools/parameters/ssm";
import { tracer } from "dd-trace";

tracer.init();

var kitchenRepository: KitchenRequestRepository | undefined = undefined;

export const handler = async (event: ALBEvent): Promise<ALBResult> => {
  if (kitchenRepository === undefined) {
    console.log('Initializating');
    const mongoConnectionString = await getParameter(process.env.CONN_STRING_PARAM!, {
      decrypt: true
    });
    kitchenRepository = new KitchenRequestRepository(mongoConnectionString!, "PlantBasedPizza", "kitchen");
  }
  const request = await kitchenRepository.getBaking();
  return {
    statusCode: 200,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(request),
  };
};
