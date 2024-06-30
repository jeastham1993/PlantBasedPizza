import { KitchenOrderConfirmedEventV1 } from "../events/kitchenOrderConfirmedV1Event";

export interface IKitchenEventPublisher {
    publishKitchenOrderConfirmedEventV1(evt: KitchenOrderConfirmedEventV1): Promise<void>;
}