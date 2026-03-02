import { Routes } from '@angular/router';
import { Login } from '../features/login/login';

export const routes: Routes = [
    {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full' // important for full URL match
  },
  {
    path: 'login',
    component: Login
  }
]
