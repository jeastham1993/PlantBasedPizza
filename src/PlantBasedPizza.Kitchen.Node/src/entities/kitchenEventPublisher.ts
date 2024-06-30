import { KitchenOrderConfirmedEventV1 } from "../events/kitchenOrderConfirmedV1Event";
import { OrderBakedEventV1 } from "../events/orderBakedEventV1";
import { OrderPrepCompleteEventV1 } from "../events/orderPrepCompleteEventV1";
import { OrderPreparingEventV1 } from "../events/orderPreparingEventV1";
import { OrderQualityCheckedEventV1 } from "../events/orderQualityCheckedEventV1";

export interface IKitchenEventPublisher {
    publishKitchenOrderConfirmedEventV1(evt: KitchenOrderConfirmedEventV1): Promise<void>;
    publishOrderBakedEventV1(evt: OrderBakedEventV1): Promise<void>;
    publishOrderPrepCompleteEventV1(evt: OrderPrepCompleteEventV1): Promise<void>;
    publishOrderPreparingEventV1(evt: OrderPreparingEventV1): Promise<void>;
    publishOrderQualityCheckedEventV1(evt: OrderQualityCheckedEventV1): Promise<void>;
}