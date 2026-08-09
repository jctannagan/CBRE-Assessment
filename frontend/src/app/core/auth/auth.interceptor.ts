import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../features/auth/service/auth.service';
import { tokenStorage } from './token-storage';

// Module-scoped so concurrent 401s across requests share one refresh call instead of racing.
let refreshInFlight$: Observable<unknown> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const token = tokenStorage.get();
  const authorizedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authorizedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthEndpoint = req.url.startsWith(`${environment.apiUrl}/auth/`);
      if (!token || error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      if (!refreshInFlight$) {
        refreshInFlight$ = authService.refreshToken().pipe(
          shareReplay(1),
          finalize(() => (refreshInFlight$ = null)),
        );
      }

      return refreshInFlight$.pipe(
        switchMap(() => {
          const newToken = tokenStorage.get();
          const retriedReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
          return next(retriedReq);
        }),
        catchError((refreshError) => {
          authService.logout();
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
