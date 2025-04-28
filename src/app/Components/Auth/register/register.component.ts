import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthenticateService } from 'src/app/Services/auth/authenticate.service';
import { ModalComponent } from '../modal/modal.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string = '';
  passwordFieldType: string = 'password';

  constructor(
    private fb: FormBuilder,
    private authService: AuthenticateService,
    private router: Router,
    private modalService: NgbModal
  ) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  // Getter to access form controls
  get f() {
    return this.registerForm.controls;
  }

  // Handle registration logic
  onRegister(): void {
    const { username, email, password } = this.registerForm.value;

    this.authService.register(username, email, password).subscribe(
      (response) => {
        const modalRef = this.modalService.open(ModalComponent);
        modalRef.componentInstance.modalTitle = 'Registration Successful';
        modalRef.componentInstance.modalMessage =
          'You have registered successfully! Redirecting to login page...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      (error) => {
        const apiErrors = error.error;

        // If there are validation errors, extract and show the first one
        const modalRef = this.modalService.open(ModalComponent);
        modalRef.componentInstance.modalTitle = 'Registration Failed';

        if (apiErrors && apiErrors.length > 0) {
          // Get the first error description and display it
          modalRef.componentInstance.modalMessage = apiErrors[0].description;
          modalRef.componentInstance.modalMessage = apiErrors[1].description;
        } else {
          // Default error message if no specific error is returned
          modalRef.componentInstance.modalMessage =
            'Registration failed. Please check the input fields.';
        }
      }
    );
  }

  togglePasswordVisibility() {
    this.passwordFieldType =
      this.passwordFieldType === 'password' ? 'text' : 'password';
  }
}
