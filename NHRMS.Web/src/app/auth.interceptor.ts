import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_KEY = 'nhrms.accessToken';
const EXPIRY_KEY = 'nhrms.expiresAtUtc';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = sessionStorage.getItem(TOKEN_KEY);
  const expiresAt = new Date(sessionStorage.getItem(EXPIRY_KEY) ?? 0).getTime();
  if (!token || expiresAt <= Date.now()) return next(request);
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
