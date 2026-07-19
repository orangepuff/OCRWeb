import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { adminGuard } from './auth/admin.guard';

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
    canActivate: [adminGuard],
    loadComponent: () => import('./admin/users/user-list/user-list').then((m) => m.UserList)
  },
  {
    path: 'admin/security-rule-categories',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin/security-rule-categories/security-rule-category-list/security-rule-category-list').then(
        (m) => m.SecurityRuleCategoryList
      )
  },
  {
    path: 'admin/security-rule-items',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin/security-rule-items/security-rule-item-list/security-rule-item-list').then((m) => m.SecurityRuleItemList)
  },
  {
    path: 'admin/themes',
    canActivate: [adminGuard],
    loadComponent: () => import('./admin/themes/theme-page').then((m) => m.ThemePage)
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./settings/settings-page').then((m) => m.SettingsPage)
  }
];