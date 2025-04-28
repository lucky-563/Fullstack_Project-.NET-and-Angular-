import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ProjectService } from 'src/app/Services/Project/project.service';
import { SharedService } from 'src/app/Services/shared/shared.service';
import * as bootstrap from 'bootstrap';
import { AccountService } from 'src/app/Services/Account/account.service';
import { TransactionService } from 'src/app/Services/Transaction/transaction.service';
import { RequestsService } from 'src/app/Services/Request/requests.service';
import { HttpClient } from '@angular/common/http';
@Component({
  selector: 'app-account-details',
  templateUrl: './account-details.component.html',
  styleUrls: ['./account-details.component.css'],
})
export class AccountDetailsComponent {
  isLoading: boolean = true;
  userName: any = '';
  projectDetails: any[] = [];
  account: any;
  addAccountForm: FormGroup;
  WithDrawForm: FormGroup;
  selectedProjectName: string = '';
  showHoverDialog = false;
  accountName: string = '';
  transferForm: FormGroup;
  requestForm: FormGroup;
  userList: string[] = [];
  accountId: number = 0;
  newManagerName: string = '';
  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private modalService: NgbModal,
    private sharedService: SharedService,
    private accountService: AccountService,
    private transactionService: TransactionService,
    private requestsService: RequestsService,
    private http: HttpClient
  ) {
    this.addAccountForm = this.fb.group({
      accountName: ['', Validators.required],
      managerName: ['', Validators.required],
    });

    this.WithDrawForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(1)]],
      remarks: [null, [Validators.required]],
    });

    this.transferForm = this.fb.group({
      projectName: [''],
      fromAccountName: ['', Validators.required],
      toAccountName: ['', Validators.required],
      amount: [null, [Validators.required, Validators.min(1)]],
    });

    this.requestForm = this.fb.group({
      projectName: [''],
      fromAccountName: ['', Validators.required],
      toAccountName: [''],
      amount: [null, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.fetchProjectDetails();
    this.userName = this.sharedService.getUserName();
    this.fetchAndStoreUsers();
  }

  fetchProjectDetails(): void {
    this.projectService.getProjectDetails().subscribe(
      (data) => {
        this.projectDetails = data;
        this.isLoading = false;
      },
      (error) => {
        console.error('Error fetching project details', error);
        this.isLoading = false;
      }
    );
  }

  // Open modal for adding account

  openAddAccountModal(projectName: string) {
    this.selectedProjectName = projectName; // Set project name

    const modalElement = document.getElementById('addAccountModal');
    if (modalElement) {
      const modalInstance = new bootstrap.Modal(modalElement);
      modalInstance.show();
    }
  }
  openWithDrawModal(projectName: string, accountName: string) {
    this.selectedProjectName = projectName;
    this.accountName = accountName;
    const modalElement = document.getElementById('WithDraw');
    if (modalElement) {
      const modalInstance = new bootstrap.Modal(modalElement);
      modalInstance.show();
    }
  }

  onWithDrawSubmit(): void {
    if (this.WithDrawForm.valid) {
      const Data = this.WithDrawForm.value;
      Data.projectName = this.selectedProjectName;
      Data.accountName = this.accountName;
      console.log('Form Submitted', this.WithDrawForm.value);
      // Process the withdrawal here

      this.transactionService.WithDrawAmount(Data).subscribe({
        next: (response) => {
          console.log('Account added successfully:', response);

          const modalElement = document.getElementById('WithDraw');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
          this.WithDrawForm.reset();
        },
        error: (error) => {
          // Log the error message from the API response
          if (error.error && error.error.message) {
            console.error('Error adding account:', error.error.message);
          } else {
            console.error('Error adding account:', error); // Fallback for unexpected error structure
          }
          alert('Failed to add account. Please try again.');
        },
      });
    }
  }

  openTransferModel(projectname: string) {
    this.selectedProjectName = projectname;
    const modalElement = document.getElementById('TransferiD');
    if (modalElement) {
      const modalInstance = new bootstrap.Modal(modalElement);
      modalInstance.show();
    }
  }
  onTransferSubmit() {
    if (this.transferForm.valid) {
      const Data = this.transferForm.value;
      Data.projectName = this.selectedProjectName;
      console.log('Form Submitted:', Data);
      this.transactionService.TransferAmount(Data).subscribe({
        next: (response) => {
          console.log('Transfered Amount successfully:', response);

          const modalElement = document.getElementById('TransferiD');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
          this.transferForm.reset();
        },
        error: (error) => {
          // Log the error message from the API response
          const modalElement = document.getElementById('TransferiD');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
        },
      });
    }
  }

  openRequestModel(projectname: string, toAccountName: string) {
    this.selectedProjectName = projectname;
    this.accountName = toAccountName;
    const modalElement = document.getElementById('Requestmodel');
    if (modalElement) {
      const modalInstance = new bootstrap.Modal(modalElement);
      modalInstance.show();
    }
  }

  onRequestSubmit() {
    if (this.requestForm.valid) {
      const Data = this.requestForm.value;
      Data.projectName = this.selectedProjectName;
      Data.toAccountName = this.accountName;
      console.log('Form Submitted:', Data);
      this.requestsService.RequestAmount(Data).subscribe({
        next: (response) => {
          console.log('Request Amount Amount successfully:', response);

          const modalElement = document.getElementById('Requestmodel');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
          this.requestForm.reset();
        },
        error: (error) => {
          // Log the error message from the API response
          if (error.error && error.error.message) {
            console.error('Error Transfer amount:', error.error.message);
          } else {
            console.error('Error Transfer amount:', error); // Fallback for unexpected error structure
          }
          alert('Failed to request account. Please try again.');
        },
      });
    }
  }

  // Handle form submission
  onAddAccountSubmit() {
    if (this.addAccountForm.valid) {
      const accountData = this.addAccountForm.value;
      accountData.projectName = this.selectedProjectName; // Ensure projectName is included

      console.log('Submitting account data:', accountData);

      this.accountService.addAccount(accountData).subscribe({
        next: (response) => {
          console.log('Account added successfully:', response);

          const modalElement = document.getElementById('addAccountModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
          this.addAccountForm.reset();
        },
        error: (error) => {
          // Log the error message from the API response
          if (error.error && error.error.message) {
            console.error('Error adding account:', error.error.message);
          } else {
            console.error('Error adding account:', error); // Fallback for unexpected error structure
          }
          alert('Failed to add account. Please try again.');
        },
      });
    }
  }

  onDeleteAccount(accountId: number) {
    this.accountService.deleteAccount(accountId).subscribe({
      next: () => {
        console.log(`Account with ID ${accountId} deleted successfully.`);

        // Optionally refresh the list or update the UI
        this.fetchProjectDetails();
      },
      error: (error) => {
        console.error('Failed to delete Account:', error);

        // Extract error message from response or use a fallback message
        const errorMessage =
          error?.error?.message ||
          'An unexpected error occurred while trying to Account the project.';

        // Show error modal with the extracted message
        this.openErrorModal('Delete Failed', errorMessage);
      },
    });
  }
  openErrorModal(title: string, message: string): void {
    const modalElement = document.getElementById('errorModal');
    if (modalElement) {
      modalElement.querySelector('.modal-title')!.textContent = title;
      modalElement.querySelector('.modal-body')!.textContent = message;
      const modal = new bootstrap.Modal(modalElement);
      modal.show();
    }
  }

  fetchAndStoreUsers(): void {
    const apiUrl = 'http://localhost:5030/api/Auth/usersList'; // Replace with your API endpoint
    this.http.get<string[]>(apiUrl).subscribe({
      next: (data) => {
        this.userList = data; // Store the fetched data in the userList variable
        console.log('Fetched users:', this.userList);
      },
      error: (err) => {
        console.error('Error fetching users:', err);
      },
    });
  }

  openUpdateManagerModal(accountId: number): void {
    this.accountId = accountId;

    const modalElement = document.getElementById('updateManagerModal');
    if (modalElement) {
      const modal = new bootstrap.Modal(modalElement);
      modal.show();
    }
  }

  onSubmitUpdateManager(): void {
    if (!this.newManagerName.trim()) {
      alert('Manager name cannot be empty.');
      return;
    }

    console.log(this.accountId, this.newManagerName);

    this.accountService
      .updateAccountManager(this.accountId, this.newManagerName)
      .subscribe({
        next: (response) => {
          console.log('Manager updated successfully:', response);
          alert(response);

          const modalElement = document.getElementById('updateManagerModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
        },
        error: (err) => {
          const modalElement = document.getElementById('updateManagerModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
        },
      });
  }
}
