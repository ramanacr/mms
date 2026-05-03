import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [ButtonModule, CardModule],
  template: `
    <main class="login-page">
      <section class="login-copy">
        <div class="brand-mark">M</div>
        <h1>Meeting Room Scheduler</h1>
        <p>Book rooms, prevent conflicts, and send Outlook invites from one secure Microsoft 365 workspace.</p>
      </section>

      <p-card header="Sign in" subheader="Use your organization Microsoft account">
        <p-button label="Continue with Microsoft" icon="pi pi-microsoft" styleClass="w-full" (onClick)="auth.login()" />
        <p class="fine-print">Admins can connect organization-wide consent after signing in.</p>
      </p-card>
    </main>
  `
})
export class LoginComponent {
  constructor(readonly auth: AuthService) {}
}
