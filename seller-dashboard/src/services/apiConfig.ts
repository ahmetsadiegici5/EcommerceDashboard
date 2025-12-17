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

// Request interceptor - Token refresh için
api.interceptors.request.use(
    (config) => {
        // İstek başlamadan önce yapılacak işlemler
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor - 401 kontrolü ve hata işleme
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;
        
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            console.warn('401 Unauthorized hatası alındı. Oturum yenileniyor...');
            
            // Token yenileme işlemi AuthContext'te yapılıyor
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