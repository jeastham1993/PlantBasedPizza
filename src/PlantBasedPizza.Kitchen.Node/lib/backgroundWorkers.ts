import { Construct } from "constructs";
import { SharedProps } from "./constructs/sharedFunctionProps";
import { IEventBus } from "aws-cdk-lib/aws-events";
import { ITable } from "aws-cdk-lib/aws-dynamodb";
import { EventQueue } from "./constructs/eventQueue";
import { InstrumentedSqsLambdaFunction } from "./constructs/sqsLambdaFunction";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";

export interface BackgroundWorkerProps {
  sharedProps: SharedProps;
  bus: IEventBus;
  table: ITable;
}

export class BackgroundWorker extends Construct {
  constructor(scope: Construct, id: string, props: BackgroundWorkerProps) {
    super(scope, id);
    const orderSubmittedQueueName = "Kitchen-OrderSubmitted";

    var queue = new EventQueue(this, "OrderSubmittedEventQueue", {
      sharedProps: props.sharedProps,
      bus: props.bus,
      queueName: orderSubmittedQueueName,
      eventSource: "https://orders.plantbasedpizza/",
      detailType: "order.orderConfirmed.v1"
    });

    const orderConfirmedHandler = new InstrumentedSqsLambdaFunction(this, "HandleOrderConfirmedEvent", {
        sharedProps: props.sharedProps,
      entry: "./src/lambda/handleOrderConfirmedEvent.ts",
      functionName: "HandleOrderConfirmedEvent",
    });

    orderConfirmedHandler.function.addEventSource(new SqsEventSource(queue.queue));

    orderConfirmedHandler.function.addEnvironment("BUS_NAME", props.bus.eventBusName);
    props.bus.grantPutEventsTo(orderConfirmedHandler.function);
    
    props.table.grantReadWriteData(orderConfirmedHandler.function);
  }
}
