import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CardModule],
  template: `
    <main class="login-page">
      <section class="login-copy">
        <div class="brand-mark">M</div>
        <h1>Meeting Room Scheduler</h1>
        <p>Book rooms, prevent conflicts, and send Outlook invites from one secure Microsoft 365 workspace.</p>
      </section>

      <p-card header="Sign in" subheader="Use your organization Microsoft account">
        <button type="button" class="login-button" (click)="handleLogin()">
          <span class="pi pi-microsoft"></span>
          Login with Microsoft
        </button>
        <p class="fine-print">Admins can connect organization-wide consent after signing in.</p>
      </p-card>
    </main>
  `
})
export class LoginComponent implements OnInit {
  constructor(private readonly router: Router, readonly auth: AuthService) {}

  ngOnInit(): void {
    this.auth.isAuthenticated$.subscribe((isAuthenticated) => {
      if (isAuthenticated) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  handleLogin(): void {
    console.log('Login button clicked');
    this.auth.login();
  }
}
