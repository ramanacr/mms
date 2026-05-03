import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { LoginComponent } from './features/login/login.component';
import { SchedulerShellComponent } from './features/shell/scheduler-shell.component';
import { AdminOnboardingComponent } from './features/admin-onboarding/admin-onboarding.component';
import { AdminCallbackComponent } from './features/admin-callback/admin-callback.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'admin-onboarding', component: AdminOnboardingComponent },
  { path: 'admin-callback', component: AdminCallbackComponent },
  { path: '', component: SchedulerShellComponent, canActivate: [MsalGuard] },
  { path: 'dashboard', redirectTo: '' },
  { path: '**', redirectTo: '' }
];
