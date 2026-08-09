import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, finalize } from 'rxjs';
import { Category } from '../../models/category';
import { CreateTaskRequest, TaskItem } from '../../models/task';
import { TASK_PRIORITIES } from '../../models/task-priority';
import { TASK_STATUSES } from '../../models/task-status';
import { CategoryService } from '../../service/category.service';
import { TaskService } from '../../service/task.service';

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'region',
    tabindex: '-1',
    '[attr.aria-labelledby]': "'task-form-heading'",
  },
})
export class TaskFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly categoryService = inject(CategoryService);
  private readonly elementRef: ElementRef<HTMLElement> = inject(ElementRef);

  readonly task = input<TaskItem | null>(null);
  readonly categories = input.required<Category[]>();

  readonly saved = output<void>();
  readonly cancelled = output<void>();
  readonly categoryCreated = output<Category>();

  readonly newCategoryInput = viewChild<ElementRef<HTMLInputElement>>('newCategoryInput');

  readonly addingCategory = signal(false);
  readonly newCategoryName = signal('');
  readonly creatingCategory = signal(false);
  readonly categoryError = signal<string | null>(null);

  readonly titleInput = viewChild<ElementRef<HTMLInputElement>>('titleInput');

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly isEditMode = computed(() => this.task() !== null);

  protected readonly priorities = TASK_PRIORITIES;
  protected readonly statuses = TASK_STATUSES;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.required],
    dueDate: this.fb.control<string | null>(null),
    priority: this.fb.nonNullable.control<(typeof TASK_PRIORITIES)[number]>('Medium'),
    categoryId: this.fb.control<string | null>(null),
    status: this.fb.nonNullable.control<(typeof TASK_STATUSES)[number]>('ToDo'),
  });

  private readonly formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });
  readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.submitting());

  constructor() {
    effect(() => {
      const current = this.task();
      if (current) {
        this.form.reset({
          title: current.title,
          description: current.description,
          dueDate: current.dueDate ? current.dueDate.substring(0, 10) : null,
          priority: current.priority,
          categoryId: current.categoryId,
          status: current.status,
        });
      } else {
        this.form.reset({
          title: '',
          description: '',
          dueDate: null,
          priority: 'Medium',
          categoryId: null,
          status: 'ToDo',
        });
      }
      this.submitError.set(null);
    });

    afterNextRender(() => {
      this.elementRef.nativeElement.focus();
      this.titleInput()?.nativeElement.focus();
    });

    effect(() => {
      if (this.addingCategory()) {
        queueMicrotask(() => this.newCategoryInput()?.nativeElement.focus());
      }
    });
  }

  startAddCategory(): void {
    this.categoryError.set(null);
    this.newCategoryName.set('');
    this.addingCategory.set(true);
  }

  cancelAddCategory(): void {
    this.addingCategory.set(false);
    this.categoryError.set(null);
  }

  submitNewCategory(): void {
    const name = this.newCategoryName().trim();
    if (!name) {
      this.categoryError.set('Category name is required.');
      return;
    }

    this.creatingCategory.set(true);
    this.categoryError.set(null);

    this.categoryService
      .create(name)
      .pipe(finalize(() => this.creatingCategory.set(false)))
      .subscribe({
        next: (category) => {
          this.categoryCreated.emit(category);
          this.form.controls.categoryId.setValue(category.id);
          this.addingCategory.set(false);
        },
        error: () => this.categoryError.set('Could not create category. Please try again.'),
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload: CreateTaskRequest = {
      title: raw.title.trim(),
      description: raw.description.trim(),
      dueDate: raw.dueDate ? new Date(raw.dueDate).toISOString() : null,
      priority: raw.priority,
      categoryId: raw.categoryId || null,
      status: raw.status,
    };

    this.submitting.set(true);
    this.submitError.set(null);

    const currentTask = this.task();
    const request$: Observable<unknown> = currentTask
      ? this.taskService.update(currentTask.id, payload)
      : this.taskService.create(payload);

    request$.pipe(finalize(() => this.submitting.set(false))).subscribe({
      next: () => this.saved.emit(),
      error: () => this.submitError.set('Could not save task. Please try again.'),
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
