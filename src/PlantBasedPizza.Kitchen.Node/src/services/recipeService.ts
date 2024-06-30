import { RecipeAdapter } from "../adapters/recipeAdapter";

export interface IRecipeService {
    getRecipe(recipeIdentifier: string): Promise<RecipeAdapter | undefined>;
}