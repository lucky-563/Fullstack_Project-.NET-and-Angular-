import { Component, OnInit } from '@angular/core';
import { ProjectService } from 'src/app/Services/Project/project.service';
import { SharedService } from 'src/app/Services/shared/shared.service';
import { TransactionService } from 'src/app/Services/Transaction/transaction.service';

@Component({
  selector: 'app-transaction-details',
  templateUrl: './transaction-details.component.html',
  styleUrls: ['./transaction-details.component.css'],
})
export class TransactionDetailsComponent implements OnInit {
  projectDetails: any[] = []; // All project details
  isLoading: boolean = true; // Loading indicator
  userName: any = ''; // Current user's name
  errorMessage: string = ''; // Error message
  transactions: any[] = []; // To store the transactions for the selected account
  selectedAccount: any = null; // To store the currently selected account for showing transactions
  selectedAccountTransactions: any[] = []; // To store the transactions for the selected account only

  constructor(
    private projectService: ProjectService,
    private sharedService: SharedService,
    private transactionService: TransactionService
  ) {}

  ngOnInit(): void {
    this.userName = this.sharedService.getUserName(); // Retrieve current user's name
    this.fetchProjectDetails(); // Fetch project and transaction data
  }

  /**
   * Fetches all project details.
   */
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

  /**
   * Opens the statement for the selected account and fetches its transactions.
   * @param managerName Name of the manager
   * @param accountName Name of the account
   * @param projectName Name of the project
   */
  opensatement(
    managerName: string,
    accountName: string,
    projectName: string
  ): void {
    // Fetch the transactions for the given account and project
    this.projectService
      .getTransactionStatements(managerName, accountName, projectName)
      .subscribe(
        (data) => {
          this.selectedAccountTransactions = data.transactions; // Store transactions for this account
          this.selectedAccount = { managerName, accountName, projectName }; // Store the selected account details
          this.openModal(); // Open the modal
        },
        (error) => {
          console.error('Error fetching transactions', error);
        }
      );
  }

  /**
   * Opens the modal for showing the transaction statement.
   */
  openModal(): void {
    const modal = document.getElementById('transactionModal') as any;
    if (modal) {
      modal.style.display = 'block'; // Open the modal
    }
  }

  /**
   * Closes the modal.
   */
  closeModal(): void {
    const modal = document.getElementById('transactionModal') as any;
    if (modal) {
      modal.style.display = 'none'; // Close the modal
    }
  }
}
