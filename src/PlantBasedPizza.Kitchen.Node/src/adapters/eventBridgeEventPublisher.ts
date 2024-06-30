import { CloudEventV1 } from "cloudevents";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { KitchenOrderConfirmedEventV1 } from "../events/kitchenOrderConfirmedV1Event";
import { EventBridgeClient, PutEventsCommand } from "@aws-sdk/client-eventbridge";
import { v4 as uuidv4 } from "uuid";
import { tracer, Tracer, TracerProvider } from "dd-trace";
const { getTraceHeaders } = require("datadog-lambda-js");

export class EventBridgeEventPublisher implements IKitchenEventPublisher {
  private eventBridgeClient: EventBridgeClient;

  constructor(eventBridgeClient: EventBridgeClient) {
    this.eventBridgeClient = eventBridgeClient;
  }

  async publishKitchenOrderConfirmedEventV1(evt: KitchenOrderConfirmedEventV1): Promise<void> {
    const currentSpan = tracer.scope().active();

    const ce: CloudEventV1<KitchenOrderConfirmedEventV1> = {
      specversion: "1.0",
      source: "https://kitchen.plantbasedpizza",
      type: "kitchen.orderConfirmed.v1",
      id: uuidv4(),
      time: new Date().toISOString(),
      datacontenttype: "application/json",
      data: evt,
      ddtraceid: currentSpan?.context().toTraceId(),
      ddspanid: currentSpan?.context().toSpanId(),
    };

    const span = tracer.scope().active()!;
    const datadog = {};

    tracer.inject(span, 'text_map', datadog);

    console.log(datadog);

    console.log(JSON.stringify(ce));

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

    const ebResult = this.eventBridgeClient.send(command);
  }
}
