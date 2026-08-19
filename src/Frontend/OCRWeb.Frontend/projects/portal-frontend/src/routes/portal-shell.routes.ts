import { Route } from '@angular/router';
import { authGuard } from '../lib/auth/auth.guard';
import { adminGuard } from '../lib/auth/admin.guard';
import { ownUserOrAdminGuard } from '../lib/auth/own-user-or-admin.guard';

export const PORTAL_SHELL_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('../lib/landing/landing').then((m) => m.Landing)
  },
  {
    path: 'auth-error',
    loadComponent: () => import('../lib/auth-error/auth-error-page').then((m) => m.AuthErrorPage)
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('../lib/unauthorized/unauthorized-page').then((m) => m.UnauthorizedPage)
  },
  {
    path: 'home',
    canActivate: [authGuard],
    loadComponent: () => import('../lib/home/home').then((m) => m.Home)
  },
  {
    path: 'admin/users',
    canActivate: [adminGuard],
    loadComponent: () => import('../lib/admin/users/user-list/user-list').then((m) => m.UserList)
  },
  {
    path: 'admin/security-rule-categories',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('../lib/admin/security-rule-categories/security-rule-category-list/security-rule-category-list').then(
        (m) => m.SecurityRuleCategoryList
      )
  },
  {
    path: 'admin/security-rule-items',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('../lib/admin/security-rule-items/security-rule-item-list/security-rule-item-list').then(
        (m) => m.SecurityRuleItemList
      )
  },
  {
    path: 'admin/themes',
    canActivate: [adminGuard],
    loadComponent: () => import('../lib/admin/themes/theme-page').then((m) => m.ThemePage)
  },
  {
    path: 'admin/config',
    canActivate: [adminGuard],
    loadComponent: () => import('../lib/admin/config/config-admin-page/config-admin-page').then((m) => m.ConfigAdminPage)
  },
  {
    path: 'admin/config-text',
    canActivate: [adminGuard],
    loadComponent: () => import('../lib/admin/config-text/config-text-list/config-text-list').then((m) => m.ConfigTextList)
  },
  {
    path: 'admin/config-data',
    canActivate: [adminGuard],
    loadComponent: () => import('../lib/admin/config-data/config-data-list/config-data-list').then((m) => m.ConfigDataList)
  },
  {
    path: 'Users/:userId/Settings',
    canActivate: [ownUserOrAdminGuard],
    loadComponent: () => import('../lib/settings/settings-page').then((m) => m.SettingsPage)
  }
];
