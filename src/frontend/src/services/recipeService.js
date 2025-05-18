import axios from "axios";

const recipeApi = axios.create({
  baseURL: "http://localhost:49536/recipes",
});

const recipeService = {
  listRecipes: async function () {
    const response = await recipeApi.get('/');

    console.log(response);

    return response.data;
  },
};

export default recipeService;
