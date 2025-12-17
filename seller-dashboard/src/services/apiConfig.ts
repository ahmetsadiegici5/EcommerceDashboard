import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5039/api';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
    timeout: 30000, // 30 saniye timeout
});

// Request interceptor - Token ve Cookie gönderme
api.interceptors.request.use(
    (config) => {
        // HttpOnly cookie otomatik gönderilecek (withCredentials: true)
        // Token gerekirse Firebase Client SDK'den alınıp gönderileceğiz
        console.debug(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor - 401 kontrolü ve hata işleme
api.interceptors.response.use(
    (response) => {
        console.debug(`API Response: ${response.status} ${response.config.url}`);
        return response;
    },
    async (error) => {
        const originalRequest = error.config;
        console.error(`API Error: ${error.response?.status} ${error.config?.url}`, error.response?.data);
        
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            console.warn('401 Unauthorized hatası alındı. Login sayfasına yönlendiriliyor...');
            
            // Kullanıcıyı login sayfasına yönlendir
            if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }
        
        // Hata mesajını standartlaştır
        const errorMessage = error.response?.data?.message 
            || error.response?.data?.Message 
            || error.message 
            || 'Beklenmeyen bir hata oluştu';
            
        return Promise.reject({ ...error, message: errorMessage });
    }
);