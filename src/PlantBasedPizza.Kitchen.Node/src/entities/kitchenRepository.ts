import { KitchenRequest } from "./kitchenRequest";

export interface IKitchenRequestRepository {
    addNew(kitchenRequest: KitchenRequest): Promise<void>;
    update(kitchenRequest: KitchenRequest): Promise<void>;
    retrieve(orderIdentifier: string): Promise<KitchenRequest | null>;
    getNew(): Promise<KitchenRequest[]>;
    getPrep(): Promise<KitchenRequest[]>;
    getBaking(): Promise<KitchenRequest[]>;
    getAwaitingQualityCheck(): Promise<KitchenRequest[]>;
}