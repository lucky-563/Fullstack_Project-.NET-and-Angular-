import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { SharedService } from '../Services/shared/shared.service';

@Injectable()
export class TokenIntercept implements HttpInterceptor {
  constructor(private sharedService: SharedService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const token = this.sharedService.getToken();

    if (token) {
      const clonedRequest = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      });
      console.log(clonedRequest);
      return next.handle(clonedRequest);
    }

    return next.handle(req);
  }
}
