import '@angular/compiler';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { AdminOnboardingComponent } from './admin-onboarding.component';

describe('AdminOnboardingComponent', () => {
  it('starts admin consent and redirects to the returned URL', () => {
    const api = {
      startAdminConsent: vi.fn(() => of({ consentUrl: 'https://login.microsoftonline.com/common/adminconsent?state=abc' }))
    };
    const component = new AdminOnboardingComponent(api as any);
    const testWindow = { location: { href: '' } };
    vi.stubGlobal('window', testWindow);

    component.startAdminConsent();

    expect(api.startAdminConsent).toHaveBeenCalledOnce();
    expect(testWindow.location.href).toBe('https://login.microsoftonline.com/common/adminconsent?state=abc');
    vi.unstubAllGlobals();
  });

  it('simulates development consent using the dev tenant and returned state', () => {
    const api = {
      startAdminConsent: vi.fn(() => of({ consentUrl: 'https://login.microsoftonline.com/common/adminconsent?state=dev-state' })),
      completeAdminConsent: vi.fn(() => of({}))
    };
    const component = new AdminOnboardingComponent(api as any);
    const testWindow = { location: { href: '' } };
    vi.stubGlobal('window', testWindow);

    component.simulateDevelopmentConsent();

    expect(api.completeAdminConsent).toHaveBeenCalledWith({
      microsoftTenantId: '00000000-0000-0000-0000-000000000001',
      adminConsentGranted: true,
      state: 'dev-state'
    });
    expect(testWindow.location.href).toBe('/dev-admin');
    vi.unstubAllGlobals();
  });
});
