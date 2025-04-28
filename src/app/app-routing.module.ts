import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './Components/Auth/login/login.component';
import { WelcomeComponent } from './Components/Dashboard/welcome/welcome.component';
import { RegisterComponent } from './Components/Auth/register/register.component';
import { ProjectdetailsComponent } from './Components/projectdetails/projectdetails.component';
import { AccountDetailsComponent } from './Components/account-details/account-details.component';
import { TransactionDetailsComponent } from './Components/Transactions/transaction-details/transaction-details.component';
import { RequestDetailsComponent } from './Components/request-details/request-details.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'welcome', component: WelcomeComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'project-details', component: ProjectdetailsComponent },
  { path: 'account-details', component: AccountDetailsComponent },
  { path: 'Tranaction-details', component: TransactionDetailsComponent },
  { path: 'request-details', component: RequestDetailsComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
