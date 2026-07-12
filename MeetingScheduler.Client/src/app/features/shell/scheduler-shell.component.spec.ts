import '@angular/compiler';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
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

  it('configures FullCalendar with edit handlers for existing meetings', () => {
    const source = readFileSync(resolve(process.cwd(), 'src/app/features/shell/scheduler-shell.component.ts'), 'utf-8');

    expect(source).toContain('editable: true');
    expect(source).toContain('eventClick:');
    expect(source).toContain('eventDrop:');
    expect(source).toContain('eventResize:');
  });

  it('does not continuously bind the rich text editor HTML while the user types', () => {
    const template = readFileSync(resolve(process.cwd(), 'src/app/features/shell/scheduler-shell.component.html'), 'utf-8');

    expect(template).not.toContain('[innerHTML]="bookingForm.controls.body.value"');
    expect(template).toContain('(input)="updateBodyFromEditor($event)"');
  });

  it('keeps the rich text editor outside a native label so user focus is not redirected', () => {
    const template = readFileSync(resolve(process.cwd(), 'src/app/features/shell/scheduler-shell.component.html'), 'utf-8');

    expect(template).not.toMatch(/<label class="full-span">\s*Body[\s\S]*class="editor-surface"/);
    expect(template).toContain('aria-labelledby="bodyEditorLabel"');
    expect(template).toContain('tabindex="0"');
  });

  it('uses toast feedback for validation and save outcomes', () => {
    const source = readFileSync(resolve(process.cwd(), 'src/app/features/shell/scheduler-shell.component.ts'), 'utf-8');

    expect(source).toContain("inject(ToastService)");
    expect(source).toContain("this.toasts.validation('Please complete required room fields.')");
    expect(source).toContain("this.toasts.validation('Please complete required meeting fields.')");
    expect(source).toContain("this.toasts.success('Room saved.')");
    expect(source).toContain("this.toasts.success('Meeting updated.')");
    expect(source).toContain("this.toasts.error(");
  });
});
