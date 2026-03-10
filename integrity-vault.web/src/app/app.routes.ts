import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { SuperadminDashboardComponent } from './features/dashboards/super-admin/super-admin';
import { AdminDashboardComponent } from './features/dashboards/admin/admin';

export const routes: Routes = [
    {
    path: '',
    redirectTo: 'admin',
    pathMatch: 'full' // important for full URL match
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'superadmin',
    component: SuperadminDashboardComponent
  },
  {
    path: 'admin',
    component: AdminDashboardComponent
  }
]
