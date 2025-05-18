// src/axiosConfig.js
import axios from "axios";

export const recipeApi = axios.create({
  baseURL: "http://localhost:49536/recipes",
});

export const ordersApi = axios.create({
  baseURL: "http://localhost:49536/order",
});

export const ordersAdminApi = axios.create({
  baseURL: "http://localhost:49536/order",
});

export const kitchenApi = axios.create({
  baseURL: "http://localhost:49536/kitchen",
});
