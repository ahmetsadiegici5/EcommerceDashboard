export interface Product {
  id?: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  sku: string;
  imageUrl?: string;
  sellerId: string;
  createdAt?: Date;
  updatedAt?: Date;
  isActive: boolean;
}

export interface Order {
  id?: string;
  orderNumber: string;
  sellerId: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  items: OrderItem[];
  totalAmount: number;
  status: string;
  shippingAddress: string;
  trackingNumber?: string;
  orderDate: Date;
  shippedDate?: Date;
  deliveredDate?: Date;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Shipping {
  id?: string;
  orderId: string;
  trackingNumber: string;
  carrier: string;
  status: string;
  estimatedDelivery?: Date;
  actualDelivery?: Date;
  shippingAddress: string;
  recipientName: string;
  recipientPhone: string;
  events: ShippingEvent[];
}

export interface ShippingEvent {
  timestamp: Date;
  status: string;
  location: string;
  description: string;
}

export interface DashboardStats {
  totalProducts: number;
  totalOrders: number;
  totalRevenue: number;
  lowStockProducts: number;
}
