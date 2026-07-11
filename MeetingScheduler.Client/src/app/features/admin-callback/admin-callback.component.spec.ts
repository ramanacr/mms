import '@angular/compiler';
import { convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { AdminCallbackComponent } from './admin-callback.component';

describe('AdminCallbackComponent', () => {
  it('sends tenant, grant result, and state to the completion endpoint', () => {
    const api = {
      completeAdminConsent: vi.fn(() => of({}))
    };
    const component = new AdminCallbackComponent({
      snapshot: {
        queryParamMap: convertToParamMap({
          tenant: 'tenant-id',
          admin_consent: 'True',
          state: 'state-id'
        })
      }
    } as any, api as any);

    component.ngOnInit();

    expect(api.completeAdminConsent).toHaveBeenCalledWith({
      microsoftTenantId: 'tenant-id',
      adminConsentGranted: true,
      state: 'state-id'
    });
  });

  it('does not call completion endpoint when state is missing', () => {
    const api = {
      completeAdminConsent: vi.fn(() => of({}))
    };
    const component = new AdminCallbackComponent({
      snapshot: {
        queryParamMap: convertToParamMap({
          tenant: 'tenant-id',
          admin_consent: 'True'
        })
      }
    } as any, api as any);

    component.ngOnInit();

    expect(api.completeAdminConsent).not.toHaveBeenCalled();
    expect(component.status()).toBe('Consent was not completed');
  });
});
