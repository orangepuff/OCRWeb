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
  },
  {
    path: 'admin/users',
    canActivate: [authGuard],
    loadComponent: () => import('./admin/users/user-list/user-list').then((m) => m.UserList)
  },
  {
    path: 'admin/security-rule-categories',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./admin/security-rule-categories/security-rule-category-list/security-rule-category-list').then(
        (m) => m.SecurityRuleCategoryList
      )
  },
  {
    path: 'admin/security-rule-items',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./admin/security-rule-items/security-rule-item-list/security-rule-item-list').then((m) => m.SecurityRuleItemList)
  }
];