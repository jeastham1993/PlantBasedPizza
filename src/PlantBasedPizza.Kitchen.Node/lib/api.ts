import { Construct } from "constructs";
import { SharedProps } from "./constructs/sharedFunctionProps";
import { InstrumentedApiLambdaFunction } from "./constructs/lambdaFunction";
import { IEventBus } from "aws-cdk-lib/aws-events";
import { ITable } from "aws-cdk-lib/aws-dynamodb";
import { IStringParameter, StringParameter } from "aws-cdk-lib/aws-ssm";

export interface ApiProps {
  sharedProps: SharedProps;
  bus: IEventBus;
  table: ITable;
  jwtKey: IStringParameter
}

export class Api extends Construct {
  constructor(scope: Construct, id: string, props: ApiProps) {
    super(scope, id);

    const getNewFunction = new InstrumentedApiLambdaFunction(this, "GetNewFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/getNew.ts",
      functionName: "GetNewFunction",
      path: "/kitchen/new",
      methods: ["GET"],
      priority: 36,
      jwtKey: props.jwtKey
    });
    const getPrepCompleteFunction = new InstrumentedApiLambdaFunction(this, "GetPrepCompleteFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/getPreparing.ts",
      functionName: "GetPrepCompleteFunction",
      path: "/kitchen/prep",
      methods: ["GET"],
      priority: 33,
      jwtKey: props.jwtKey
    });
    const getBakingFunction = new InstrumentedApiLambdaFunction(this, "GetBakingFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/getBaking.ts",
      functionName: "GetBakingFunction",
      path: "/kitchen/baking",
      methods: ["GET"],
      priority: 34,
      jwtKey: props.jwtKey
    });
    const getAwaitingQualityCheckFunction = new InstrumentedApiLambdaFunction(this, "GetAwaitingQualityCheckFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/getAwaitingQualityCheck.ts",
      functionName: "GetAwaitingQualityCheckFunction",
      path: "/kitchen/quality-check",
      methods: ["GET"],
      priority: 35,
      jwtKey: props.jwtKey
    });
    const setPreparingFunction = new InstrumentedApiLambdaFunction(this, "SetPreparingFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/setPreparing.ts",
      functionName: "SetPreparingFunction",
      path: "/kitchen/preparing",
      methods: ["POST"],
      priority: 30,
      jwtKey: props.jwtKey
    });
    const setBakingFunction = new InstrumentedApiLambdaFunction(this, "SetBakingFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/setBaking.ts",
      functionName: "SetBakingFunction",
      path: "/kitchen/prep-complete",
      methods: ["POST"],
      priority: 31,
      jwtKey: props.jwtKey
    });
    const setQualityCheckingFunction = new InstrumentedApiLambdaFunction(this, "SetQualityCheckingFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/setQualityChecking.ts",
      functionName: "SetQualityCheckingFunction",
      path: "/kitchen/bake-complete",
      methods: ["POST"],
      priority: 32,
      jwtKey: props.jwtKey
    });
    const setDoneFunction = new InstrumentedApiLambdaFunction(this, "SetCompleteFunction", {
      sharedProps: props.sharedProps,
      entry: "./src/lambda/setComplete.ts",
      functionName: "SetCompleteFunction",
      path: "/kitchen/quality-check",
      methods: ["POST"],
      priority: 37,
      jwtKey: props.jwtKey
    });

    setPreparingFunction.function.addEnvironment("BUS_NAME", props.bus.eventBusName);
    props.bus.grantPutEventsTo(setPreparingFunction.function);
    setBakingFunction.function.addEnvironment("BUS_NAME", props.bus.eventBusName);
    props.bus.grantPutEventsTo(setBakingFunction.function);
    setQualityCheckingFunction.function.addEnvironment("BUS_NAME", props.bus.eventBusName);
    props.bus.grantPutEventsTo(setQualityCheckingFunction.function);
    setDoneFunction.function.addEnvironment("BUS_NAME", props.bus.eventBusName);
    props.bus.grantPutEventsTo(setDoneFunction.function);

    props.table.grantReadWriteData(setPreparingFunction.function);
    props.table.grantReadWriteData(setBakingFunction.function);
    props.table.grantReadWriteData(setQualityCheckingFunction.function);
    props.table.grantReadWriteData(setDoneFunction.function);
    props.table.grantReadData(getAwaitingQualityCheckFunction.function);
    props.table.grantReadData(getBakingFunction.function);
    props.table.grantReadData(getPrepCompleteFunction.function);
    props.table.grantReadData(getNewFunction.function);
  }
}
