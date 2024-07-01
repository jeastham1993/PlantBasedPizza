import { Construct } from "constructs";
import { NodejsFunction } from "aws-cdk-lib/aws-lambda-nodejs";
import { Runtime } from "aws-cdk-lib/aws-lambda";
import { LambdaTarget } from "aws-cdk-lib/aws-elasticloadbalancingv2-targets";
import {
  ApplicationTargetGroup,
  ListenerCondition,
  TargetType,
} from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedProps } from "./sharedFunctionProps";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { IEventBus, Rule } from "aws-cdk-lib/aws-events";
import { SqsQueue } from "aws-cdk-lib/aws-events-targets";

export interface EventQueueProps {
  sharedProps: SharedProps
  bus: IEventBus
  queueName: string
  eventSource: string
  detailType: string
}

export class EventQueue extends Construct {
  queue: Queue;
  deadLetterQueue: Queue

  constructor(scope: Construct, id: string, props: EventQueueProps) {
    super(scope, id);
    
    var eventSource = props.eventSource;

    if (!eventSource.endsWith('/')){
        eventSource = eventSource + "/"
    }

    this.deadLetterQueue = new Queue(this, `${props.queueName}DLQ-${props.sharedProps.environment}`, {
        queueName: `${props.queueName}DLQ-${props.sharedProps.environment}`
    });

    this.queue = new Queue(this, `${props.queueName}-${props.sharedProps.environment}`, {
        queueName: `${props.queueName}-${props.sharedProps.environment}`,
        deadLetterQueue: {
            maxReceiveCount: 3,
            queue: this.deadLetterQueue
        }
    });

    Tags.of(this.deadLetterQueue).add("service", props.sharedProps.serviceName);
    Tags.of(this.deadLetterQueue).add("env", props.sharedProps.environment);
    Tags.of(this.deadLetterQueue).add("version", props.sharedProps.version);
    Tags.of(this.queue).add("service", props.sharedProps.serviceName);
    Tags.of(this.queue).add("env", props.sharedProps.environment);
    Tags.of(this.queue).add("version", props.sharedProps.version);

    const rule = new Rule(this, `${props.queueName}Rule`, {
        eventBus: props.bus
    });

    rule.addEventPattern({
        source: [props.eventSource],
        detailType: [props.detailType]
    });
    rule.addTarget(new SqsQueue(this.queue));
  }
}
