// Development environment. Angular CLI swaps this file for environment.prod.ts
// during a production build via the fileReplacements entry in angular.json.
export const environment = {
  production: false,

  /** Backend API root (no trailing slash). */
  apiBaseUrl: 'http://localhost:5020',

  /** Static files (prescription images, etc.) live under the same origin. */
  staticBaseUrl: 'http://localhost:5020'
};
