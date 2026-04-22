import axios from "axios";

// Create a reusable Axios instance for all API requests
const api = axios.create({
  baseURL: "http://localhost:5041/api",
  timeout: 10000,
});

// Automatically attach a token if it exists
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Basic response error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("API error:", error);
    return Promise.reject(error);
  }
);

export default api;