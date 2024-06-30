import { KitchenRequest } from "./kitchenRequest";

export interface IKitchenRequestRepository {
    addNew(kitchenRequest: KitchenRequest): Promise<void>;
    update(kitchenRequest: KitchenRequest): Promise<void>;
    retrieve(orderIdentifier: string): Promise<KitchenRequest>;
    getNew(orderIdentifier: string): Promise<KitchenRequest[]>;
    getPrep(orderIdentifier: string): Promise<KitchenRequest[]>;
    getBaking(orderIdentifier: string): Promise<KitchenRequest[]>;
    getAwaitingQualityCheck(orderIdentifier: string): Promise<KitchenRequest[]>;
}