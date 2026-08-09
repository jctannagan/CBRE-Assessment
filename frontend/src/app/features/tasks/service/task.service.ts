import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateTaskRequest, TaskItem, UpdateTaskRequest } from '../models/task';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  getAll(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.baseUrl);
  }

  getById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.baseUrl, payload);
  }

  update(id: string, payload: UpdateTaskRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
