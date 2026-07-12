import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';

@Component({
  standalone: true,
  selector: 'app-admin-callback',
  imports: [RouterLink],
  template: `
    <main class="onboarding-page">
      <section class="auth-card">
        <span>{{ status() }}</span>
        <h1>Admin Consent</h1>
        <p>{{ detail() }}</p>
        <a class="btn" routerLink="/">
          Open Scheduler
          <i class="ph ph-arrow-right" aria-hidden="true"></i>
        </a>
      </section>
    </main>
  `
})
export class AdminCallbackComponent implements OnInit {
  readonly status = signal('Completing tenant registration');
  readonly detail = signal('Saving Microsoft tenant consent in the scheduler.');

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService, private readonly toasts?: ToastService) {}

  ngOnInit(): void {
    const tenant = this.route.snapshot.queryParamMap.get('tenant') ?? '';
    const granted = this.route.snapshot.queryParamMap.get('admin_consent') === 'True';
    const state = this.route.snapshot.queryParamMap.get('state') ?? '';

    if (!tenant || !state || !granted) {
      this.status.set('Consent was not completed');
      this.detail.set('Microsoft did not return a successful admin consent response.');
      this.toasts?.validation('Microsoft did not return a successful admin consent response.');
      return;
    }

    this.api.completeAdminConsent({
      microsoftTenantId: tenant,
      adminConsentGranted: granted,
      state
    }).subscribe({
      next: () => {
        this.status.set('Organization connected');
        this.detail.set('Users from this tenant can now sign in and book rooms.');
        this.toasts?.success('Organization connected.');
      },
      error: () => {
        this.status.set('Registration failed');
        this.detail.set('The consent returned from Microsoft, but the API could not save the tenant.');
        this.toasts?.error('The consent returned from Microsoft, but the API could not save the tenant.');
      }
    });
  }
}
