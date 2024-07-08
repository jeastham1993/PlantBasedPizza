import axios from "axios";
import { IRecipeService } from "../services/recipeService";
import { RecipeAdapter } from "./recipeAdapter";

export class RecipeService implements IRecipeService {
    async getRecipe(recipeIdentifier: string): Promise<RecipeAdapter | undefined> {
        try{
            const endpoint = `${process.env['RECIPE_API_ENDPOINT']}/recipes/${recipeIdentifier}`;

            console.log(`Querying '${endpoint}'`);
    
            const receipeResult = await axios.get<RecipeAdapter>(endpoint);
    
            console.log(`Result: ${receipeResult.status}`);
    
            return receipeResult.data;
        }
        catch (e) {
            console.log(e);

            return undefined;
        }
    }
}