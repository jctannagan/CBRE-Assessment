import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { TASK_PRIORITY_LABELS } from '../../models/task-priority';
import { TASK_STATUS_LABELS } from '../../models/task-status';
import { TaskItem } from '../../models/task';

@Component({
  selector: 'app-task-list',
  imports: [DatePipe],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskListComponent {
  private readonly elementRef: ElementRef<HTMLElement> = inject(ElementRef);

  readonly tasks = input.required<TaskItem[]>();
  readonly loading = input(false);
  readonly error = input<string | null>(null);
  readonly deletingTaskId = input<string | null>(null);
  readonly deleteError = input<string | null>(null);

  readonly edit = output<TaskItem>();
  readonly deleteRequested = output<string>();
  readonly deleteConfirmed = output<string>();
  readonly deleteCancelled = output<void>();

  protected readonly priorityLabels = TASK_PRIORITY_LABELS;
  protected readonly statusLabels = TASK_STATUS_LABELS;

  constructor() {
    effect(() => {
      const id = this.deletingTaskId();
      const targetId = id ? `confirm-delete-${id}` : null;
      queueMicrotask(() => {
        const el = targetId
          ? this.elementRef.nativeElement.querySelector<HTMLElement>(`#${CSS.escape(targetId)}`)
          : null;
        el?.focus();
      });
    });
  }

  protected priorityClass(priority: TaskItem['priority']): string {
    return `badge badge-priority-${priority.toLowerCase()}`;
  }

  protected statusClass(status: TaskItem['status']): string {
    return `badge badge-status-${status.toLowerCase()}`;
  }
}
