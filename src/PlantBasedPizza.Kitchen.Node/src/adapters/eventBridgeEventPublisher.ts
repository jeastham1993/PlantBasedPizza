import { CloudEventV1 } from "cloudevents";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { KitchenOrderConfirmedEventV1 } from "../events/kitchenOrderConfirmedV1Event";
import { EventBridgeClient, PutEventsCommand } from "@aws-sdk/client-eventbridge";
import { tracer } from "dd-trace";
import { OrderBakedEventV1 } from "../events/orderBakedEventV1";
import { OrderPrepCompleteEventV1 } from "../events/orderPrepCompleteEventV1";
import { OrderPreparingEventV1 } from "../events/orderPreparingEventV1";
import { OrderQualityCheckedEventV1 } from "../events/orderQualityCheckedEventV1";
import { v4 as uuidv4 } from 'uuid';

const { FORMAT_HTTP_HEADERS } = require('opentracing')
const { getTraceHeaders } = require("datadog-lambda-js");

export class EventBridgeEventPublisher implements IKitchenEventPublisher {
  private eventBridgeClient: EventBridgeClient;

  constructor(eventBridgeClient: EventBridgeClient) {
    this.eventBridgeClient = eventBridgeClient;
  }
  async publishOrderBakedEventV1(evt: OrderBakedEventV1): Promise<void> {
    await this.publish("kitchen.orderBaked.v1", evt);
  }
  async publishOrderPrepCompleteEventV1(evt: OrderPrepCompleteEventV1): Promise<void> {
    await this.publish("kitchen.orderPrepComplete.v1", evt);
  }
  async publishOrderPreparingEventV1(evt: OrderPreparingEventV1): Promise<void> {
    await this.publish("kitchen.orderPreparing.v1", evt);
  }
  async publishOrderQualityCheckedEventV1(evt: OrderQualityCheckedEventV1): Promise<void> {
    await this.publish("kitchen.qualityChecked.v1", evt);
  }

  async publishKitchenOrderConfirmedEventV1(evt: KitchenOrderConfirmedEventV1): Promise<void> {
    await this.publish("kitchen.orderConfirmed.v1", evt);
  }

  async publish<T>(evtType: string, evtData: T) {
    const currentSpan = tracer.scope().active();

    const span= tracer.scope().active()!;
    const headers = {};

    tracer.inject(span, FORMAT_HTTP_HEADERS, headers);

    const ce: CloudEventV1<T> = {
      specversion: "1.0",
      source: "https://kitchen.plantbasedpizza/",
      type: evtType,
      id: uuidv4(),
      time: new Date().toISOString(),
      datacontenttype: "application/json",
      data: evtData,
      ddtraceid: currentSpan?.context().toTraceId(),
      ddspanid: currentSpan?.context().toSpanId(),
      traceparent: currentSpan?.context().toTraceparent(),
    };

    currentSpan?.addTags({
      "messaging.eventId": ce.id,
      "messaging.eventType": ce.type,
      "messaging.eventSource": ce.source,
      "messaging.busName": process.env.BUS_NAME,
    });

    const command = new PutEventsCommand({
      Entries: [
        {
          Detail: JSON.stringify(ce),
          DetailType: ce.type,
          EventBusName: process.env.BUS_NAME,
          Source: ce.source,
          Time: new Date(),
        },
      ],
    });

    const ebResult = await this.eventBridgeClient.send(command);

    currentSpan?.addTags({"messaging.failedEvents": ebResult.FailedEntryCount})
  }
}
