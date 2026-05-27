import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  constructor(
    public auth: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  logout(): void {
    const name = this.auth.getUsername();
    this.auth.logout();
    this.toast.info('Signed out', name ? `See you soon, ${name}.` : undefined);
    this.router.navigate(['/login']);
  }
}
