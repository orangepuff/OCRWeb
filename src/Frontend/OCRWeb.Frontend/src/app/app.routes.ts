import { Routes } from '@angular/router';
import { PORTAL_SHELL_ROUTES, authGuard } from '@orangepuff/portal-frontend';

export const routes: Routes = [
  ...PORTAL_SHELL_ROUTES.filter((r) => r.path !== 'home'),
  {
    path: 'home',
    loadComponent: () => import('./pages/project-list/project-list').then((m) => m.ProjectList),
    canActivate: [authGuard]
  },
  {
    path: 'projects/add',
    loadComponent: () => import('./pages/project-form/project-form').then((m) => m.ProjectForm),
    canActivate: [authGuard]
  },
  {
    path: 'projects/:id/edit',
    loadComponent: () => import('./pages/project-form/project-form').then((m) => m.ProjectForm),
    canActivate: [authGuard]
  },
  {
    path: 'projects/:id/crop',
    loadComponent: () => import('./pages/crop-pdf/crop-pdf').then((m) => m.CropPdf),
    canActivate: [authGuard]
  },
  {
    path: 'projects/:id/split',
    loadComponent: () => import('./pages/split-pdf/split-pdf').then((m) => m.SplitPdf),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'home' }
];
