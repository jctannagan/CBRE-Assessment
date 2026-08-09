import { TASK_STATUSES } from './task-status';
import { TaskItem } from './task';

export const TASK_SORT_OPTIONS = ['dueDateAsc', 'dueDateDesc', 'statusAsc', 'statusDesc'] as const;
export type TaskSortOption = (typeof TASK_SORT_OPTIONS)[number];

export const TASK_SORT_LABELS: Record<TaskSortOption, string> = {
  dueDateAsc: 'Due Date (soonest first)',
  dueDateDesc: 'Due Date (latest first)',
  statusAsc: 'Status (To Do → Completed)',
  statusDesc: 'Status (Completed → To Do)',
};

const statusRank: Record<string, number> = Object.fromEntries(
  TASK_STATUSES.map((status, index) => [status, index]),
);

function compareDueDate(a: TaskItem, b: TaskItem, direction: 1 | -1): number {
  if (a.dueDate === null && b.dueDate === null) return 0;
  if (a.dueDate === null) return 1;
  if (b.dueDate === null) return -1;
  return direction * (new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
}

function compareStatus(a: TaskItem, b: TaskItem, direction: 1 | -1): number {
  return direction * (statusRank[a.status] - statusRank[b.status]);
}

const comparators: Record<TaskSortOption, (a: TaskItem, b: TaskItem) => number> = {
  dueDateAsc: (a, b) => compareDueDate(a, b, 1),
  dueDateDesc: (a, b) => compareDueDate(a, b, -1),
  statusAsc: (a, b) => compareStatus(a, b, 1),
  statusDesc: (a, b) => compareStatus(a, b, -1),
};

export function sortTasks(tasks: TaskItem[], option: TaskSortOption): TaskItem[] {
  return [...tasks].sort(comparators[option]);
}
