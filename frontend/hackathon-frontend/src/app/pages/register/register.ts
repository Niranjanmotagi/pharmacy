import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

const USERNAME = /^[A-Za-z]{4,20}$/;
const NAME = /^[A-Za-z]{1,30}$/;
const PHONE = /^\d{10}$/;
const STRONG_PASSWORD =
  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,64}$/;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, Navbar],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = false;
  serverError = '';

  showPassword = false;
  showConfirm = false;

  form: FormGroup = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.pattern(NAME)]],
      lastName: ['', [Validators.required, Validators.pattern(NAME)]],
      username: ['', [Validators.required, Validators.pattern(USERNAME)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(PHONE)]],
      password: ['', [Validators.required, Validators.pattern(STRONG_PASSWORD)]],
      confirmPassword: ['', [Validators.required]]
    },
    { validators: [this.passwordsMatchValidator] }
  );

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const p = group.get('password')?.value;
    const c = group.get('confirmPassword')?.value;
    if (!p || !c) return null;
    return p === c ? null : { passwordMismatch: true };
  }

  // For template
  hasError(name: string, code: string): boolean {
    const c = this.form.get(name);
    return !!c && c.touched && c.hasError(code);
  }

  invalid(name: string): boolean {
    const c = this.form.get(name);
    return !!c && c.touched && c.invalid;
  }

  showConfirmMismatch(): boolean {
    const confirm = this.form.get('confirmPassword');
    return (
      !!confirm &&
      confirm.touched &&
      !confirm.hasError('required') &&
      this.form.hasError('passwordMismatch')
    );
  }

  // Strength meter 0..5
  strengthScore(): number {
    const pwd: string = this.form.get('password')?.value || '';
    let score = 0;
    if (pwd.length >= 8) score++;
    if (/[a-z]/.test(pwd)) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/\d/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  strengthLabel(): string {
    const s = this.strengthScore();
    return ['Empty', 'Very weak', 'Weak', 'Fair', 'Strong', 'Excellent'][s];
  }

  togglePassword() { this.showPassword = !this.showPassword; }
  toggleConfirm() { this.showConfirm = !this.showConfirm; }

  submit() {
    this.serverError = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.error('Please fix the highlighted fields.');
      return;
    }

    this.loading = true;
    const v = this.form.value as {
      firstName: string;
      lastName: string;
      username: string;
      email: string;
      phoneNumber: string;
      password: string;
    };

    this.auth
      .register({
        username: v.username,
        password: v.password,
        role: 'Customer',
        email: v.email,
        firstName: v.firstName,
        lastName: v.lastName,
        phoneNumber: v.phoneNumber
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.toast.success('Account created!', 'Please log in to continue.');
          setTimeout(() => this.router.navigate(['/login']), 900);
        },
        error: (err: unknown) => {
          this.loading = false;
          const msg = readErrorMessage(
            err,
            'Registration failed. Please try again.'
          );
          this.serverError = msg;
          this.toast.error('Registration failed', msg);
        }
      });
  }
}
