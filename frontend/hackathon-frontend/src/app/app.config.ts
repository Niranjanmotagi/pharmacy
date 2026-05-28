import { ApplicationConfig } from '@angular/core';

import {
  provideRouter,
  withInMemoryScrolling,
  withRouterConfig
} from '@angular/router';

import {
  provideHttpClient,
  withInterceptors,
  withFetch
} from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(
      routes,
      // On every navigation: jump back to the top of the page so users on
      // mobile don't think the click didn't work.
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled'
      }),
      // Treat /foo and /foo/ as the same path; reload when navigating to
      // the same URL so users can re-click "Medicines" to refresh.
      withRouterConfig({ onSameUrlNavigation: 'reload' })
    ),

    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    )
  ]
};
