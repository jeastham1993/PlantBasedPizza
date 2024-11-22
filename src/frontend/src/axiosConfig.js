// src/axiosConfig.js
import axios from "axios";

export const base_url = "http://localhost:5051";

export const accountApi = axios.create({
  baseURL: `${base_url}/account`,
});

export const recipeApi = axios.create({
  baseURL: `${base_url}/recipes`,
});

export const ordersApi = axios.create({
  baseURL: `${base_url}/order`,
});

export const ordersAdminApi = axios.create({
  baseURL: `${base_url}/order`,
});

export const kitchenApi = axios.create({
  baseURL: `${base_url}/kitchen`,
});

ordersApi.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

kitchenApi.interceptors.request.use((config) => {
  const staffToken = localStorage.getItem("staffToken");
  if (staffToken) {
    config.headers.Authorization = `Bearer ${staffToken}`;
  }
  return config;
});

ordersAdminApi.interceptors.request.use((config) => {
  const staffToken = localStorage.getItem("staffToken");
  if (staffToken) {
    config.headers.Authorization = `Bearer ${staffToken}`;
  }
  return config;
});
