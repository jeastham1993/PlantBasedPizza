import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";

export interface SetKitchenRequestQualityCheckCommand {
  orderIdentifier: string;
}

export interface SetKitchenRequestQualityCheckReponse {
  kitchenRequest: KitchenRequest;
}

export class SetKitchenRequestQualityCheckCommandHandler {
  kitchenRepository: IKitchenRequestRepository;
  eventPublisher: IKitchenEventPublisher;

  constructor(kitchenRepository: IKitchenRequestRepository, eventPublisher: IKitchenEventPublisher) {
    this.kitchenRepository = kitchenRepository;
    this.eventPublisher = eventPublisher;
  }

  async handle(command: SetKitchenRequestQualityCheckCommand): Promise<SetKitchenRequestQualityCheckReponse | null> {
    const kitchenRequest = await this.kitchenRepository.retrieve(command.orderIdentifier);

    if (kitchenRequest === null) {
      return null;
    }

    kitchenRequest.orderState = OrderState.QUALITYCHECK;
    kitchenRequest.bakeCompleteOn = new Date();

    await this.kitchenRepository.update(kitchenRequest);
    await this.eventPublisher.publishOrderBakedEventV1({
      orderIdentifier: kitchenRequest.orderIdentifier,
      kitchenIdentifier: kitchenRequest.kitchenRequestId,
    });

    return {
      kitchenRequest,
    };
  }
}
