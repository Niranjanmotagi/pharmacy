import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { timeout, catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { CartService } from '../../services/cart.service';
import { OrderService, PlaceOrderResponse } from '../../services/order.service';
import {
  PromotionService,
  PromotionValidationResponse
} from '../../services/promotion.service';
import { ToastService } from '../../services/toast.service';

import { CartItem } from '../../models/cart-item.model';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

type PaymentMethod = 'COD' | 'UPI' | 'CARD';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Navbar],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class Checkout implements OnInit {
  items: CartItem[] = [];

  loading = true;
  placing = false;
  loadError = '';
  error = '';
  successMessage = '';

  // Prescription
  selectedFile: File | null = null;
  previewUrl: string | null = null;
  dragging = false;

  // Promo
  promoCode = '';
  promoMessage = '';
  promoDiscount = 0;
  promoValid = false;

  // Delivery details
  address = '';
  phone = '';
  notes = '';

  // Payment
  paymentMethod: PaymentMethod = 'COD';
  upiId = '';
  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private promotionService: PromotionService,
    private toast: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading = true;
    this.loadError = '';

    const cart$: Observable<CartItem[]> = this.cartService.getMyCart().pipe(
      timeout(8000),
      catchError((err: unknown) => {
        console.error('[checkout] failed to load cart', err);
        this.loadError = this.describeLoadError(err);
        return of<CartItem[]>([]);
      })
    );

    cart$.subscribe((data: CartItem[]) => {
      this.items = data ?? [];
      this.loading = false;
      if (this.loadError && this.loadError.startsWith('Your session')) {
        setTimeout(() => this.router.navigate(['/login']), 1200);
      }
    });
  }

  private describeLoadError(err: unknown): string {
    if (err instanceof HttpErrorResponse && err.status === 401) {
      return 'Your session expired. Please log in again.';
    }
    if (err instanceof Error && err.name === 'TimeoutError') {
      return 'The server is taking too long to respond. Is the backend running?';
    }
    return readErrorMessage(err, 'Could not load your cart. Please try again.');
  }

  needsPrescription(): boolean {
    return this.items.some((i) => i.medicine?.requiresPrescription);
  }

  lineTotal(item: CartItem): number {
    return item.totalPrice ?? (item.medicine?.price || 0) * item.quantity;
  }
  subtotal(): number {
    return this.items.reduce((sum, i) => sum + this.lineTotal(i), 0);
  }
  finalTotal(): number {
    return Math.max(0, this.subtotal() - this.promoDiscount);
  }

  // ===== Rx file: input + drag/drop + preview =====

  onFileInput(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0];
    if (file) this.acceptFile(file);
  }

  onDragOver(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
    this.dragging = true;
  }
  onDragLeave(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
    this.dragging = false;
  }
  onDrop(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
    this.dragging = false;
    const file = e.dataTransfer?.files?.[0];
    if (file) this.acceptFile(file);
  }

  private acceptFile(file: File): void {
    const name = (file.name || '').toLowerCase();
    const type = (file.type || '').toLowerCase();
    const isImage =
      type === 'image/jpeg' ||
      type === 'image/png' ||
      name.endsWith('.jpg') ||
      name.endsWith('.jpeg') ||
      name.endsWith('.png');
    const isPdf = type === 'application/pdf' || name.endsWith('.pdf');

    if (!isImage && !isPdf) {
      this.toast.error('Unsupported file type', 'Use a JPG, PNG or PDF.');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.toast.error('File too large', 'Max size is 5 MB.');
      return;
    }

    this.selectedFile = file;
    if (this.previewUrl) URL.revokeObjectURL(this.previewUrl);
    this.previewUrl = isImage ? URL.createObjectURL(file) : null;
  }

  clearFile(): void {
    this.selectedFile = null;
    if (this.previewUrl) URL.revokeObjectURL(this.previewUrl);
    this.previewUrl = null;
  }

  isPdf(): boolean {
    if (!this.selectedFile) return false;
    return (
      this.selectedFile.type === 'application/pdf' ||
      (this.selectedFile.name || '').toLowerCase().endsWith('.pdf')
    );
  }

  // ===== Promo =====
  applyPromo(): void {
    this.promoMessage = '';
    if (!this.promoCode.trim()) {
      this.promoDiscount = 0;
      this.promoValid = false;
      return;
    }
    this.promotionService
      .validate(this.promoCode.trim(), this.subtotal())
      .subscribe({
        next: (res: PromotionValidationResponse) => {
          this.promoDiscount = res.discount;
          this.promoValid = true;
          this.promoMessage = `${res.description} — ₹${res.discount} off`;
        },
        error: (err: unknown) => {
          this.promoDiscount = 0;
          this.promoValid = false;
          this.promoMessage = readErrorMessage(err, 'Invalid promo code');
        }
      });
  }

  // ===== Payment validators =====
  validatePayment(): string | null {
    if (this.paymentMethod === 'COD') return null;
    if (this.paymentMethod === 'UPI') {
      if (!this.upiId.trim() || !this.upiId.includes('@')) {
        return 'Enter a valid UPI ID (e.g. name@bank).';
      }
      return null;
    }
    if (this.paymentMethod === 'CARD') {
      const cleanCard = this.cardNumber.replace(/\s+/g, '');
      if (!/^\d{12,19}$/.test(cleanCard)) return 'Enter a valid card number.';
      if (!/^\d{2}\/\d{2}$/.test(this.cardExpiry))
        return 'Card expiry must be in MM/YY format.';
      if (!/^\d{3,4}$/.test(this.cardCvv)) return 'CVV must be 3 or 4 digits.';
      return null;
    }
    return null;
  }

  // ===== Place order =====
  placeOrder(): void {
    this.error = '';
    this.successMessage = '';

    if (!this.address.trim()) {
      this.error = 'Please enter a delivery address.';
      return;
    }
    if (!/^\d{10}$/.test(this.phone.trim())) {
      this.error = 'Please enter a valid 10-digit phone number.';
      return;
    }
    if (this.needsPrescription() && !this.selectedFile) {
      this.error = 'Please upload prescription before placing the order.';
      return;
    }

    const paymentError = this.validatePayment();
    if (paymentError) {
      this.error = paymentError;
      return;
    }

    this.placing = true;

    const notesWithPayment = [
      this.notes.trim(),
      `Payment: ${this.paymentMethod}` +
        (this.paymentMethod === 'UPI'
          ? ` (${this.upiId})`
          : this.paymentMethod === 'CARD'
          ? ` (xxxx-${this.cardNumber.slice(-4)})`
          : '')
    ]
      .filter(Boolean)
      .join(' | ');

    this.orderService
      .placeOrder({
        prescription: this.selectedFile,
        promoCode: this.promoValid ? this.promoCode.trim() : undefined,
        address: this.address.trim(),
        phone: this.phone.trim(),
        notes: notesWithPayment
      })
      .subscribe({
        next: (res: PlaceOrderResponse) => {
          this.placing = false;
          this.successMessage = `Order placed! ${res.orderNumber} — ₹${res.finalAmount} • ${res.pointsEarned} points earned.`;
          this.toast.success(
            'Order confirmed',
            `${res.orderNumber} • waiting for validation`
          );
          setTimeout(() => this.router.navigate(['/orders']), 1400);
        },
        error: (err: unknown) => {
          console.error(err);
          this.placing = false;
          this.error = readErrorMessage(err, 'Failed to place order');
          this.toast.error('Order failed', this.error);
        }
      });
  }
}
