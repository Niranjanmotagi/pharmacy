import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bb-toast-stack" aria-live="polite" aria-atomic="true">
      @for (t of toasts(); track t.id) {
        <div class="bb-toast" [class.success]="t.kind === 'success'"
             [class.error]="t.kind === 'error'"
             [class.info]="t.kind === 'info'">
          <div>
            <div class="bb-toast-title">{{ t.title }}</div>
            @if (t.body) {
              <div class="bb-toast-body">{{ t.body }}</div>
            }
          </div>
          <button class="bb-toast-close" type="button" aria-label="Close"
                  (click)="toast.dismiss(t.id)">&times;</button>
        </div>
      }
    </div>
  `
})
export class ToastHost {
  toast = inject(ToastService);
  toasts = this.toast.toasts;
}
