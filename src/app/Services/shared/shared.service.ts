import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SharedService {
  private userData: any = null;

  constructor() {}

  // Save user data after login
  setUserData(data: any): void {
    this.userData = {
      email: data.email,
      userName: data.userName,
      token: data.token,
    };
    localStorage.setItem('userData', JSON.stringify(this.userData));
  }

  // Retrieve user data
  getUserData(): any {
    if (!this.userData) {
      const storedData = localStorage.getItem('userData');
      this.userData = storedData ? JSON.parse(storedData) : null;
    }
    return this.userData;
  }

  // Retrieve specific user info
  getEmail(): string | null {
    return this.getUserData()?.email || null;
  }

  getUserName(): string | null {
    return this.getUserData()?.userName || null;
  }

  getToken(): string | null {
    return this.getUserData()?.token || null;
  }

  // Clear user data on logout
  clearUserData(): void {
    this.userData = null;
    localStorage.removeItem('userData');
  }
}
