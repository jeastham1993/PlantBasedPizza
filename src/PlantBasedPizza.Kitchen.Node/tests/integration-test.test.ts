import { EventBridgeClient } from "@aws-sdk/client-eventbridge";
import { CloudFormationClient } from "@aws-sdk/client-cloudformation";
import { v4 as uuidv4 } from "uuid";
import axios from "axios";
import { TestDriver } from "./test-driver";

const version = process.env.VERSION ?? "latest";
const eventBridgeClient = new EventBridgeClient();
const cloudFormationClient = new CloudFormationClient();
const testDriver = new TestDriver(version, eventBridgeClient, cloudFormationClient);

describe("EventHandling", () => {
  test("WhenOrderConfirmedEventPublishes_ShouldCreateNewKitchenRequest", async () => {
    //Arrange
    const orderIdentifier = uuidv4();

    await testDriver.publishOrderConfirmedEvent(orderIdentifier);

    let retries = 3;
    let orderInDatabase = false;

    while (retries > 0){
      await sleep(2000);

      const newOrders = await testDriver.getNewOrders();

      orderInDatabase = newOrders.some(order => order.orderIdentifier === orderIdentifier);

      if (orderInDatabase === true){
        break;
      }

      retries--;
    }

    //Assert
    expect(orderInDatabase).toBe(true);
  }, 10000);
});

async function sleep(ms: number): Promise<void> {
  return new Promise(
      (resolve) => setTimeout(resolve, ms));
}