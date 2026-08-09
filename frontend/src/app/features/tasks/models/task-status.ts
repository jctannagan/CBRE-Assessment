export const TASK_STATUSES = ['ToDo', 'InProgress', 'Completed'] as const;
export type TaskItemStatus = (typeof TASK_STATUSES)[number];

export const TASK_STATUS_LABELS: Record<TaskItemStatus, string> = {
  ToDo: 'To Do',
  InProgress: 'In Progress',
  Completed: 'Completed',
};
