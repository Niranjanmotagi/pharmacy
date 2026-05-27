import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export type OrderAction = 'approve' | 'reject';

export interface OrderActionResult {
  action: OrderAction;
  reason?: string;
  estimatedDeliveryDate?: string;
}

@Component({
  selector: 'app-order-action-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order-action-modal.html',
  styleUrl: './order-action-modal.css'
})
export class OrderActionModal implements OnChanges {
  @Input() action: OrderAction = 'approve';
  @Input() orderNumber = '';
  @Input() show = false;
  @Input() submitting = false;

  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<OrderActionResult>();

  reason = '';
  estimatedDeliveryDate = this.defaultEta();

  private defaultEta(): string {
    const d = new Date();
    d.setDate(d.getDate() + 3);
    return d.toISOString().slice(0, 10);
  }

  ngOnChanges(): void {
    if (this.show) {
      this.reason = '';
      this.estimatedDeliveryDate = this.defaultEta();
    }
  }

  closeModal(): void {
    if (this.submitting) return;
    this.close.emit();
  }

  submit(): void {
    if (this.action === 'reject') {
      if (!this.reason.trim()) return;
      this.confirm.emit({ action: 'reject', reason: this.reason.trim() });
    } else {
      this.confirm.emit({
        action: 'approve',
        estimatedDeliveryDate: this.estimatedDeliveryDate
      });
    }
  }
}
