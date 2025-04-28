import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private baseUrl = 'http://localhost:5030/api/Project';

  constructor(private http: HttpClient) {}

  // updateProjectName(
  //   projectId: number,
  //   newProjectName: string
  // ): Observable<string> {
  //   const url = `${this.baseUrl}/UpdateProjectName`;
  //   const params = { projectId, newProjectName };

  //   return this.http.put<string>(url, null, { params });
  // }
  // updateProjectName(
  //   projectId: number,
  //   newProjectName: string
  // ): Observable<any> {
  //   // Constructing the URL with query parameters
  //   const url = `${this.baseUrl}/UpdateProjectName?projectId=${projectId}&newProjectName=${newProjectName}`;
  //   console.log(url);
  //   // Sending PUT request to update project name
  //   return this.http.put<any>(url, Response);
  // }

  updateProjectName(
    projectId: number,
    newProjectName: string
  ): Observable<any> {
    const url = `${this.baseUrl}/UpdateProjectName?projectId=${projectId}&newProjectName=${newProjectName}`;
    console.log(url);
    return this.http.put<any>(url, null);
  }
  getProjectDetails(): Observable<any> {
    const url = `${this.baseUrl}/ProjectDetails`;
    return this.http.get<any>(url);
  }

  createProject(projectDto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/NewProject`, projectDto);
  }
  deleteProject(projectId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${projectId}`);
  }

  getTransactionStatements(
    managerName: string,
    accountName: string,
    projectName: string
  ): Observable<any> {
    const url = `${this.baseUrl}/GetAccountstatement?managerName=${managerName}&accountName=${accountName}&projectName=${projectName}`;
    return this.http.get<any>(url);
  }
}
