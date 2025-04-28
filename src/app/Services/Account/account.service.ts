import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private apiUrl = 'http://localhost:5030/api/Account';
  constructor(private http: HttpClient) {}

  // Method to send a POST request to add an account
  addAccount(accountData: any): Observable<any> {
    console.log('recieved dto');
    console.log(accountData);
    return this.http.post(this.apiUrl, accountData);
  }

  deleteAccount(accountId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${accountId}`);
  }
  updateAccountManager(
    accountId: number,
    newManagerName: string
  ): Observable<any> {
    const url = `${this.apiUrl}/ChangeManagerDetails?accountId=${accountId}&newManagerName=${newManagerName}`;
    console.log('Request URL:', url);
    return this.http.put<any>(url, null);
  }
}
