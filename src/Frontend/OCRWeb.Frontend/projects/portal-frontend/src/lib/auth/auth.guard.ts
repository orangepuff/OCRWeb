import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);

  return authService.checkSession().pipe(
    map(() => {
      if (authService.isAuthenticated()) {
        return true;
      }

      authService.login(state.url);
      return false;
    })
  );
};
