import { Collection, MongoClient } from "mongodb";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";

export class KitchenRequestRepository implements IKitchenRequestRepository {
  kitchenRequestCollection: Collection<KitchenRequest>;

  constructor(mongoConnectionString: string, database: string, collection: string) {
    const client = new MongoClient(mongoConnectionString);
    const db = client.db(database);
    this.kitchenRequestCollection = db.collection<KitchenRequest>(collection);
  }

  async addNew(kitchenRequest: KitchenRequest): Promise<void> {
    console.log(`Storing: ${kitchenRequest.orderIdentifier}`);
    
    await this.kitchenRequestCollection.insertOne(kitchenRequest);
  }
  async update(kitchenRequest: KitchenRequest): Promise<void> {
    await this.kitchenRequestCollection.findOneAndReplace({ OrderIdentifier: kitchenRequest.orderIdentifier }, kitchenRequest);
  }
  async retrieve(orderIdentifier: string): Promise<KitchenRequest> {
    const result = await this.kitchenRequestCollection.findOne({ OrderIdentifier: orderIdentifier });

    return result!;
  }
  async getNew(): Promise<KitchenRequest[]> {
    const result = await this.kitchenRequestCollection.find({OrderState: OrderState.NEW})!;

    return result.toArray();
  }
  async getPrep(): Promise<KitchenRequest[]> {
    const result = await this.kitchenRequestCollection.find({OrderState: OrderState.PREPARING})!;

    return result.toArray();
  }
  async getBaking(): Promise<KitchenRequest[]> {
    const result = await this.kitchenRequestCollection.find({OrderState: OrderState.BAKING})!;

    return result.toArray();
  }
  async getAwaitingQualityCheck(): Promise<KitchenRequest[]> {
    const result = await this.kitchenRequestCollection.find({OrderState: OrderState.QUALITYCHECK})!;

    return result.toArray();
  }
}
