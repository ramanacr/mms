import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

describe('free UI dependency policy', () => {
  it('does not depend on PrimeUI licensed packages', () => {
    const packageJson = JSON.parse(readFileSync(resolve(process.cwd(), 'package.json'), 'utf-8')) as {
      dependencies?: Record<string, string>;
      devDependencies?: Record<string, string>;
    };
    const dependencies = {
      ...packageJson.dependencies,
      ...packageJson.devDependencies
    };

    expect(dependencies).not.toHaveProperty('primeng');
    expect(dependencies).not.toHaveProperty('@primeng/themes');
    expect(dependencies).not.toHaveProperty('@primeuix/themes');
    expect(dependencies).not.toHaveProperty('primeicons');
  });
});
