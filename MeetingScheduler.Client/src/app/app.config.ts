import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { MsalInterceptor, MsalModule, MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { IPublicClientApplication, InteractionType, PublicClientApplication } from '@azure/msal-browser';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { environment } from '../environments/environment';

export function msalInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: environment.auth.clientId,
      authority: environment.auth.authority,
      redirectUri: environment.auth.redirectUri
    },
    cache: {
      cacheLocation: 'localStorage'
    }
  });
}

export function msalGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: environment.auth.scopes
    }
  };
}

export function msalInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
  protectedResourceMap.set(`${environment.apiBaseUrl}/*`, environment.auth.apiScopes);
  protectedResourceMap.set('https://graph.microsoft.com/v1.0/me', ['User.Read']);

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.app-dark'
        }
      }
    }),
    importProvidersFrom(
      MsalModule.forRoot(
        msalInstanceFactory(),
        msalGuardConfigFactory(),
        msalInterceptorConfigFactory()
      )
    ),
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true }
  ]
};
