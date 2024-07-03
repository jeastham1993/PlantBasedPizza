import axios from "axios";

const recipeApi = axios.create({
  baseURL: "http://localhost:8080/recipes",
});

const recipeService = {
  listRecipes: async function () {
    const response = await recipeApi.get('/');

    return response.data;
  },
};

export default recipeService;
