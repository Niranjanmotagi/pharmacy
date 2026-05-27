// Production environment. Set these to your deployed backend URLs.
// You can either:
//   1) Hardcode the Render URL here before each `ng build`, or
//   2) Replace the value at build-time on Vercel using NG_API_BASE_URL — see
//      the README / docs/DEPLOYMENT.md for the snippet.
export const environment = {
  production: true,

  /** Backend API root (no trailing slash). */
  apiBaseUrl: 'https://YOUR-RENDER-SERVICE.onrender.com',

  /** Where uploaded prescription images are served from. */
  staticBaseUrl: 'https://YOUR-RENDER-SERVICE.onrender.com'
};
