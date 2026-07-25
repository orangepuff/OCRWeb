import { Routes } from '@angular/router';
import { PORTAL_SHELL_ROUTES, authGuard } from '@orangepuff/portal-frontend';
import { ProjectForm } from './pages/project-form/project-form';
import { ProjectList } from './pages/project-list/project-list';

export const routes: Routes = [
  ...PORTAL_SHELL_ROUTES.filter((r) => r.path !== 'home'),
  { path: 'home', component: ProjectList, canActivate: [authGuard] },
  { path: 'projects/add', component: ProjectForm, canActivate: [authGuard] },
  { path: 'projects/:id/edit', component: ProjectForm, canActivate: [authGuard] },
  { path: '**', redirectTo: 'home' }
];
