import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthenticateService } from 'src/app/Services/auth/authenticate.service';
import { SharedService } from 'src/app/Services/shared/shared.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string = '';
  passwordFieldType: string = 'password';
  eyeIcon: string = 'fa-eye-slash';
  constructor(
    private fb: FormBuilder,
    private authService: AuthenticateService,
    private router: Router,
    private sharedService: SharedService
  ) {
    // Initialize the form with validators
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  // Getter for easier access to form controls
  get f() {
    return this.loginForm.controls;
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      return; // Don't proceed if form is invalid
    }

    const { email, password } = this.loginForm.value;

    this.authService.login(email, password).subscribe({
      next: (response) => {
        console.log(response);
        this.sharedService.setUserData(response);
        this.router.navigate(['/welcome']); // Navigate to welcome page
      },
      error: (error) => {
        if (error.status === 401) {
          this.errorMessage = 'Invalid username or password.';
        } else {
          this.errorMessage = 'An unexpected error occurred.';
        }
      },
    });
  }

  togglePasswordVisibility() {
    this.passwordFieldType =
      this.passwordFieldType === 'password' ? 'text' : 'password';
  }
}
