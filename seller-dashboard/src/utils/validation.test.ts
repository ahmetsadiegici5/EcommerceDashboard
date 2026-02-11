import { describe, expect, it } from 'vitest';
import { loginValidationSchema, productValidationSchema, validateField, validateForm } from './validation';

describe('validateField', () => {
  it('zorunlu alan boşsa mesaj döner', () => {
    expect(validateField('', [{ required: true, message: 'Zorunlu' }])).toBe('Zorunlu');
  });

  it('zorunlu olmayan boş alanda diğer kuralları atlar', () => {
    expect(validateField('', [{ minLength: 3, message: 'Kısa' }])).toBeNull();
  });

  it('sayısal sınırları kontrol eder', () => {
    const rules = [{ min: 1, max: 10, message: 'Aralık dışı' }];
    expect(validateField(0, rules)).toBe('Aralık dışı');
    expect(validateField(5, rules)).toBeNull();
  });
});

describe('validateForm', () => {
  it('geçerli ürünü kabul eder', () => {
    const { isValid } = validateForm(
      { name: 'Kalem', price: 12.5, stock: 3, category: 'Kırtasiye', sku: 'KLM-01', imageUrl: 'https://ornek.com/a.png' },
      productValidationSchema,
    );
    expect(isValid).toBe(true);
  });

  it('hatalı alanları tek tek raporlar', () => {
    const { isValid, errors } = validateForm(
      { name: 'K', price: 0, stock: -1, category: '', sku: 'boşluk var' },
      productValidationSchema,
    );
    expect(isValid).toBe(false);
    expect(errors.name).toBe('Ürün adı en az 2 karakter olmalıdır');
    expect(errors.price).toBe("Fiyat 0'dan büyük olmalıdır");
    expect(errors.stock).toBe("Stok 0'dan küçük olamaz");
    expect(errors.category).toBe('Kategori zorunludur');
    expect(errors.sku).toBe('SKU sadece harf, rakam, tire ve alt çizgi içerebilir');
  });

  it('giriş formunda e-posta biçimini kontrol eder', () => {
    const { errors } = validateForm({ email: 'gecersiz', password: '123456' }, loginValidationSchema);
    expect(errors.email).toBe('Geçerli bir e-posta adresi giriniz');
    expect(errors.password).toBeNull();
  });
});
