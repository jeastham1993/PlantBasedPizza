import { tracer } from "dd-trace";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";
import { DynamoDBClient, PutItemCommand, GetItemCommand, QueryCommand } from "@aws-sdk/client-dynamodb";
export interface IKitchenRequest {
  kitchenRequestId: string;
  orderIdentifier: string;
  orderReceivedOn: Date;
  orderState: OrderState;
  prepCompleteOn: Date;
  bakeCompleteOn: Date;
  qualityCheckCompleteOn: Date;
  recipes: IRecipe[];
}

export interface IRecipe {
  id: string;
  name: string;
  category: string;
  price: number;
  ingredients: IRecipeItem[];
}

export interface IRecipeItem {
  name: string;
  quantity: number;
}

export class KitchenRequestRepository implements IKitchenRequestRepository {
  client: DynamoDBClient;
  tableName: string;

  constructor(client: DynamoDBClient, tableName: string) {
    this.client = client;
    this.tableName = tableName;
  }

  async addNew(kitchenRequest: KitchenRequest): Promise<void> {
    const activeSpan = tracer.scope().active();
    const item = {
      TableName: this.tableName,
      Item: {
        PK: { S: kitchenRequest.orderIdentifier },
        GSI1SK: { S: `${kitchenRequest.orderReceivedOn.toISOString()}${kitchenRequest.orderIdentifier}}` },
        GSI1PK: { S: kitchenRequest.orderState.toString() },
        OrderIdentifier: { S: kitchenRequest.orderIdentifier },
        OrderReceivedOn: { S: kitchenRequest.orderReceivedOn.toISOString() },
        KitchenRequestId: { S: kitchenRequest.kitchenRequestId },
        OrderState: { S: kitchenRequest.orderState.toString() },
        Recipes: { S: JSON.stringify(kitchenRequest.recipes) },
      },
    };

    const command = new PutItemCommand(item);
    const response = await this.client.send(command);

    activeSpan?.addTags({
      "dynamo.tableName": this.tableName,
      "dynamo.consumedRcu": response.ConsumedCapacity?.ReadCapacityUnits,
      "dynamo.consumedWcu": response.ConsumedCapacity?.WriteCapacityUnits
    })
  }

  async update(kitchenRequest: KitchenRequest): Promise<void> {
    const activeSpan = tracer.scope().active();

    const item = {
      TableName: this.tableName,
      Item: {
        PK: { S: kitchenRequest.orderIdentifier },
        GSI1SK: { S: `${kitchenRequest.orderReceivedOn.toISOString()}${kitchenRequest.orderIdentifier}}` },
        GSI1PK: { S: kitchenRequest.orderState.toString() },
        OrderReceivedOn: { S: kitchenRequest.orderReceivedOn.toISOString() },
        OrderIdentifier: { S: kitchenRequest.orderIdentifier },
        KitchenRequestId: { S: kitchenRequest.kitchenRequestId },
        OrderState: { S: kitchenRequest.orderState.toString() },
        Recipes: { S: JSON.stringify(kitchenRequest.recipes) },
      },
    };

    const command = new PutItemCommand(item);
    const response = await this.client.send(command);

    activeSpan?.addTags({
      "dynamo.tableName": this.tableName,
      "dynamo.consumedRcu": response.ConsumedCapacity?.ReadCapacityUnits,
      "dynamo.consumedWcu": response.ConsumedCapacity?.WriteCapacityUnits
    })
  }

  async retrieve(orderIdentifier: string): Promise<KitchenRequest | null> {
    const params = {
      TableName: this.tableName,
      Key: {
        PK: { S: orderIdentifier },
      },
    };

    const command = new GetItemCommand(params);
    const response = await this.client.send(command);

    if (response.Item === undefined) {
      return null;
    }

    try {
      const kitchenRequest: KitchenRequest = {
        kitchenRequestId: response.Item["KitchenRequestId"].S!,
        orderIdentifier: response.Item["OrderIdentifier"].S!,
        orderState: response.Item["OrderState"].S! as unknown as number,
        orderReceivedOn: new Date(response.Item["OrderReceivedOn"].S!),
        prepCompleteOn: null,
        bakeCompleteOn: null,
        qualityCheckCompleteOn: null,
        recipes: JSON.parse(response.Item["Recipes"].S!)
      };

      return kitchenRequest;
    }
    catch (e) {
      console.log(e);

      return null;
    }
  }

  async getNew(): Promise<KitchenRequest[]> {
    return await this.executeGsiQuery("0");
  }

  async getPrep(): Promise<KitchenRequest[]> {
    return await this.executeGsiQuery("1");
  }

  async getBaking(): Promise<KitchenRequest[]> {
    return await this.executeGsiQuery("2");
  }
  
  async getAwaitingQualityCheck(): Promise<KitchenRequest[]> {
    return await this.executeGsiQuery("3");
  }

  async executeGsiQuery(orderState: string): Promise<KitchenRequest[]> {
    const activeSpan = tracer.scope().active();
    const result: KitchenRequest[] = [];

    const params = {
      TableName: this.tableName,
      IndexName: "GSI1",
      KeyConditionExpression: "GSI1PK = :gsi1pk",
      ExpressionAttributeValues: {
        ":gsi1pk": { S: orderState },
      },
    };

    const command = new QueryCommand(params);
    const response = await this.client.send(command);

    if (response.Items === undefined) {
      throw "Query items undefined";
    }

    for (const item of response.Items){
      const kitchenRequest: KitchenRequest = {
        kitchenRequestId: item["KitchenRequestId"].S!,
        orderIdentifier: item["OrderIdentifier"].S!,
        orderState: item["OrderState"].S! as unknown as number,
        orderReceivedOn: new Date(item["OrderReceivedOn"].S!),
        prepCompleteOn: null,
        bakeCompleteOn: null,
        qualityCheckCompleteOn: null,
        recipes: JSON.parse(item["Recipes"].S!)
      };

      result.push(kitchenRequest);
    }

    activeSpan?.addTags({
      "dynamo.foundItems": response.Count,
      "dynamo.tableName": this.tableName,
      "dynamo.consumedRcu": response.ConsumedCapacity?.ReadCapacityUnits,
      "dynamo.consumedWcu": response.ConsumedCapacity?.WriteCapacityUnits
    })
    
    return result;
  }
}
