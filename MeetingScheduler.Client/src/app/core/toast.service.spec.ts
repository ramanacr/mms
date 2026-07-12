import { describe, expect, it } from 'vitest';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  it('adds success, error, and validation toasts', () => {
    const service = new ToastService();

    service.success('Room saved.');
    service.error('Room could not be saved.');
    service.validation('Please complete required fields.');

    expect(service.toasts()).toEqual([
      expect.objectContaining({ tone: 'success', message: 'Room saved.' }),
      expect.objectContaining({ tone: 'error', message: 'Room could not be saved.' }),
      expect.objectContaining({ tone: 'warning', message: 'Please complete required fields.' })
    ]);
  });

  it('dismisses a toast by id', () => {
    const service = new ToastService();
    service.info('Loading profile.');
    const [toast] = service.toasts();

    service.dismiss(toast.id);

    expect(service.toasts()).toEqual([]);
  });
});
