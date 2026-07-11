import '@angular/compiler';
import { describe, expect, it } from 'vitest';
import { DEFAULT_PROFILE, resolveDevelopmentRole } from './scheduler-shell.component';

describe('SchedulerShellComponent defaults', () => {
  it('does not grant admin role before the profile loads', () => {
    expect(DEFAULT_PROFILE.roles).toEqual([]);
  });

  it('resolves the dev user route to a non-admin persona', () => {
    expect(resolveDevelopmentRole('/dev-user', 'admin')).toBe('user');
  });

  it('resolves the dev admin route to an admin persona', () => {
    expect(resolveDevelopmentRole('/dev-admin', 'user')).toBe('admin');
  });
});
