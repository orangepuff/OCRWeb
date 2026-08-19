import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

/** Allows the route only for the signed-in user viewing their own :userId, or an admin viewing anyone's. */
export const ownUserOrAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.checkSession().pipe(
    map(() => {
      if (!authService.isAuthenticated()) {
        authService.login(state.url);
        return false;
      }

      const currentUser = authService.currentUser()!;
      const targetUserId = route.paramMap.get('userId');

      if (currentUser.isAdmin || currentUser.userId === targetUserId) {
        return true;
      }

      return router.parseUrl('/unauthorized');
    })
  );
};
