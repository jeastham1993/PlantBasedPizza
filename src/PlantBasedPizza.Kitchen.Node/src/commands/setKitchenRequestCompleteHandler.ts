import { KitchenRequestRepository } from "../adapters/kitchenRepository";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest, OrderState } from "../entities/kitchenRequest";

export interface SetKitchenRequestCompleteCommand {
  orderIdentifier: string;
}

export interface SetKitchenRequestCompleteReponse {
  kitchenRequest: KitchenRequest;
}

export class SetKitchenRequestCompleteCommandHandler {
  kitchenRepository: IKitchenRequestRepository;
  eventPublisher: IKitchenEventPublisher;

  constructor(kitchenRepository: IKitchenRequestRepository, eventPublisher: IKitchenEventPublisher) {
    this.kitchenRepository = kitchenRepository;
    this.eventPublisher = eventPublisher;
  }

  async handle(command: SetKitchenRequestCompleteCommand): Promise<SetKitchenRequestCompleteReponse | null> {
    const kitchenRequest = await this.kitchenRepository.retrieve(command.orderIdentifier);

    if (kitchenRequest === null) {
      return null;
    }

    kitchenRequest.orderState = OrderState.DONE;
  kitchenRequest.qualityCheckCompleteOn = new Date();

    await this.kitchenRepository.update(kitchenRequest);
    await this.eventPublisher.publishOrderQualityCheckedEventV1({
      orderIdentifier: kitchenRequest.orderIdentifier,
      kitchenIdentifier: kitchenRequest.kitchenRequestId,
    });

    return {
      kitchenRequest,
    };
  }
}
