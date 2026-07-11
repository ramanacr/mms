import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ApiService } from '../../core/api.service';

@Component({
  standalone: true,
  selector: 'app-admin-callback',
  imports: [ButtonModule, CardModule, RouterLink],
  template: `
    <main class="onboarding-page">
      <p-card header="Admin Consent" [subheader]="status()">
        <p>{{ detail() }}</p>
        <p-button label="Open Scheduler" icon="pi pi-arrow-right" routerLink="/" />
      </p-card>
    </main>
  `
})
export class AdminCallbackComponent implements OnInit {
  readonly status = signal('Completing tenant registration');
  readonly detail = signal('Saving Microsoft tenant consent in the scheduler.');

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    const tenant = this.route.snapshot.queryParamMap.get('tenant') ?? '';
    const granted = this.route.snapshot.queryParamMap.get('admin_consent') === 'True';
    const state = this.route.snapshot.queryParamMap.get('state') ?? '';

    if (!tenant || !state || !granted) {
      this.status.set('Consent was not completed');
      this.detail.set('Microsoft did not return a successful admin consent response.');
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
      },
      error: () => {
        this.status.set('Registration failed');
        this.detail.set('The consent returned from Microsoft, but the API could not save the tenant.');
      }
    });
  }
}
