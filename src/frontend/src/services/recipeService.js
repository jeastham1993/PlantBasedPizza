import axios from "axios";
import { recipeApi } from "../axiosConfig";

const recipeService = {
  listRecipes: async function () {
    const response = await recipeApi.get('/');

    return response.data;
  },
};

export default recipeService;
