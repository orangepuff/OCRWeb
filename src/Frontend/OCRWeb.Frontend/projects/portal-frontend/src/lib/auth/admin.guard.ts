import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const adminGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.checkSession().pipe(
    map(() => {
      if (!authService.isAuthenticated()) {
        authService.login(state.url);
        return false;
      }

      if (authService.currentUser()?.isAdmin) {
        return true;
      }

      return router.parseUrl('/unauthorized');
    })
  );
};
