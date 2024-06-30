import { inherits } from "util"
import { RecipeAdapter } from "../adapters/recipeAdapter"
import {v4 as uuidv4} from 'uuid';
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
        this.recipes = recipes;
        this.orderReceivedOn = new Date();
        this.kitchenRequestId = uuidv4();
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