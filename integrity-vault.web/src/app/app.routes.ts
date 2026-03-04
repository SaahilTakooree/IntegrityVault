import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { SuperadminDashboardComponent } from './features/dashboards/super-admin/super-admin';

export const routes: Routes = [
    {
    path: '',
    redirectTo: 'superadmin',
    pathMatch: 'full' // important for full URL match
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'superadmin',
    component: SuperadminDashboardComponent
  }
]
