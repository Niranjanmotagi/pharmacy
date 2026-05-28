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
import { OrderDetailModal } from '../../components/order-detail-modal/order-detail-modal';
import { PrescriptionModal } from '../../components/prescription-modal/prescription-modal';
import { readErrorMessage } from '../../shared/http-error';

type OrderFilter = 'all' | 'pending' | 'approved' | 'packed' | 'shipped' | 'delivered' | 'rejected';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    Navbar,
    OrderActionModal,
    OrderDetailModal,
    PrescriptionModal
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders implements OnInit {
  orders: Order[] = [];
  loading = true;
  adminMode = false;
  selectedFilter: OrderFilter = 'all';

  // Approve / Reject modal
  modalAction: 'approve' | 'reject' = 'approve';
  modalShow = false;
  modalSubmitting = false;

  // Detail modal
  detailShow = false;
  detailOrder: Order | null = null;

  // Prescription modal
  rxShow = false;
  rxOrderId: number | null = null;
  rxOrderNumber = '';
  rxFilename = '';

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
    return this.orders.filter((o) => {
      const s = (o.status || '').toLowerCase();
      switch (this.selectedFilter) {
        case 'pending':   return s.includes('pending');
        case 'approved':  return s.includes('approve');
        case 'packed':    return s.includes('pack');
        case 'shipped':   return s.includes('out');
        case 'delivered': return s.includes('deliver');
        case 'rejected':  return s.includes('reject');
        default:          return true;
      }
    });
  }

  setFilter(f: OrderFilter): void {
    this.selectedFilter = f;
  }

  statusClass(o: Order): string {
    const s = (o.status || '').toLowerCase();
    if (s.includes('reject'))  return 'bb-status bb-status-rejected';
    if (s.includes('deliver')) return 'bb-status bb-status-delivered';
    if (s.includes('out'))     return 'bb-status bb-status-shipped';
    if (s.includes('pack'))    return 'bb-status bb-status-packed';
    if (s.includes('approve')) return 'bb-status bb-status-approved';
    return 'bb-status bb-status-pending';
  }

  // ===== Detail modal =====
  openDetail(o: Order): void {
    this.detailOrder = o;
    this.detailShow = true;
  }
  closeDetail(): void {
    this.detailShow = false;
    this.detailOrder = null;
  }

  // ===== Prescription preview =====
  openPrescription(o: Order): void {
    if (!o.prescriptionFile) return;
    this.rxOrderId = o.id;
    this.rxOrderNumber = o.orderNumber;
    this.rxFilename = o.prescriptionFile;
    this.rxShow = true;
  }
  closePrescription(): void {
    this.rxShow = false;
    this.rxOrderId = null;
  }

  // ===== Approve / Reject modal =====
  openApprove(o: Order): void {
    this.detailShow = false;
    this.modalAction = 'approve';
    this.detailOrder = o;
    this.modalShow = true;
  }
  openReject(o: Order): void {
    this.detailShow = false;
    this.modalAction = 'reject';
    this.detailOrder = o;
    this.modalShow = true;
  }
  closeModal(): void {
    this.modalShow = false;
  }

  onModalConfirm(result: OrderActionResult): void {
    const order = this.detailOrder;
    if (!order) return;
    this.modalSubmitting = true;

    const onSuccess = (): void => {
      this.modalSubmitting = false;
      this.modalShow = false;
      this.toast.success(
        result.action === 'approve'
          ? `Order ${order.orderNumber} approved.`
          : `Order ${order.orderNumber} rejected.`
      );
      this.load();
    };
    const onError = (err: unknown): void => {
      this.modalSubmitting = false;
      this.toast.error('Action failed', readErrorMessage(err, 'Server error.'));
    };

    if (result.action === 'approve') {
      this.orderService
        .approve(order.id, result.estimatedDeliveryDate ?? null)
        .subscribe({ next: onSuccess, error: onError });
    } else {
      this.orderService
        .reject(order.id, result.reason ?? '')
        .subscribe({ next: onSuccess, error: onError });
    }
  }

  // ===== Lifecycle: Pack / Ship / Deliver =====
  markPacked(o: Order): void {
    this.orderService.pack(o.id).subscribe({
      next: () => {
        this.toast.success(`Order ${o.orderNumber} packed.`);
        this.load();
        this.detailShow = false;
      },
      error: (err: unknown) =>
        this.toast.error('Could not pack', readErrorMessage(err))
    });
  }
  markShipped(o: Order): void {
    this.orderService.ship(o.id).subscribe({
      next: () => {
        this.toast.success(`Order ${o.orderNumber} is out for delivery.`);
        this.load();
        this.detailShow = false;
      },
      error: (err: unknown) =>
        this.toast.error('Could not ship', readErrorMessage(err))
    });
  }
  markDelivered(o: Order): void {
    this.orderService.deliver(o.id).subscribe({
      next: () => {
        this.toast.success(`Order ${o.orderNumber} marked as delivered.`);
        this.load();
        this.detailShow = false;
      },
      error: (err: unknown) =>
        this.toast.error('Could not mark delivered', readErrorMessage(err))
    });
  }

  // ===== Customer reorder =====
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
}
