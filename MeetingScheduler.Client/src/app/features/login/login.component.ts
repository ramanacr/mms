import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  template: `
    <main class="login-page">
      <section class="login-copy">
        <div class="brand-mark">M</div>
        <h1>Meeting Room Scheduler</h1>
        <p>Book rooms, prevent conflicts, and send Outlook invites from one secure Microsoft 365 workspace.</p>
      </section>

      <section class="auth-card">
        <span>Use your organization Microsoft account</span>
        <h2>Sign in</h2>
        <button type="button" class="btn btn-block login-button" (click)="handleLogin()">
          <i class="ph ph-microsoft-logo" aria-hidden="true"></i>
          Login with Microsoft
        </button>
        <p class="fine-print">Admins can connect organization-wide consent after signing in.</p>
      </section>
    </main>
  `
})
export class LoginComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  constructor(private readonly router: Router, readonly auth: AuthService) {}

  ngOnInit(): void {
    this.auth.isAuthenticated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((isAuthenticated) => {
        if (isAuthenticated) {
          this.router.navigate(['/dashboard']);
        }
      });
  }

  handleLogin(): void {
    this.auth.login();
  }
}
