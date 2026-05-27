import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { Order } from '../../models/order.model';
import { Navbar } from '../../components/navbar/navbar';
import {
  OrderActionModal,
  OrderActionResult
} from '../../components/order-action-modal/order-action-modal';
import { readErrorMessage } from '../../shared/http-error';
import { environment } from '../../../environments/environment';

type OrderFilter = 'all' | 'pending' | 'approved' | 'rejected' | 'delivered';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar, OrderActionModal],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders implements OnInit {
  orders: Order[] = [];
  loading = true;
  adminMode = false;
  selectedFilter: OrderFilter = 'all';

  // Modal state
  modalAction: 'approve' | 'reject' = 'approve';
  modalShow = false;
  modalSubmitting = false;
  modalOrder: Order | null = null;

  constructor(
    private orderService: OrderService,
    private cartService: CartService,
    private toast: ToastService,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.adminMode = this.auth.isAdmin();
    this.load();
  }

  load(): void {
    this.loading = true;
    const call: Observable<Order[]> = this.adminMode
      ? this.orderService.allOrders()
      : this.orderService.myOrders();

    call.subscribe({
      next: (data: Order[]) => {
        this.orders = data ?? [];
        this.loading = false;
      },
      error: (err: unknown) => {
        this.loading = false;
        this.toast.error('Could not load orders', readErrorMessage(err));
      }
    });
  }

  filtered(): Order[] {
    if (this.selectedFilter === 'all') return this.orders;
    return this.orders.filter((o) =>
      (o.status || '').toLowerCase().includes(this.selectedFilter)
    );
  }

  setFilter(f: OrderFilter): void {
    this.selectedFilter = f;
  }

  statusClass(o: Order): string {
    const s = (o.status || '').toLowerCase();
    if (s.includes('reject')) return 'bb-status bb-status-rejected';
    if (s.includes('approve')) return 'bb-status bb-status-approved';
    if (s.includes('deliver')) return 'bb-status bb-status-delivered';
    return 'bb-status bb-status-pending';
  }

  openApprove(o: Order): void {
    this.modalAction = 'approve';
    this.modalOrder = o;
    this.modalShow = true;
  }

  openReject(o: Order): void {
    this.modalAction = 'reject';
    this.modalOrder = o;
    this.modalShow = true;
  }

  closeModal(): void {
    this.modalShow = false;
    this.modalOrder = null;
  }

  onModalConfirm(result: OrderActionResult): void {
    const order = this.modalOrder;
    if (!order) return;

    this.modalSubmitting = true;

    // Each branch creates its own Observable with a concrete response shape,
    // and we subscribe with handlers typed as (unknown, unknown). That avoids
    // the TS2349 "expression not callable" error caused by unifying the
    // subscribe() overloads of two different Observable<T> types.
    const handleSuccess = (): void => {
      this.modalSubmitting = false;
      this.modalShow = false;
      this.modalOrder = null;
      this.toast.success(
        result.action === 'approve'
          ? `Order ${order.orderNumber} approved.`
          : `Order ${order.orderNumber} rejected.`
      );
      this.load();
    };

    const handleError = (err: unknown): void => {
      this.modalSubmitting = false;
      this.toast.error('Action failed', readErrorMessage(err, 'Server error.'));
    };

    if (result.action === 'approve') {
      this.orderService
        .approve(order.id, result.estimatedDeliveryDate ?? null)
        .subscribe({ next: handleSuccess, error: handleError });
    } else {
      this.orderService
        .reject(order.id, result.reason ?? '')
        .subscribe({ next: handleSuccess, error: handleError });
    }
  }

  markDelivered(o: Order): void {
    this.orderService.deliver(o.id).subscribe({
      next: () => {
        this.toast.success(`Order ${o.orderNumber} marked as delivered.`);
        this.load();
      },
      error: (err: unknown) =>
        this.toast.error(
          'Could not mark delivered',
          readErrorMessage(err, 'Server error.')
        )
    });
  }

  reorder(o: Order): void {
    if (!o.items || o.items.length === 0) return;

    const requests = o.items.map((i) =>
      this.cartService.addToCart(i.medicineId, i.quantity)
    );
    let done = 0;
    let failed = 0;

    requests.forEach((req) =>
      req.subscribe({
        next: () => {
          done++;
          if (done + failed === requests.length) this.summarize(done, failed);
        },
        error: () => {
          failed++;
          if (done + failed === requests.length) this.summarize(done, failed);
        }
      })
    );
  }

  private summarize(done: number, failed: number): void {
    if (failed === 0) {
      this.toast.success('Reorder added to your cart.');
    } else if (done === 0) {
      this.toast.error('Reorder failed. Items may be out of stock.');
    } else {
      this.toast.info(
        'Partial reorder',
        `${done} items added, ${failed} could not be added.`
      );
    }
  }

  prescriptionUrl(file?: string | null): string {
    if (!file) return '';
    return `${environment.staticBaseUrl}/prescriptions/${file}`;
  }
}
