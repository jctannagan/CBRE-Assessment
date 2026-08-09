import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
    {
        path: '',
        pathMatch: 'full',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/pages/login/login.component').then(c => c.LoginComponent)
    },
    {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/pages/login/login.component').then(c => c.LoginComponent)
    },
    {
        path: 'register',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/pages/register/register.component').then(c => c.RegisterComponent)
    },
    {
        path: 'tasks',
        canActivate: [authGuard],
        loadComponent: () => import('./features/tasks/pages/tasks-page/tasks-page.component').then(c => c.TasksPageComponent)
    }
];
