import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { AuthService } from '../../core/auth.service';

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
        <p-button label="Authorize as Administrator" icon="pi pi-microsoft" styleClass="w-full" (onClick)="auth.triggerAdminConsent()" />
      </p-card>
    </main>
  `
})
export class AdminOnboardingComponent {
  readonly steps = [
    { label: 'Consent' },
    { label: 'Register' },
    { label: 'Sync' }
  ];

  constructor(readonly auth: AuthService) {}
}
