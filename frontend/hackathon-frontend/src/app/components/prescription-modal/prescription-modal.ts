import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { OrderService } from '../../services/order.service';
import { ToastService } from '../../services/toast.service';
import { readErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-prescription-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './prescription-modal.html',
  styleUrl: './prescription-modal.css'
})
export class PrescriptionModal implements OnChanges {
  @Input() show = false;
  @Input() orderId: number | null = null;
  @Input() orderNumber = '';
  @Input() filename = '';

  @Output() close = new EventEmitter<void>();

  private sanitizer = inject(DomSanitizer);

  loading = false;
  /** Raw blob: URL used for image preview + downloads. */
  objectUrl: string | null = null;
  /** Same URL passed through the sanitizer so the iframe accepts it. */
  safePdfUrl: SafeResourceUrl | null = null;
  isPdf = false;
  zoom = 1;
  errorMessage = '';

  constructor(
    private orderService: OrderService,
    private toast: ToastService
  ) {}

  ngOnChanges(): void {
    if (this.show && this.orderId !== null) {
      this.load();
    } else if (!this.show) {
      this.cleanup();
    }
  }

  private cleanup(): void {
    if (this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
      this.objectUrl = null;
    }
    this.safePdfUrl = null;
    this.isPdf = false;
    this.zoom = 1;
    this.errorMessage = '';
  }

  load(): void {
    if (this.orderId === null) return;
    this.cleanup();
    this.loading = true;

    this.orderService.getPrescriptionBlob(this.orderId).subscribe({
      next: (blob: Blob) => {
        this.loading = false;
        this.isPdf =
          blob.type === 'application/pdf' ||
          (this.filename || '').toLowerCase().endsWith('.pdf');

        const url = URL.createObjectURL(blob);
        this.objectUrl = url;

        // iframe[src] is treated as a resource URL and Angular sanitizes blob:
        // links by default. Trust ours explicitly — it's our own data.
        if (this.isPdf) {
          this.safePdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        }
      },
      error: (err: unknown) => {
        this.loading = false;
        this.errorMessage = readErrorMessage(
          err,
          'Could not load this prescription.'
        );
        this.toast.error('Prescription failed', this.errorMessage);
      }
    });
  }

  closeModal(): void {
    this.cleanup();
    this.close.emit();
  }

  zoomIn(): void   { this.zoom = Math.min(this.zoom + 0.25, 3); }
  zoomOut(): void  { this.zoom = Math.max(this.zoom - 0.25, 0.5); }
  zoomReset(): void { this.zoom = 1; }

  download(): void {
    if (!this.objectUrl) return;
    const a = document.createElement('a');
    a.href = this.objectUrl;
    a.download = this.filename || `prescription-${this.orderNumber}`;
    document.body.appendChild(a);
    a.click();
    a.remove();
  }
}
