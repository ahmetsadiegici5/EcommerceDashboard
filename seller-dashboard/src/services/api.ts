import { api } from './apiConfig';
import type { Product, Order, Shipping, DashboardStats } from '../types';

type ProductPayload = Omit<Product, 'id' | 'sellerId' | 'createdAt' | 'updatedAt'>;

// Product Service
export const productService = {
  getAll: async (): Promise<Product[]> => {
    const response = await api.get('/Products', {
      params: { page: 1, pageSize: 1000 },
    });
    return response.data;
  },

  getById: async (id: string): Promise<Product> => {
    const response = await api.get(`/Products/${id}`);
    return response.data;
  },

  search: async (q: string = '', lowStock?: number, page: number = 1, pageSize: number = 10): Promise<Product[]> => {
    const response = await api.get('/Products/search', {
      params: { q, lowStock, page, pageSize },
    });
    return response.data;
  },

  create: async (product: ProductPayload): Promise<{ id: string }> => {
    const response = await api.post('/Products', product);
    return response.data;
  },

  update: async (id: string, product: ProductPayload): Promise<void> => {
    await api.put(`/Products/${id}`, product);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/Products/${id}`);
  },

  // Excel işlemleri
  downloadTemplate: async (): Promise<Blob> => {
    const response = await api.get('/Products/template', {
      responseType: 'blob',
    });
    return response.data;
  },

  importFromExcel: async (file: File): Promise<void> => {
    const formData = new FormData();
    formData.append('file', file);
    await api.post('/Products/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  exportToExcel: async (): Promise<Blob> => {
    const response = await api.get('/Products/export', {
      responseType: 'blob',
    });
    return response.data;
  },
};

// Order Service
export const orderService = {
  getAll: async (): Promise<Order[]> => {
    const response = await api.get('/Orders', {
      params: { page: 1, pageSize: 1000 },
    });
    return response.data;
  },

  getById: async (id: string): Promise<Order> => {
    const response = await api.get(`/Orders/${id}`);
    return response.data;
  },

  create: async (order: Omit<Order, 'id'>): Promise<{ id: string }> => {
    const response = await api.post('/Orders', order);
    return response.data;
  },

  updateStatus: async (id: string, status: string): Promise<void> => {
    await api.put(`/Orders/${id}/status`, { status });
  },
};

// Shipping Service
export const shippingService = {
  getAll: async (): Promise<Shipping[]> => {
    const response = await api.get('/Shipping', {
      params: { page: 1, pageSize: 1000 },
    });
    return response.data;
  },

  getByTrackingNumber: async (trackingNumber: string): Promise<Shipping> => {
    const response = await api.get(`/Shipping/tracking/${trackingNumber}`);
    return response.data;
  },

  create: async (shipping: Omit<Shipping, 'id'>): Promise<{ id: string }> => {
    const response = await api.post('/Shipping', shipping);
    return response.data;
  },

  updateStatus: async (
    id: string,
    status: string,
    location: string,
    description: string
  ): Promise<void> => {
    await api.put(`/Shipping/${id}/status`, {
      status,
      location,
      description,
    });
  },

  importFromExcel: async (file: File): Promise<void> => {
    const formData = new FormData();
    formData.append('file', file);
    await api.post('/Shipping/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  exportToExcel: async (): Promise<Blob> => {
    const response = await api.get('/Shipping/export', {
      responseType: 'blob',
    });
    return response.data;
  },
};

// Dashboard Stats Service
export const dashboardService = {
  getStats: async (): Promise<DashboardStats> => {
    // baseURL zaten /api ile bittiği için başa / koymuyoruz
    // Böylece istek: http://localhost:5039/api/Dashboard
    const response = await api.get('Dashboard');
    return response.data;
  },
};

export default api;
