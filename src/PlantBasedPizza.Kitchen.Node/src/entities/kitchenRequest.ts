import { inherits } from "util"
import { RecipeAdapter } from "../adapters/recipeAdapter"
import { v4 as uuidv4 } from 'uuid';

export class KitchenRequest {
    kitchenRequestId: string
    orderIdentifier: string
    orderReceivedOn: Date
    orderState: OrderState
    prepCompleteOn: Date | null
    bakeCompleteOn: Date | null
    qualityCheckCompleteOn: Date | null
    recipes: RecipeAdapter[]

    constructor(orderIdentifier: string, recipes: RecipeAdapter[]) {
        this.orderIdentifier = orderIdentifier;
        this.kitchenRequestId = uuidv4();
        this.recipes = recipes;
        this.orderReceivedOn = new Date();
        this.orderState = OrderState.NEW;
    }
}

export enum OrderState {
    NEW,
    PREPARING,
    BAKING,
    QUALITYCHECK,
    DONE
}