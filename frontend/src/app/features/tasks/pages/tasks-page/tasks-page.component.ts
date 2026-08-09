import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthService } from '../../../auth/service/auth.service';
import { TaskListComponent } from '../../components/task-list/task-list.component';
import { TaskFormComponent } from '../../components/task-form/task-form.component';
import { Category } from '../../models/category';
import { TaskItem } from '../../models/task';
import { sortTasks, TASK_SORT_LABELS, TASK_SORT_OPTIONS, TaskSortOption } from '../../models/task-sort';
import { CategoryService } from '../../service/category.service';
import { TaskService } from '../../service/task.service';

@Component({
  selector: 'app-tasks-page',
  imports: [TaskListComponent, TaskFormComponent],
  templateUrl: './tasks-page.component.html',
  styleUrls: ['./tasks-page.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TasksPageComponent {
  protected readonly authService = inject(AuthService);
  private readonly taskService = inject(TaskService);
  private readonly categoryService = inject(CategoryService);

  readonly newTaskButton = viewChild<ElementRef<HTMLButtonElement>>('newTaskButton');

  readonly tasks = signal<TaskItem[]>([]);
  readonly sortOption = signal<TaskSortOption>('dueDateAsc');
  readonly sortedTasks = computed(() => sortTasks(this.tasks(), this.sortOption()));

  protected readonly sortOptions = TASK_SORT_OPTIONS;
  protected readonly sortLabels = TASK_SORT_LABELS;

  readonly categories = signal<Category[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly isFormOpen = signal(false);
  readonly editingTask = signal<TaskItem | null>(null);

  readonly deletingTaskId = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  constructor() {
    this.loadTasks();
    this.loadCategories();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.taskService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (tasks) => this.tasks.set(tasks),
        error: () => this.loadError.set('Could not load tasks. Please try again.'),
      });
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (categories) => this.categories.set(categories),
    });
  }

  openCreateForm(): void {
    this.editingTask.set(null);
    this.isFormOpen.set(true);
  }

  openEditForm(task: TaskItem): void {
    this.editingTask.set(task);
    this.isFormOpen.set(true);
  }

  closeForm(): void {
    this.isFormOpen.set(false);
    this.editingTask.set(null);
    this.newTaskButton()?.nativeElement.focus();
  }

  handleFormSaved(): void {
    this.closeForm();
    this.loadTasks();
  }

  handleSortChange(value: string): void {
    this.sortOption.set(value as TaskSortOption);
  }

  handleCategoryCreated(category: Category): void {
    this.categories.update((list) =>
      [...list, category].sort((a, b) => a.name.localeCompare(b.name)),
    );
  }

  requestDelete(id: string): void {
    this.deleteError.set(null);
    this.deletingTaskId.set(id);
  }

  cancelDelete(): void {
    this.deletingTaskId.set(null);
    this.deleteError.set(null);
  }

  confirmDelete(id: string): void {
    this.taskService.delete(id).subscribe({
      next: () => {
        this.tasks.update((list) => list.filter((t) => t.id !== id));
        this.deletingTaskId.set(null);
        this.deleteError.set(null);
      },
      error: () => this.deleteError.set('Could not delete task. Please try again.'),
    });
  }
}
