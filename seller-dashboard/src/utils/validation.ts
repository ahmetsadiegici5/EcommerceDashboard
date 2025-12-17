// Form validation utilities for frontend
export interface ValidationRule {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: RegExp;
  custom?: (value: unknown) => boolean;
  message: string;
}

export interface ValidationSchema {
  [field: string]: ValidationRule[];
}

export interface ValidationErrors {
  [field: string]: string | null;
}

export function validateField(value: unknown, rules: ValidationRule[]): string | null {
  for (const rule of rules) {
    // Required check
    if (rule.required && (value === undefined || value === null || value === '')) {
      return rule.message;
    }

    // Skip other validations if value is empty and not required
    if (value === undefined || value === null || value === '') {
      continue;
    }

    // String validations
    if (typeof value === 'string') {
      if (rule.minLength && value.length < rule.minLength) {
        return rule.message;
      }
      if (rule.maxLength && value.length > rule.maxLength) {
        return rule.message;
      }
      if (rule.pattern && !rule.pattern.test(value)) {
        return rule.message;
      }
    }

    // Number validations
    if (typeof value === 'number') {
      if (rule.min !== undefined && value < rule.min) {
        return rule.message;
      }
      if (rule.max !== undefined && value > rule.max) {
        return rule.message;
      }
    }

    // Custom validation
    if (rule.custom && !rule.custom(value)) {
      return rule.message;
    }
  }

  return null;
}

export function validateForm<T extends Record<string, unknown>>(
  data: T,
  schema: ValidationSchema
): { isValid: boolean; errors: ValidationErrors } {
  const errors: ValidationErrors = {};
  let isValid = true;

  for (const [field, rules] of Object.entries(schema)) {
    const error = validateField(data[field], rules);
    errors[field] = error;
    if (error) {
      isValid = false;
    }
  }

  return { isValid, errors };
}

// Predefined validation schemas
export const productValidationSchema: ValidationSchema = {
  name: [
    { required: true, message: 'Ürün adı zorunludur' },
    { minLength: 2, message: 'Ürün adı en az 2 karakter olmalıdır' },
    { maxLength: 200, message: 'Ürün adı en fazla 200 karakter olabilir' },
  ],
  price: [
    { required: true, message: 'Fiyat zorunludur' },
    { min: 0.01, message: 'Fiyat 0\'dan büyük olmalıdır' },
  ],
  stock: [
    { required: true, message: 'Stok zorunludur' },
    { min: 0, message: 'Stok 0\'dan küçük olamaz' },
  ],
  category: [
    { required: true, message: 'Kategori zorunludur' },
  ],
  sku: [
    { required: true, message: 'SKU zorunludur' },
    { pattern: /^[A-Za-z0-9-_]+$/, message: 'SKU sadece harf, rakam, tire ve alt çizgi içerebilir' },
  ],
  description: [
    { maxLength: 2000, message: 'Açıklama en fazla 2000 karakter olabilir' },
  ],
  imageUrl: [
    { 
      pattern: /^(https?:\/\/)?[\w\-]+(\.[\w\-]+)+[/#?]?.*$/, 
      message: 'Geçerli bir URL giriniz' 
    },
  ],
};

export const orderValidationSchema: ValidationSchema = {
  customerName: [
    { required: true, message: 'Müşteri adı zorunludur' },
    { minLength: 2, message: 'Müşteri adı en az 2 karakter olmalıdır' },
  ],
  customerEmail: [
    { required: true, message: 'E-posta zorunludur' },
    { pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Geçerli bir e-posta adresi giriniz' },
  ],
  customerPhone: [
    { required: true, message: 'Telefon zorunludur' },
    { pattern: /^[0-9+\-\s()]+$/, message: 'Geçerli bir telefon numarası giriniz' },
  ],
  shippingAddress: [
    { required: true, message: 'Teslimat adresi zorunludur' },
    { minLength: 10, message: 'Adres en az 10 karakter olmalıdır' },
  ],
};

export const loginValidationSchema: ValidationSchema = {
  email: [
    { required: true, message: 'E-posta zorunludur' },
    { pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Geçerli bir e-posta adresi giriniz' },
  ],
  password: [
    { required: true, message: 'Şifre zorunludur' },
    { minLength: 6, message: 'Şifre en az 6 karakter olmalıdır' },
  ],
};
