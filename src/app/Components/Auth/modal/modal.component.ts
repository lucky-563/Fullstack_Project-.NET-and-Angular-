import { Component, Input } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-modal',
  template: `
    <div class="modal-header">
      <h4 class="modal-title">{{ modalTitle }}</h4>
      <button type="button" class="close" aria-label="Close" (click)="close()">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>
    <div class="modal-body">
      <p>{{ modalMessage }}</p>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-secondary" (click)="close()">
        Close
      </button>
    </div>
  `,
  styleUrls: ['./modal.component.css'],
})
export class ModalComponent {
  @Input() modalTitle: string = '';
  @Input() modalMessage: string = '';

  constructor(private modalService: NgbModal) {}

  close() {
    this.modalService.dismissAll(); // Close the modal
  }
}
