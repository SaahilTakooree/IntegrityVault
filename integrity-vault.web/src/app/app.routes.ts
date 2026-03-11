import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';


export const routes : Routes = [
    {
    path : '',
    redirectTo : 'admin',
    pathMatch : 'full'
  },
  {
    path : 'login',
    loadComponent : () => import("./features/auth/login/login").then(m => m.Login)
  },
  {
    path : 'superadmin',
    loadComponent : () => import("./features/dashboards/super-admin/super-admin").then(m => m.SuperadminDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data : { roles : ["SuperAdmin"]}
  },
  {
    path : 'admin',
    loadComponent : () => import("./features/dashboards/admin/admin").then(m => m.AdminDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data : { roles : ["Admin"]}
  },
  // {
  //   path : 'doctor',
  //   loadComponent : () => import("./features/dashboards/doctor/doctor").then(m => m.DoctorDashboardComponent),
  //   canActivate: [authGuard, roleGuard],
  //   data : { roles : ["Doctor"]}
  // },
  // {
  //   path : 'patient',
  //   loadComponent : () => import("./features/dashboards/patient/patient").then(m => m.PatientDashboardComponent),
  //   canActivate: [authGuard, roleGuard],
  //   data : { roles : ["Patient"]}
  // },
  // {
  //   path : 'external-provider',
  //   loadComponent : () => import("./features/dashboards/external-provider/external-provider").then(m => m.ExternalProviderDashboardComponent),
  //   canActivate: [authGuard, roleGuard],
  //   data : { roles : ["ExternalProvider"]}
  // },
  {
    path: '**',
    redirectTo: 'login'
  }
]
