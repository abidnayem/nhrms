import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const permissionGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const auth = inject(AuthService);
  if (!auth.isAuthenticated) return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  return auth.hasPermission(String(route.data['permission'])) ? true : router.createUrlTree(['/forbidden']);
};
