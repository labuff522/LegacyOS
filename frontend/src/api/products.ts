import { http } from './http';

export type Product = { id: string; name: string; shortName: string; description?: string; productType: string;
  price: number; isSessionPackage: boolean; hasUnlimitedSessions: boolean; sessionCount?: number;
  validityDays?: number; installmentCount?: number; billingDayOfMonth?: number; isActive: boolean };
export type ProductInput = Omit<Product, 'id' | 'productType'> & { productType: number };

export async function getProducts() { return (await http.get<Product[]>('/products')).data; }
export async function createProduct(value: ProductInput) { return (await http.post<Product>('/products', value)).data; }
export async function updateProduct(id: string, value: ProductInput) { await http.put(`/products/${id}`, value); }
export type ProductRemovalResult = { deleted: boolean; archived: boolean; message: string };
export async function removeProduct(id: string) { return (await http.delete<ProductRemovalResult>(`/products/${id}`)).data; }
