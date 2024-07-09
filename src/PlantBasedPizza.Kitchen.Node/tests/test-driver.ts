import {
  CloudFormationClient,
  DescribeStacksCommand,
  DescribeStacksCommandInput,
} from "@aws-sdk/client-cloudformation";
import { EventBridgeClient, PutEventsCommand } from "@aws-sdk/client-eventbridge";
import axios from "axios";

export class TestDriver {
  eventBridgeClient: EventBridgeClient;
  cloudFormationClient: CloudFormationClient;
  apiUrl: string | undefined;
  version: string;
  busName: string;

  constructor(version: string, eventBridgeClient: EventBridgeClient, cloudFormationClient: CloudFormationClient) {
    this.version = version;
    this.busName = `kitchen-service.${version}`;
    this.eventBridgeClient = eventBridgeClient;
    this.cloudFormationClient = cloudFormationClient;
  }

  async publishOrderConfirmedEvent(orderIdentifier: string) {
    if (this.apiUrl === undefined) {
      await this.getStackDetails(`KitchenTestStack-${this.version}`);
    }

    const eventToPublish = {
      specversion: "1.0",
      type: "order.orderConfirmed.v1",
      source: "https://orders.plantbasedpizza/",
      id: "7612394874",
      time: "2024-07-01T14:00:00Z",
      datacontenttype: "application/json",
      data: {
        OrderIdentifier: orderIdentifier,
        Items: [
          {
            ItemName: "Fanta",
            RecipeIdentifier: "6",
          },
        ],
      },
    };

    //Act
    const command = new PutEventsCommand({
      Entries: [
        {
          Detail: JSON.stringify(eventToPublish),
          DetailType: "order.orderConfirmed.v1",
          EventBusName: this.busName,
          Source: "https://orders.plantbasedpizza/",
          Time: new Date(),
        },
      ],
    });

    await this.eventBridgeClient.send(command);
  }

  public async getNewOrders(): Promise<KitchenRequestDTO[]> {
    const newOrders = (await axios.get<KitchenRequestDTO[]>(`${this.apiUrl}/new`)).data;

    return newOrders;
  }

  private async getStackDetails(stackName: string) {
    if (this.apiUrl !== undefined) {
      return;
    }

    const input: DescribeStacksCommandInput = {
      StackName: stackName,
    };
    const command = new DescribeStacksCommand(input);
    const stackObj = await this.cloudFormationClient.send(command);

    this.apiUrl = stackObj?.Stacks?.[0]?.Outputs?.find((item) => item.ExportName === "ApiUrl")?.OutputValue;

    if (!this.apiUrl) {
      throw new Error("Missing required resources");
    }
  }
}

export interface KitchenRequestDTO {
  orderIdentifier: String;
}
