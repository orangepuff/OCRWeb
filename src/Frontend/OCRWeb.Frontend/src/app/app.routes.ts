import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./landing/landing').then((m) => m.Landing)
  },
  {
    path: 'auth-error',
    loadComponent: () => import('./auth-error/auth-error-page').then((m) => m.AuthErrorPage)
  },
  {
    path: 'home',
    canActivate: [authGuard],
    loadComponent: () => import('./home/home').then((m) => m.Home)
  }
];