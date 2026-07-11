import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { ApiService } from '../../core/api.service';
import { environment } from '../../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-admin-onboarding',
  imports: [ButtonModule, CardModule, StepsModule],
  template: `
    <main class="onboarding-page">
      <p-card header="Organization Integration" subheader="Connect your Microsoft 365 tenant">
        <p-steps [model]="steps" [readonly]="true" />
        <div class="consent-list">
          <span><i class="pi pi-check-circle"></i> Delegated calendar access</span>
          <span><i class="pi pi-check-circle"></i> Tenant-isolated rooms and bookings</span>
          <span><i class="pi pi-check-circle"></i> Outlook attendee invites</span>
        </div>
        <p-button label="Authorize as Administrator" icon="pi pi-microsoft" styleClass="w-full" (onClick)="startAdminConsent()" />
        @if (showDevelopmentBypass) {
          <p-button label="Simulate Consent" icon="pi pi-bolt" severity="secondary" styleClass="w-full mt-2" (onClick)="simulateDevelopmentConsent()" />
        }
      </p-card>
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
