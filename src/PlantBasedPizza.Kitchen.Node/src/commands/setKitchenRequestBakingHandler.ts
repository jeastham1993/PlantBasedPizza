import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";

export interface SetKitchenRequestBakingCommand {
  orderIdentifier: string;
}

export interface SetKitchenRequestBakingReponse {
  kitchenRequest: KitchenRequest;
}

export class SetKitchenRequestBakingCommandHandler {
  kitchenRepository: IKitchenRequestRepository;
  eventPublisher: IKitchenEventPublisher;

  constructor(kitchenRepository: IKitchenRequestRepository, eventPublisher: IKitchenEventPublisher) {
    this.kitchenRepository = kitchenRepository;
    this.eventPublisher = eventPublisher;
  }

  async handle(command: SetKitchenRequestBakingCommand): Promise<SetKitchenRequestBakingReponse | null> {
    const kitchenRequest = await this.kitchenRepository.retrieve(command.orderIdentifier);

    if (kitchenRequest === null) {
      return null;
    }

    kitchenRequest.orderState = OrderState.BAKING;
    kitchenRequest.prepCompleteOn = new Date();

    await this.kitchenRepository.update(kitchenRequest);
    await this.eventPublisher.publishOrderPrepCompleteEventV1({
      orderIdentifier: kitchenRequest.orderIdentifier,
      kitchenIdentifier: kitchenRequest.kitchenRequestId,
    });

    return {
      kitchenRequest,
    };
  }
}
