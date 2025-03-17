// src/axiosConfig.js
import axios from 'axios';

export const recipeApi = axios.create({
  baseURL: 'https://api.dev.plantbasedpizza.net/recipes',
});

export const ordersApi = axios.create({
    baseURL: 'https://api.dev.plantbasedpizza.net/order',
    //baseURL: 'http://localhost:5004/order',
  });

  export const ordersAdminApi = axios.create({
      baseURL: 'https://api.dev.plantbasedpizza.net/order',
      //baseURL: 'http://localhost:5004/order',
    });

  export const kitchenApi = axios.create({
    baseURL: 'https://api.dev.plantbasedpizza.net/kitchen',
    //baseURL: 'http://localhost:5004/order',
  });

ordersApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

kitchenApi.interceptors.request.use((config) => {
  const staffToken = localStorage.getItem('staffToken');
  if (staffToken) {
    config.headers.Authorization = `Bearer ${staffToken}`;
  }
  return config;
});

ordersAdminApi.interceptors.request.use((config) => {
  const staffToken = localStorage.getItem('staffToken');
  if (staffToken) {
    config.headers.Authorization = `Bearer ${staffToken}`;
  }
  return config;
});
