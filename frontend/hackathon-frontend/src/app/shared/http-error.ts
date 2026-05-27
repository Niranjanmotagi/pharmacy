import { HttpErrorResponse } from '@angular/common/http';

/**
 * A consistent way to read the message we should show the user from an HTTP
 * error. The .NET backend always returns one of these shapes:
 *
 *   { message: "..." }                         // direct controller messages
 *   { status, title, detail, message, errors } // ProblemDetails / validation
 *
 * Anything else falls back to the generic message you pass in.
 */
export function readErrorMessage(
  err: unknown,
  fallback = 'Something went wrong. Please try again.'
): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error;
    if (body && typeof body === 'object') {
      const maybeMessage =
        (body as { message?: unknown }).message ??
        (body as { detail?: unknown }).detail ??
        (body as { title?: unknown }).title;
      if (typeof maybeMessage === 'string' && maybeMessage.trim().length > 0) {
        return maybeMessage;
      }
    }
    if (typeof err.error === 'string' && err.error.trim().length > 0) {
      return err.error;
    }
    if (err.message) return err.message;
  }
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

/** True when the HTTP error indicates the user is not authenticated. */
export function isUnauthorized(err: unknown): boolean {
  return err instanceof HttpErrorResponse && err.status === 401;
}
