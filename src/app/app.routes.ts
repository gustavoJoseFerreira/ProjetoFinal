import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AccountComponent } from './account/account.component';
import { TransferComponent } from './transfer/transfer.component';
import { AlertComponent } from './alert/alert.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { DashboardComponent } from './dashboard/dashboard.component';


export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent},
  { path: 'register', component: RegisterComponent},
  { path: 'dashboard', component: DashboardComponent  },
  { path: 'account', component: AccountComponent},
  { path: 'transfer', component: TransferComponent},
  { path: 'alerts', component: AlertComponent},

  { path: '**', redirectTo: 'login' }
];