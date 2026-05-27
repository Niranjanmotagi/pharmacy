import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  title: string;
  body?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private idSeq = 0;
  readonly toasts = signal<Toast[]>([]);

  success(title: string, body?: string) {
    this.push({ kind: 'success', title, body });
  }

  error(title: string, body?: string) {
    this.push({ kind: 'error', title, body });
  }

  info(title: string, body?: string) {
    this.push({ kind: 'info', title, body });
  }

  dismiss(id: number) {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private push(toast: Omit<Toast, 'id'>) {
    const id = ++this.idSeq;
    this.toasts.update((list) => [...list, { id, ...toast }]);
    setTimeout(() => this.dismiss(id), 4500);
  }
}
