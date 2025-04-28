import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RequestsService } from 'src/app/Services/Request/requests.service';
import { SharedService } from 'src/app/Services/shared/shared.service';

@Component({
  selector: 'app-request-details',
  templateUrl: './request-details.component.html',
  styleUrls: ['./request-details.component.css'],
})
export class RequestDetailsComponent {
  data: any[] = []; // Holds the API response
  isLoading = true; // Loading state
  errorMessage: string | null = null; // Error message state
  currentUser: any = '';
  ActionId: FormGroup;

  constructor(
    private paymentRequestService: RequestsService,
    private sharedservice: SharedService,
    private fb: FormBuilder
  ) {
    this.ActionId = this.fb.group({
      paymentRequestId: [null, [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.fetchData();
    this.currentUser = this.sharedservice.getUserName();
  }
  isApprover(request: any): boolean {
    return (
      request.requestToManger === this.currentUser
      // request.project.createdBy === this.currentUser
    );
  }
  fetchData(): void {
    this.paymentRequestService.getAllRequestsWithProjectDetails().subscribe(
      (response) => {
        this.data = response;
        this.isLoading = false;
      },
      (error) => {
        this.errorMessage = 'Failed to load data.';
        this.isLoading = false;
        console.error(error);
      }
    );
  }

  approveRequest(paymentRequestId: number): void {
    this.paymentRequestService.approve(paymentRequestId).subscribe({
      next: () => {
        console.log('Approval successful.');
        this.fetchData(); // Optionally refresh the list or update the UI
      },
      error: (error) => {
        console.error('Action Failed:', error);
        const errorMessage =
          error?.error?.message ||
          'An unexpected error occurred while trying to approve the request.';
        // Optionally show an error message or modal
      },
    });
  }

  rejectRequest(paymentRequestId: number): void {
    this.paymentRequestService.reject(paymentRequestId).subscribe({
      next: () => {
        console.log('Rejection successful.');
        this.fetchData(); // Optionally refresh the list or update the UI
      },
      error: (error) => {
        console.error('Action Failed:', error);
        const errorMessage =
          error?.error?.message ||
          'An unexpected error occurred while trying to reject the request.';
        // Optionally show an error message or modal
      },
    });
  }
}
