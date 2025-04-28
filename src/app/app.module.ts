import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RegisterComponent } from './Components/Auth/register/register.component';
import { LoginComponent } from './Components/Auth/login/login.component';
import { WelcomeComponent } from './Components/Dashboard/welcome/welcome.component';
import { ModalComponent } from './Components/Auth/modal/modal.component';
import { TokenIntercept } from './Interceptor/token-intercept';
import { NavHeaderComponent } from './Components/nav-header/nav-header.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SidebarComponent } from './Components/sidebar/sidebar.component';
import { ProjectdetailsComponent } from './Components/projectdetails/projectdetails.component';
import { AccountDetailsComponent } from './Components/account-details/account-details.component';
import { TransactionDetailsComponent } from './Components/Transactions/transaction-details/transaction-details.component';
import { RequestDetailsComponent } from './Components/request-details/request-details.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    WelcomeComponent,
    ModalComponent,
    NavHeaderComponent,
    SidebarComponent,
    ProjectdetailsComponent,
    AccountDetailsComponent,
    TransactionDetailsComponent,
    RequestDetailsComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    NgbModule,
    BrowserAnimationsModule,
    FormsModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenIntercept,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
