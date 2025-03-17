import { Construct } from "constructs";
import { SharedProps } from "./constructs/sharedFunctionProps";
import { InstrumentedApiLambdaFunction } from "./constructs/lambdaFunction";
import { IEventBus } from "aws-cdk-lib/aws-events";
import { ITable } from "aws-cdk-lib/aws-dynamodb";
import { IStringParameter, StringParameter } from "aws-cdk-lib/aws-ssm";
import { Tags } from "aws-cdk-lib";

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
      handler: "index.handler",
      buildDef: './src/lambda/buildGetNew.js',
      outDir: './out/getNew',
      functionName: "GetNewFunction",
      path: "/kitchen/new",
      method: "GET",
      priority: 36,
      jwtKey: props.jwtKey
    });
    const getPrepCompleteFunction = new InstrumentedApiLambdaFunction(this, "GetPrepCompleteFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildGetPreparing.js',
      outDir: './out/getPreparing',
      functionName: "GetPrepCompleteFunction",
      path: "/kitchen/prep",
      method: "GET",
      priority: 33,
      jwtKey: props.jwtKey
    });
    const getBakingFunction = new InstrumentedApiLambdaFunction(this, "GetBakingFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildGetBaking.js',
      outDir: './out/getBaking',
      functionName: "GetBakingFunction",
      path: "/kitchen/baking",
      method: "GET",
      priority: 34,
      jwtKey: props.jwtKey
    });
    const getAwaitingQualityCheckFunction = new InstrumentedApiLambdaFunction(this, "GetAwaitingQualityCheckFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildGetAwaitingQualityCheck.js',
      outDir: './out/getAwaitingQualityCheck',
      functionName: "GetAwaitingQualityCheckFunction",
      path: "/kitchen/quality-check",
      method: "GET",
      priority: 35,
      jwtKey: props.jwtKey
    });
    const setPreparingFunction = new InstrumentedApiLambdaFunction(this, "SetPreparingFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildSetPreparing.js',
      outDir: './out/setPreparing',
      functionName: "SetPreparingFunction",
      path: "/kitchen/preparing",
      method: "POST",
      priority: 30,
      jwtKey: props.jwtKey
    });
    const setBakingFunction = new InstrumentedApiLambdaFunction(this, "SetBakingFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildSetBaking.js',
      outDir: './out/setBaking',
      functionName: "SetBakingFunction",
      path: "/kitchen/prep-complete",
      method: "POST",
      priority: 31,
      jwtKey: props.jwtKey
    });
    const setQualityCheckingFunction = new InstrumentedApiLambdaFunction(this, "SetQualityCheckingFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildSetQualityCheck.js',
      outDir: './out/setQualityChecking',
      functionName: "SetQualityCheckingFunction",
      path: "/kitchen/bake-complete",
      method: "POST",
      priority: 32,
      jwtKey: props.jwtKey
    });
    const setDoneFunction = new InstrumentedApiLambdaFunction(this, "SetCompleteFunction", {
      sharedProps: props.sharedProps,
      handler: "index.handler",
      buildDef: './src/lambda/buildSetComplete.js',
      outDir: './out/setComplete',
      functionName: "SetCompleteFunction",
      path: "/kitchen/quality-check",
      method: "POST",
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

    Tags.of(this).add("plantbasedpizza:application", "KitchenApi");
  }
}
