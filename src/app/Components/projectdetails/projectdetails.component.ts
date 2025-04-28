import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ProjectService } from 'src/app/Services/Project/project.service';
import * as bootstrap from 'bootstrap';
import { SharedService } from 'src/app/Services/shared/shared.service';
import { TransactionService } from 'src/app/Services/Transaction/transaction.service';
@Component({
  selector: 'app-projectdetails',
  templateUrl: './projectdetails.component.html',
  styleUrls: ['./projectdetails.component.css'],
})
export class ProjectdetailsComponent {
  projectDetails: any[] = [];
  isLoading: boolean = true;
  createProjectForm!: FormGroup;
  projectId: number = 0; // Initialize as a number
  newProjectName: any = '';
  userName: any = '';
  projectIdToDelete: number | null = null;
  DepositForm!: FormGroup;
  selectedprojectName: string = '';
  accountName: string = '';

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private modalService: NgbModal,
    private sharedService: SharedService,
    private transactionService: TransactionService
  ) {}

  ngOnInit(): void {
    this.fetchProjectDetails();
    this.userName = this.sharedService.getUserName();
    this.createProjectForm = this.fb.group({
      projectName: ['', [Validators.required]],
      budget: [null, [Validators.required, Validators.min(1)]],
    });

    this.DepositForm = this.fb.group({
      projectName: [''],
      accountName: [''],
      amount: [null, [Validators.required, Validators.min(1)]],
    });
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

  openCreateProjectModal(): void {
    const modalElement = document.getElementById('createProjectModal');
    if (modalElement) {
      // Directly use the global bootstrap object (from the CDN)
      const modal = new window.bootstrap.Modal(modalElement);
      modal.show();
    }
  }

  onSubmitCreateProject(): void {
    if (this.createProjectForm.valid) {
      const projectDto = this.createProjectForm.value;
      console.log('started created project');
      this.projectService.createProject(projectDto).subscribe(
        (response) => {
          console.log('Project created successfully:', response);
          // Optionally reload project details
          window.location.reload(); // Redirect to refresh project details
        },
        (error) => {
          console.error('Failed to create project:', error);
        }
      );
    }
  }

  openDeleteConfirmationModal(projectId: number): void {
    console.log(projectId);
    this.projectIdToDelete = projectId;
    console.log('Project ID to delete:', this.projectIdToDelete); // Debugging log
    const modalElement = document.getElementById('deleteConfirmationModal');
    if (modalElement) {
      const modal = new bootstrap.Modal(modalElement);
      modal.show();
    }
  }

  deleteProject(projectId: number | null): void {
    if (this.projectIdToDelete !== null) {
      this.projectService.deleteProject(this.projectIdToDelete).subscribe(
        (response) => {
          console.log('Project deleted successfully:', response);
          this.fetchProjectDetails(); // Refresh project list
        },
        (error) => {
          console.error('Failed to delete project:', error);

          // Extract error message from response or use a fallback message
          const errorMessage =
            error?.error?.message ||
            'An unexpected error occurred while trying to delete the project.';

          // Show error modal with the extracted message
          this.openErrorModal('Delete Failed', errorMessage);
        }
      );
    } else {
      console.error('No project ID provided for deletion.');
      this.openErrorModal(
        'Delete Failed',
        'No project ID provided for deletion.'
      );
    }
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

  openDepositModal(projectName: string): void {
    this.selectedprojectName = projectName;
    this.accountName = 'Master';
    const modalElement = document.getElementById('depositformModal');
    if (modalElement) {
      const modal = new bootstrap.Modal(modalElement);
      modal.show();
    }
  }

  onSubmitDeposit(): void {
    if (this.DepositForm.valid) {
      const DepositData = this.DepositForm.value;
      DepositData.projectName = this.selectedprojectName; // Ensure projectName is included
      DepositData.accountName = this.accountName;
      console.log('Submitting account data:', DepositData);

      this.transactionService.Deposit(DepositData).subscribe({
        next: (response) => {
          console.log('Account added successfully:', response);

          const modalElement = document.getElementById('depositformModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
          this.DepositForm.reset();
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

  openUpdateProjectModal(projectid: number): void {
    this.projectId = projectid;

    const modalElement = document.getElementById('updateProjectModal');
    if (modalElement) {
      const modal = new bootstrap.Modal(modalElement);
      modal.show();
    }
  }
  onSubmitUpdateProjectName(): void {
    if (!this.newProjectName.trim()) {
      alert('Project name cannot be empty.');
      return;
    }

    console.log(this.projectId, this.newProjectName);

    this.projectService
      .updateProjectName(this.projectId, this.newProjectName)
      .subscribe({
        next: (response) => {
          // Handle the response if needed
          console.log('Project name updated successfully:', response);
          alert(response);
          // Close the modal after successful submission
          const modalElement = document.getElementById('updateProjectModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
        },
        error: (err) => {
          console.error('Error updating project name:', err);
          const modalElement = document.getElementById('updateProjectModal');
          if (modalElement) {
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance?.hide();
          }
          this.fetchProjectDetails();
        },
      });
  }
}
