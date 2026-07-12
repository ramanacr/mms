import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

describe('App shell', () => {
  it('mounts the global toast host beside routed content', () => {
    const source = readFileSync(resolve(process.cwd(), 'src/app/app.ts'), 'utf-8');

    expect(source).toContain('ToastHostComponent');
    expect(source).toContain('<app-toast-host />');
  });
});
