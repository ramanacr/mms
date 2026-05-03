import { Injectable, Inject } from '@angular/core';
import { MsalBroadcastService, MsalGuardConfiguration, MsalService, MSAL_GUARD_CONFIG } from '@azure/msal-angular';
import { InteractionStatus, RedirectRequest } from '@azure/msal-browser';
import { BehaviorSubject, filter } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly isAuthenticated$ = new BehaviorSubject(false);

  constructor(
    @Inject(MSAL_GUARD_CONFIG) private readonly guardConfig: MsalGuardConfiguration,
    private readonly msal: MsalService,
    broadcast: MsalBroadcastService
  ) {
    broadcast.inProgress$
      .pipe(filter((status) => status === InteractionStatus.None))
      .subscribe(() => this.checkAccount());
  }

  checkAccount(): void {
    const accounts = this.msal.instance.getAllAccounts();
    if (accounts.length > 0) {
      this.msal.instance.setActiveAccount(accounts[0]);
    }
    this.isAuthenticated$.next(accounts.length > 0);
  }

  login(): void {
    const authRequest = this.guardConfig.authRequest as RedirectRequest | undefined;
    this.msal.loginRedirect(authRequest ?? { scopes: environment.auth.scopes });
  }

  logout(): void {
    this.msal.logoutRedirect({ postLogoutRedirectUri: environment.auth.postLogoutRedirectUri });
  }

  accountName(): string {
    return this.msal.instance.getActiveAccount()?.name ?? this.msal.instance.getAllAccounts()[0]?.username ?? 'Meeting Scheduler';
  }

  triggerAdminConsent(): void {
    const redirectUri = encodeURIComponent(`${window.location.origin}/admin-callback`);
    const state = crypto.randomUUID();
    sessionStorage.setItem('adminConsentState', state);
    const consentUrl = `https://login.microsoftonline.com/common/adminconsent?client_id=${environment.auth.clientId}&redirect_uri=${redirectUri}&state=${state}`;
    window.location.href = consentUrl;
  }
}
