import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  standalone: true,
  selector: 'app-toast-host',
  imports: [CommonModule],
  template: `
    <section class="toast-region" aria-live="polite" aria-label="Notifications">
      @for (toast of toasts.toasts(); track toast.id) {
        <article class="toast" [class]="toast.tone">
          <div>
            <strong>{{ toast.title }}</strong>
            <span>{{ toast.message }}</span>
          </div>
          <button type="button" class="toast-close" aria-label="Dismiss notification" (click)="toasts.dismiss(toast.id)">x</button>
        </article>
      }
    </section>
  `,
  styles: [`
    .toast-region {
      display: grid;
      gap: 0.6rem;
      max-width: min(92vw, 26rem);
      position: fixed;
      right: 1rem;
      top: 1rem;
      z-index: 80;
    }

    .toast {
      align-items: flex-start;
      backdrop-filter: blur(18px) saturate(150%);
      background: rgba(255, 255, 255, 0.82);
      border: 1px solid rgba(255, 255, 255, 0.76);
      border-left-width: 0.35rem;
      border-radius: 0.75rem;
      box-shadow: 0 18px 48px rgba(15, 23, 42, 0.14), inset 0 1px 0 rgba(255, 255, 255, 0.78);
      color: #132238;
      display: flex;
      gap: 0.75rem;
      justify-content: space-between;
      padding: 0.85rem 0.9rem;
    }

    .toast.success {
      border-left-color: #16a34a;
    }

    .toast.error {
      border-left-color: #dc2626;
    }

    .toast.warning {
      border-left-color: #f59e0b;
    }

    .toast.info {
      border-left-color: #2563eb;
    }

    .toast strong,
    .toast span {
      display: block;
    }

    .toast strong {
      font-size: 0.84rem;
      line-height: 1.25;
      margin-bottom: 0.15rem;
    }

    .toast span {
      color: #475569;
      font-size: 0.88rem;
      line-height: 1.35;
    }

    .toast-close {
      appearance: none;
      background: rgba(248, 250, 252, 0.72);
      border: 1px solid rgba(148, 163, 184, 0.24);
      border-radius: 0.45rem;
      color: #64748b;
      cursor: pointer;
      font: inherit;
      font-weight: 900;
      line-height: 1;
      min-height: 1.7rem;
      min-width: 1.7rem;
      padding: 0.15rem;
    }

    .toast-close:hover {
      background: rgba(236, 254, 255, 0.86);
      color: #0f172a;
    }

    @media (max-width: 640px) {
      .toast-region {
        left: 0.75rem;
        max-height: 45vh;
        max-width: none;
        overflow-y: auto;
        right: 0.75rem;
        top: 0.75rem;
      }

      .toast {
        padding: 0.75rem;
      }
    }
  `]
})
export class ToastHostComponent {
  readonly toasts = inject(ToastService);
}
