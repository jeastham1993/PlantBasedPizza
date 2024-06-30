import { RecipeAdapter } from "../adapters/recipeAdapter"

export class KitchenRequest {
    kitchenRequestId: string
    orderIdentifier: string
    orderReceivedOn: Date
    orderState: OrderState
    prepCompleteOn: Date
    bakeCompleteOn: Date
    qualityCheckCompleteOn: Date
    recipes: RecipeAdapter[]

    constructor(orderIdentifier: string, recipes: RecipeAdapter[]) {
        this.orderIdentifier = orderIdentifier;
        this.recipes = recipes;
    }

    preparing() {
        this.orderState = OrderState.PREPARING
    }

    prepComplete() {
        this.orderState = OrderState.BAKING;
        this.prepCompleteOn = new Date();
    }

    bakeComplete() {
        this.orderState = OrderState.QUALITYCHECK;
        this.bakeCompleteOn = new Date();
    }

    qualityCheckComplete() {
        this.orderState = OrderState.DONE;
        this.qualityCheckCompleteOn = new Date();
    }
}

export enum OrderState {
    NEW,
    PREPARING,
    BAKING,
    QUALITYCHECK,
    DONE
}