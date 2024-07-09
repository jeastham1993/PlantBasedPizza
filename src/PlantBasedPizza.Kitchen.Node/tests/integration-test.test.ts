import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { EventBridgeClient, PutEventsCommand } from "@aws-sdk/client-eventbridge";
import { v4 as uuidv4 } from "uuid";
import { KitchenRequestRepository } from "../src/adapters/kitchenRepository";

const version = process.env.COMMIT_HASH ?? "latest";
const eventBridgeClient = new EventBridgeClient();
const dynamoDbClient = new DynamoDBClient();
const kitchenRepository = new KitchenRequestRepository(dynamoDbClient, `kitchen-integration-test.${version}`)
const busName = `kitchen-service.${version}`;

describe("EventHandling", () => {
  test("WhenOrderConfirmedEventPublishes_ShouldCreateNewKitchenRequest", async () => {
    //Arrange
    const orderIdentifier = uuidv4();
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
          EventBusName: busName,
          Source: "https://orders.plantbasedpizza/",
          Time: new Date(),
        },
      ],
    });

    const ebResult = await eventBridgeClient.send(command);

    // TODO: This could be a flaky test, if takes longer than 3 seconds.
    // TODO: Implement a while loop to retry a few times to allow for slow throughput
    await sleep(6000);

    const orders = await kitchenRepository.getNew();

    const orderInDatabase = orders.some(order => order.orderIdentifier === orderIdentifier);

    //Assert
    expect(orderInDatabase).toBe(true);
  }, 15000);
});

async function sleep(ms: number): Promise<void> {
  return new Promise(
      (resolve) => setTimeout(resolve, ms));
}