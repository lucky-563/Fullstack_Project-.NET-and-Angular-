import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private baseUrl = 'http://localhost:5030/api/Transaction';
  constructor(private http: HttpClient) {}

  WithDrawAmount(withDrawDto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/withdraw`, withDrawDto);
  }

  Deposit(DepositDto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/deposit`, DepositDto);
  }

  TransferAmount(TransferDto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/transfer`, TransferDto);
  }
}
