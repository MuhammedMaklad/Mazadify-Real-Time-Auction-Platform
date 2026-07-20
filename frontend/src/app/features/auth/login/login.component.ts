import { Component, signal, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly error = signal<string | null>(null);
  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly year = new Date().getFullYear();

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/payment-methods']);
    }
  }


  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  clearError(): void {
    if (this.error()) this.error.set(null);
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/payment-methods']),
      error: (err: { error?: { error?: string } }) => {
        this.error.set(err?.error?.error ?? 'Login failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
