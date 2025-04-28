import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { share } from 'rxjs';
import { SharedService } from 'src/app/Services/shared/shared.service';

@Component({
  selector: 'app-nav-header',
  templateUrl: './nav-header.component.html',
  styleUrls: ['./nav-header.component.css'],
})
export class NavHeaderComponent {
  userName: any = '';

  @Output() toggleSidenav = new EventEmitter<void>();

  constructor(private router: Router, private sharedService: SharedService) {}

  ngOnInit(): void {
    this.userName = this.sharedService.getUserName();
  }
  logout() {
    // Implement logout logic here (e.g., clearing tokens, redirecting)
    this.sharedService.clearUserData();
    this.router.navigate(['/login']);
  }
}
