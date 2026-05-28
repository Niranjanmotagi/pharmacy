import {
  Component,
  EventEmitter,
  Input,
  Output
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order } from '../../models/order.model';

export type LifecycleStage =
  | 'Pending Validation'
  | 'Approved'
  | 'Packed'
  | 'Out for Delivery'
  | 'Delivered'
  | 'Rejected';

@Component({
  selector: 'app-order-detail-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-detail-modal.html',
  styleUrl: './order-detail-modal.css'
})
export class OrderDetailModal {
  @Input() show = false;
  @Input() order: Order | null = null;
  @Input() adminMode = false;

  @Output() close = new EventEmitter<void>();
  @Output() previewPrescription = new EventEmitter<Order>();
  @Output() approve = new EventEmitter<Order>();
  @Output() reject = new EventEmitter<Order>();
  @Output() pack = new EventEmitter<Order>();
  @Output() ship = new EventEmitter<Order>();
  @Output() deliver = new EventEmitter<Order>();

  /** Ordered stages used for the timeline. */
  readonly stages: LifecycleStage[] = [
    'Pending Validation',
    'Approved',
    'Packed',
    'Out for Delivery',
    'Delivered'
  ];

  closeModal(): void {
    this.close.emit();
  }

  /** Index of the current order status within `stages`. -1 if rejected. */
  currentIndex(): number {
    if (!this.order) return 0;
    const s = (this.order.status || '').toLowerCase();
    if (s.includes('reject')) return -1;
    if (s.includes('deliver')) return 4;
    if (s.includes('out')) return 3;
    if (s.includes('pack')) return 2;
    if (s.includes('approve')) return 1;
    return 0;
  }

  isRejected(): boolean {
    return (this.order?.status || '').toLowerCase().includes('reject');
  }

  statusClass(): string {
    const s = (this.order?.status || '').toLowerCase();
    if (s.includes('reject')) return 'bb-status bb-status-rejected';
    if (s.includes('deliver')) return 'bb-status bb-status-delivered';
    if (s.includes('out')) return 'bb-status bb-status-shipped';
    if (s.includes('pack')) return 'bb-status bb-status-packed';
    if (s.includes('approve')) return 'bb-status bb-status-approved';
    return 'bb-status bb-status-pending';
  }

  // ===== Admin lifecycle action visibility =====
  canApprove(): boolean {
    if (!this.adminMode || !this.order) return false;
    const s = (this.order.status || '').toLowerCase();
    return s.includes('pending');
  }
  canReject(): boolean {
    if (!this.adminMode || !this.order) return false;
    const s = (this.order.status || '').toLowerCase();
    return !s.includes('deliver') && !s.includes('reject');
  }
  canPack(): boolean {
    if (!this.adminMode || !this.order) return false;
    const s = (this.order.status || '').toLowerCase();
    return s.includes('approve');
  }
  canShip(): boolean {
    if (!this.adminMode || !this.order) return false;
    const s = (this.order.status || '').toLowerCase();
    return s.includes('pack');
  }
  canDeliver(): boolean {
    if (!this.adminMode || !this.order) return false;
    const s = (this.order.status || '').toLowerCase();
    return s.includes('out');
  }

  onApprove(): void { if (this.order) this.approve.emit(this.order); }
  onReject(): void { if (this.order) this.reject.emit(this.order); }
  onPack(): void { if (this.order) this.pack.emit(this.order); }
  onShip(): void { if (this.order) this.ship.emit(this.order); }
  onDeliver(): void { if (this.order) this.deliver.emit(this.order); }
  onPreview(): void { if (this.order) this.previewPrescription.emit(this.order); }
}
