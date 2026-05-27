import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService, PlaceOrderResponse } from '../../services/order.service';
import { CartItem } from '../../models/cart-item.model';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Navbar],
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class Cart implements OnInit {
  items: CartItem[] = [];
  loading = true;
  updatingId: number | null = null;
  purchasing = false;
  error = '';

  // Prescription upload (only used when needsPrescription())
  selectedFile: File | null = null;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(clearError = true): void {
    this.loading = true;
    if (clearError) this.error = '';
    this.cartService.getMyCart().subscribe({
      next: (data: CartItem[]) => {
        this.items = data ?? [];
        this.loading = false;
        this.updatingId = null;
      },
      error: (err: unknown) => {
        this.loading = false;
        this.updatingId = null;
        this.error = readErrorMessage(err, 'Unable to load your cart.');
      }
    });
  }

  remove(item: CartItem): void {
    this.cartService.remove(item.id).subscribe({
      next: () => this.load(),
      error: (err: unknown) => {
        this.error = readErrorMessage(err, 'Could not remove the item.');
      }
    });
  }

  updateQuantity(item: CartItem, quantityValue: number | string): void {
    const quantity = Math.trunc(Number(quantityValue));
    if (!Number.isFinite(quantity) || quantity < 1) {
      this.error = 'Quantity must be at least 1.';
      return;
    }

    const stock = item.medicine?.stockQuantity ?? quantity;
    if (quantity > stock) {
      this.error = `Only ${stock} item(s) available in stock.`;
      return;
    }

    if (quantity === item.quantity) return;

    this.error = '';
    this.updatingId = item.id;
    this.cartService.updateQuantity(item.id, quantity).subscribe({
      next: () => this.load(),
      error: (err: unknown) => {
        this.updatingId = null;
        this.error = readErrorMessage(err, 'Unable to update quantity.');
        this.load(false);
      }
    });
  }

  lineTotal(item: CartItem): number {
    return item.totalPrice ?? (item.medicine?.price || 0) * item.quantity;
  }

  total(): number {
    return this.items.reduce((sum, i) => sum + this.lineTotal(i), 0);
  }

  needsPrescription(): boolean {
    return this.items.some((i) => i.medicine?.requiresPrescription);
  }

  onFile(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file: File | undefined = input?.files?.[0];
    this.error = '';

    if (!file) {
      this.selectedFile = null;
      return;
    }

    // Only allow JPG / PNG for cart-side prescription uploads
    const name = (file.name || '').toLowerCase();
    const type = (file.type || '').toLowerCase();
    const isJpgOrPng =
      type === 'image/jpeg' ||
      type === 'image/png' ||
      name.endsWith('.jpg') ||
      name.endsWith('.jpeg') ||
      name.endsWith('.png');

    if (!isJpgOrPng) {
      this.selectedFile = null;
      // Reset the input so the user can re-pick the same file after fixing
      if (input) {
        try { input.value = ''; } catch { /* ignore */ }
      }
      this.error = 'Only JPG or PNG images are allowed for the prescription.';
      return;
    }

    this.selectedFile = file;
  }

  purchase(): void {
    if (this.purchasing || this.items.length === 0) return;

    if (this.needsPrescription() && !this.selectedFile) {
      this.error =
        'Some items in your cart require a prescription. Please upload a JPG or PNG before purchasing.';
      return;
    }

    this.purchasing = true;
    this.error = '';

    this.orderService
      .placeOrder({
        prescription: this.selectedFile,
        address: 'To be collected at delivery',
        phone: '0000000000',
        notes: 'Quick purchase from cart'
      })
      .subscribe({
        next: (_res: PlaceOrderResponse) => {
          this.purchasing = false;
          this.items = [];
          this.selectedFile = null;
          alert('Order confirmed — waiting for validation');
          this.router.navigate(['/orders']);
        },
        error: (err: unknown) => {
          console.error(err);
          this.purchasing = false;
          this.error = readErrorMessage(
            err,
            'Could not place the order. Please try again.'
          );
        }
      });
  }
}
