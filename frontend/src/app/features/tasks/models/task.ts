import { TaskPriority } from './task-priority';
import { TaskItemStatus } from './task-status';

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  dueDate: string | null;
  priority: TaskPriority;
  categoryId: string | null;
  categoryName: string | null;
  status: TaskItemStatus;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  dueDate: string | null;
  priority: TaskPriority;
  categoryId: string | null;
  status: TaskItemStatus;
}

export type UpdateTaskRequest = CreateTaskRequest;
