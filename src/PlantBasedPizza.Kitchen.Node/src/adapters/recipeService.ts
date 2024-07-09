import axios from "axios";
import { IRecipeService } from "../services/recipeService";
import { RecipeAdapter } from "./recipeAdapter";

export class RecipeService implements IRecipeService {
    async getRecipe(recipeIdentifier: string): Promise<RecipeAdapter | undefined> {
        try{
            if (process.env.INTEGRATION_TEST_RUN === "true"){
                return {
                    id: recipeIdentifier,
                    name: 'Test recipe',
                    category: 'Pizza',
                    price: 5,
                    ingredients:[{
                        name: 'test',
                        quantity: 1
                    }]
                }
            }
            
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