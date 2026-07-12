import '@angular/compiler';
import { describe, expect, it } from 'vitest';
import {
  buildTimeZoneOptions,
  DEFAULT_PROFILE,
  isRecurringSelection,
  resolveDefaultTimeZone,
  resolveDevelopmentRole
} from './scheduler-shell.component';

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

  it('uses the OS/browser timezone as the default scheduler timezone', () => {
    const browserTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';

    expect(resolveDefaultTimeZone()).toBe(browserTimeZone);
    expect(buildTimeZoneOptions(browserTimeZone)).toContainEqual({
      label: browserTimeZone.replaceAll('_', ' '),
      value: browserTimeZone
    });
  });

  it('enables recurrence controls only for repeating meetings', () => {
    expect(isRecurringSelection('None')).toBe(false);
    expect(isRecurringSelection(null)).toBe(false);
    expect(isRecurringSelection(undefined)).toBe(false);
    expect(isRecurringSelection('Daily')).toBe(true);
    expect(isRecurringSelection('Weekly')).toBe(true);
    expect(isRecurringSelection('Monthly')).toBe(true);
  });
});
