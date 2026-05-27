import { Medicine } from './medicine.model';

export interface OrderItem {
  id: number;
  orderId: number;
  medicineId: number;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  medicine?: Medicine;
}

export interface Order {
  id: number;
  userId: number;
  orderNumber: string;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  promotionId?: number | null;
  orderDate: string;
  estimatedDeliveryDate?: string | null;
  status: string;
  rejectionReason?: string | null;
  deliveryAddress?: string;
  deliveryPhone?: string;
  deliveryNotes?: string;
  prescriptionFile?: string;
  items: OrderItem[];
}
