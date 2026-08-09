import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';

declare const google: any;

@Injectable({ providedIn: 'root' })
export class GoogleIdentityService {
  private initialized = false;
  private readonly credential$ = new Subject<string>();

  private ensureInitialized(): void {
    if (this.initialized) {
      return;
    }

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (response: { credential: string }) => this.credential$.next(response.credential),
    });
    this.initialized = true;
  }

  /**
   * Renders the standard "Sign in with Google" button into the given container.
   * Unlike One Tap's `prompt()`, this opens Google's account chooser and works
   * even when the browser has no existing Google session.
   */
  renderButton(container: HTMLElement, options?: Record<string, unknown>): Observable<string> {
    this.ensureInitialized();

    google.accounts.id.renderButton(container, {
      theme: 'outline',
      size: 'large',
      width: 300,
      ...options,
    });

    return this.credential$.asObservable();
  }
}
