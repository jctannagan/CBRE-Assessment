import { AfterViewInit, ChangeDetectionStrategy, Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { GoogleIdentityService } from '../../../../core/auth/google-identity';
import { AuthService } from '../../service/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly googleIdentity = inject(GoogleIdentityService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly googleButton = viewChild.required<ElementRef<HTMLElement>>('googleButton');

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });
  private readonly formStatus = toSignal(this.form.statusChanges, {
    initialValue: this.form.status
  });
  readonly canSubmit = computed(() => this.formStatus() === 'VALID' && !this.loading());

  signIn(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService
      .login(this.form.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.navigateAfterAuth(),
        error: () => this.errorMessage.set('Invalid email or password.'),
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
            next: () => this.navigateAfterAuth(),
            error: () => this.errorMessage.set('Google sign-in failed.'),
          });
      },
      error: () => this.errorMessage.set('Google sign-in failed.'),
    });
  }

  private navigateAfterAuth(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/tasks';
    this.router.navigateByUrl(returnUrl);
  }
}
