import { Component } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { environment } from '../../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-admin-onboarding',
  template: `
    <main class="onboarding-page">
      <section class="auth-card">
        <span>Connect your Microsoft 365 tenant</span>
        <h1>Organization Integration</h1>
        <ol class="steps">
          @for (step of steps; track step.label) {
            <li>{{ step.label }}</li>
          }
        </ol>
        <div class="consent-list">
          <span><i class="ph ph-check-circle" aria-hidden="true"></i> Delegated calendar access</span>
          <span><i class="ph ph-check-circle" aria-hidden="true"></i> Tenant-isolated rooms and bookings</span>
          <span><i class="ph ph-check-circle" aria-hidden="true"></i> Outlook attendee invites</span>
        </div>
        <button type="button" class="btn btn-block" (click)="startAdminConsent()">
          <i class="ph ph-microsoft-logo" aria-hidden="true"></i>
          Authorize as Administrator
        </button>
        @if (showDevelopmentBypass) {
          <button type="button" class="btn btn-secondary btn-block mt-2" (click)="simulateDevelopmentConsent()">
            <i class="ph ph-lightning" aria-hidden="true"></i>
            Simulate Consent
          </button>
        }
      </section>
    </main>
  `
})
export class AdminOnboardingComponent {
  private static readonly developmentTenantId = '00000000-0000-0000-0000-000000000001';

  readonly showDevelopmentBypass = !environment.production;
  readonly steps = [
    { label: 'Consent' },
    { label: 'Register' },
    { label: 'Sync' }
  ];

  constructor(private readonly api: ApiService) {}

  startAdminConsent(): void {
    this.api.startAdminConsent().subscribe({
      next: ({ consentUrl }) => {
        window.location.href = consentUrl;
      }
    });
  }

  simulateDevelopmentConsent(): void {
    this.api.startAdminConsent().subscribe({
      next: ({ consentUrl }) => {
        const state = new URL(consentUrl).searchParams.get('state') ?? '';
        this.api.completeAdminConsent({
          microsoftTenantId: AdminOnboardingComponent.developmentTenantId,
          adminConsentGranted: true,
          state
        }).subscribe({
          next: () => {
            window.location.href = '/dev-admin';
          }
        });
      }
    });
  }
}
