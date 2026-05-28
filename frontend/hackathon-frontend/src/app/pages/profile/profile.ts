import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { AuthService } from '../../services/auth.service';
import {
  UserProfile,
  ChangePasswordPayload,
  UpdateProfilePayload
} from '../../models/user-profile.model';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

type ProfileTab = 'overview' | 'edit' | 'password';

const NAME = /^[A-Za-z]{1,30}$/;
const PHONE = /^\d{10}$/;
const STRONG_PASSWORD =
  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,64}$/;

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, Navbar],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private toast = inject(ToastService);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = true;
  saving = false;
  changing = false;
  loadError = '';

  profile: UserProfile | null = null;
  activeTab: ProfileTab = 'overview';

  showCurrent = false;
  showNew = false;

  editForm: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.pattern(NAME)]],
    lastName: ['', [Validators.required, Validators.pattern(NAME)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.pattern(PHONE)]]
  });

  passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.pattern(STRONG_PASSWORD)]],
    confirmPassword: ['', Validators.required]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.loadError = '';

    this.userService.me().subscribe({
      next: (data: UserProfile) => {
        this.profile = data;
        this.editForm.patchValue({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phoneNumber: data.phoneNumber
        });
        this.loading = false;
      },
      error: (err: unknown) => {
        this.loading = false;
        this.loadError = readErrorMessage(
          err,
          'Could not load your profile. Please try again.'
        );
      }
    });
  }

  setTab(tab: ProfileTab): void {
    this.activeTab = tab;
  }

  initials(): string {
    if (!this.profile) return '?';
    const first = (this.profile.firstName || this.profile.username || '?')
      .charAt(0)
      .toUpperCase();
    const last = (this.profile.lastName || '').charAt(0).toUpperCase();
    return `${first}${last}`;
  }

  fullName(): string {
    if (!this.profile) return '';
    const fn = (this.profile.firstName || '').trim();
    const ln = (this.profile.lastName || '').trim();
    const joined = `${fn} ${ln}`.trim();
    return joined || this.profile.username;
  }

  // ===== Edit profile =====
  hasError(form: FormGroup, name: string, code: string): boolean {
    const c = form.get(name);
    return !!c && c.touched && c.hasError(code);
  }

  invalid(form: FormGroup, name: string): boolean {
    const c = form.get(name);
    return !!c && c.touched && c.invalid;
  }

  saveProfile(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.toast.error('Please fix the highlighted fields.');
      return;
    }

    const payload = this.editForm.value as UpdateProfilePayload;
    this.saving = true;

    this.userService.updateProfile(payload).subscribe({
      next: () => {
        this.saving = false;
        this.toast.success('Profile updated.');
        this.load(); // refresh stats
        this.setTab('overview');
      },
      error: (err: unknown) => {
        this.saving = false;
        this.toast.error(
          'Update failed',
          readErrorMessage(err, 'Server error.')
        );
      }
    });
  }

  // ===== Change password =====
  toggleCurrent(): void { this.showCurrent = !this.showCurrent; }
  toggleNew(): void { this.showNew = !this.showNew; }

  passwordsMatch(): boolean {
    const n = this.passwordForm.get('newPassword')?.value;
    const c = this.passwordForm.get('confirmPassword')?.value;
    return !!n && n === c;
  }

  strengthScore(): number {
    const pwd: string = this.passwordForm.get('newPassword')?.value || '';
    let score = 0;
    if (pwd.length >= 8) score++;
    if (/[a-z]/.test(pwd)) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/\d/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  strengthLabel(): string {
    return ['Empty', 'Very weak', 'Weak', 'Fair', 'Strong', 'Excellent'][
      this.strengthScore()
    ];
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      this.toast.error('Please fix the highlighted fields.');
      return;
    }
    if (!this.passwordsMatch()) {
      this.toast.error('Passwords do not match.');
      return;
    }

    const payload: ChangePasswordPayload = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.changing = true;

    this.userService.changePassword(payload).subscribe({
      next: () => {
        this.changing = false;
        this.toast.success('Password changed. Please log in again.');
        this.passwordForm.reset();
        // Best practice: force a fresh login after password change.
        setTimeout(() => {
          this.auth.logout();
          this.router.navigate(['/login']);
        }, 1200);
      },
      error: (err: unknown) => {
        this.changing = false;
        this.toast.error(
          'Password change failed',
          readErrorMessage(err, 'Server error.')
        );
      }
    });
  }
}
