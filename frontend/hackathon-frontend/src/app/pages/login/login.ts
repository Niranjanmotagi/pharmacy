import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService, LoginResponse } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Navbar],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  username = '';
  password = '';
  error = '';
  loading = false;
  showPassword = false;

  togglePassword(): void { this.showPassword = !this.showPassword; }

  login(form: NgForm): void {
    if (form.invalid) {
      form.control.markAllAsTouched();
      this.toast.error('Please enter your username and password.');
      return;
    }

    this.error = '';
    this.loading = true;
    this.authService.login(this.username.trim(), this.password).subscribe({
      next: (res: LoginResponse) => {
        this.loading = false;
        this.toast.success(`Welcome back, ${res.username}!`);
        this.router.navigate([
          res.role === 'Admin' ? '/dashboard' : '/medicines'
        ]);
      },
      error: (err: unknown) => {
        this.loading = false;
        const msg = readErrorMessage(
          err,
          'Login failed. Please check your credentials.'
        );
        this.error = msg;
        this.toast.error('Login failed', msg);
      }
    });
  }
}
