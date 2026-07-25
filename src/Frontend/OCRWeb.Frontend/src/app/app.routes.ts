import { Routes } from '@angular/router';
import { PORTAL_SHELL_ROUTES, authGuard } from '@orangepuff/portal-frontend';
import { AddProject } from './pages/add-project/add-project';
import { ProjectList } from './pages/project-list/project-list';

export const routes: Routes = [
  ...PORTAL_SHELL_ROUTES.filter((r) => r.path !== 'home'),
  { path: 'home', component: ProjectList, canActivate: [authGuard] },
  { path: 'projects/add', component: AddProject, canActivate: [authGuard] },
  { path: '**', redirectTo: 'home' }
];
