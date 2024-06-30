import { RecipeAdapter } from "../adapters/recipeAdapter";
import { IKitchenEventPublisher } from "../entities/kitchenEventPublisher";
import { IKitchenRequestRepository } from "../entities/kitchenRepository";
import { KitchenRequest } from "../entities/kitchenRequest";
import { OrderConfirmedEvent } from "../integration-events/orderConfirmedEvent";
import { IRecipeService } from "../services/recipeService";

export class OrderConfirmedEventHandler {
  kitchenRequestRepository: IKitchenRequestRepository;
  eventPublisher: IKitchenEventPublisher;
  recipeService: IRecipeService;

  constructor(
    kitchenRequestRepository: IKitchenRequestRepository,
    eventPublisher: IKitchenEventPublisher,
    recipeService: IRecipeService,
  ) {
    this.kitchenRequestRepository = kitchenRequestRepository;
    this.eventPublisher = eventPublisher;
    this.recipeService = recipeService;
  }

  async handle(evt: OrderConfirmedEvent): Promise<void> {
    const recipes: RecipeAdapter[] = [];

    for (const item of evt.Items) {
      console.log('Starting loop');

      const recipe = await this.recipeService.getRecipe(item.RecipeIdentifier);

      console.log('Recipe service complete');

      if (recipe === undefined) {
        throw "Recipe not found";
      };

      recipes.push(recipe);
    }
    console.log(`Found recipe(s): ${recipes.length}`);

    const kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

    await this.kitchenRequestRepository.addNew(kitchenRequest);
    await this.eventPublisher.publishKitchenOrderConfirmedEventV1({
        orderIdentifier: evt.OrderIdentifier
    });
  }
}
