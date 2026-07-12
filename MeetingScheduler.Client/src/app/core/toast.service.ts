import { Injectable, signal } from '@angular/core';

export type ToastTone = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  tone: ToastTone;
  message: string;
  title: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly messages = signal<ToastMessage[]>([]);

  readonly toasts = this.messages.asReadonly();

  success(message: string): void {
    this.show('success', 'Success', message);
  }

  error(message: string): void {
    this.show('error', 'Error', message);
  }

  validation(message: string): void {
    this.show('warning', 'Validation', message);
  }

  info(message: string): void {
    this.show('info', 'Info', message);
  }

  dismiss(id: number): void {
    this.messages.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }

  private show(tone: ToastTone, title: string, message: string): void {
    const id = this.nextId++;
    this.messages.update((toasts) => [...toasts, { id, tone, title, message }]);
    const timer = globalThis.setTimeout(() => this.dismiss(id), 4000);
    (timer as { unref?: () => void }).unref?.();
  }
}
