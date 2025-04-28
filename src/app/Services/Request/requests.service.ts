import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class RequestsService {
  private baseUrl = 'http://localhost:5030/api/PaymentRequest';
  constructor(private http: HttpClient) {}

  RequestAmount(requestDto: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/CreatePaymentRequest`,
      requestDto
    );
  }

  // Approve(ApproveDto: any): Observable<any> {
  //   return this.http.delete(
  //     `${this.baseUrl}/ApprovePaymentRequest`,
  //     ApproveDto
  //   );
  // }
  reject(paymentRequestId: number): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/RejectPaymentRequest`,
      paymentRequestId // Sending paymentRequestId directly
    );
  }
  approve(paymentRequestId: number): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/ApprovePaymentRequest`,
      paymentRequestId // Send the integer as the request body
    );
  }

  getAllRequestsWithProjectDetails(): Observable<any> {
    const url = `${this.baseUrl}/GetAllRequestsWithProjectDetails`;
    return this.http.get<any>(url);
  }
}
