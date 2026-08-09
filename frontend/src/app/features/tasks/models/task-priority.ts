export const TASK_PRIORITIES = ['Low', 'Medium', 'High'] as const;
export type TaskPriority = (typeof TASK_PRIORITIES)[number];

export const TASK_PRIORITY_LABELS: Record<TaskPriority, string> = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
};
