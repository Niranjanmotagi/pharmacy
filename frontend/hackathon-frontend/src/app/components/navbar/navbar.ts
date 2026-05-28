import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs/operators';
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
  /** Mobile menu open/close state — no Bootstrap JS needed. */
  open = signal(false);

  constructor(
    public auth: AuthService,
    private router: Router,
    private toast: ToastService
  ) {
    // Auto-close the mobile menu on every navigation so the next page is
    // immediately visible.
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => this.open.set(false));
  }

  toggle(): void {
    this.open.update((v) => !v);
  }

  close(): void {
    this.open.set(false);
  }

  /** Close the mobile menu when the user clicks outside of it. */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.open()) return;
    const target = event.target as HTMLElement | null;
    if (!target) return;
    if (target.closest('.bb-navbar')) return; // click inside the navbar
    this.close();
  }

  logout(): void {
    const name = this.auth.getUsername();
    this.auth.logout();
    this.toast.info('Signed out', name ? `See you soon, ${name}.` : undefined);
    this.close();
    this.router.navigate(['/login']);
  }
}
