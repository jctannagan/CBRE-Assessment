import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { GoogleIdentityService } from '../../../../core/auth/google-identity';
import { AuthService } from '../../service/auth.service';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent implements AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly googleIdentity = inject(GoogleIdentityService);
  private readonly router = inject(Router);

  private readonly googleButton = viewChild.required<ElementRef<HTMLElement>>('googleButton');

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator },
  );

  private readonly formStatus = toSignal(this.form.statusChanges, {
    initialValue: this.form.status,
  });
  readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.loading());

  signUp(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const { confirmPassword, ...payload } = this.form.getRawValue();

    this.authService
      .register(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.router.navigateByUrl('/tasks'),
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.extractErrorMessage(error)),
      });
  }

  ngAfterViewInit(): void {
    this.googleIdentity.renderButton(this.googleButton().nativeElement).subscribe({
      next: (idToken) => {
        this.errorMessage.set(null);
        this.loading.set(true);
        this.authService
          .loginWithGoogle(idToken)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => this.router.navigateByUrl('/tasks'),
            error: () => this.errorMessage.set('Google sign-up failed.'),
          });
      },
      error: () => this.errorMessage.set('Google sign-up failed.'),
    });
  }

  private extractErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return error.error?.message ?? 'An account with this email already exists.';
    }
    if (error.status === 400 && Array.isArray(error.error?.errors)) {
      return error.error.errors.join(' ');
    }
    return 'Something went wrong. Please try again.';
  }
}
