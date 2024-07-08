import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";

export interface SetKitchenRequestPreparingCommand {
  orderIdentifier: string;
}

export interface SetKitchenRequestPreparingReponse {
    kitchenRequest: KitchenRequest
  }

export class SetKitchenRequestPreparingCommandHandler {
  kitchenRepository: IKitchenRequestRepository;
  eventPublisher: IKitchenEventPublisher;

  constructor(kitchenRepository: IKitchenRequestRepository, eventPublisher: IKitchenEventPublisher) {
    this.kitchenRepository = kitchenRepository;
    this.eventPublisher = eventPublisher;
  }

  async handle(command: SetKitchenRequestPreparingCommand): Promise<SetKitchenRequestPreparingReponse | null> {
    const kitchenRequest = await this.kitchenRepository.retrieve(command.orderIdentifier);

    if (kitchenRequest === null) {
      return null;
    }

    kitchenRequest.orderState = OrderState.PREPARING;

    await this.kitchenRepository.update(kitchenRequest);
    await this.eventPublisher.publishOrderPreparingEventV1({
      orderIdentifier: kitchenRequest.orderIdentifier,
      kitchenIdentifier: kitchenRequest.kitchenRequestId,
    });

    return {
        kitchenRequest
    };
  }
}
