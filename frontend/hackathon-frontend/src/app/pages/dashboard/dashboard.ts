import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { MedicineService } from '../../services/medicine.service';
import { OrderService } from '../../services/order.service';
import { Navbar } from '../../components/navbar/navbar';
import { Medicine } from '../../models/medicine.model';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  loading = true;
  medicines: Medicine[] = [];
  orders: Order[] = [];

  /** Threshold below which a medicine is flagged "low stock". */
  readonly LOW_STOCK_THRESHOLD = 50;

  constructor(
    public auth: AuthService,
    private medicineService: MedicineService,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    if (this.auth.isAdmin()) {
      this.loadAdmin();
    } else {
      this.loading = false;
    }
  }

  private loadAdmin(): void {
    forkJoin({
      medicines: this.medicineService.getAll().pipe(
        catchError(() => of<Medicine[]>([]))
      ),
      orders: this.orderService.allOrders().pipe(
        catchError(() => of<Order[]>([]))
      )
    }).subscribe(({ medicines, orders }) => {
      this.medicines = medicines ?? [];
      this.orders = orders ?? [];
      this.loading = false;
    });
  }

  // ===== Stats =====
  totalMedicines(): number {
    return this.medicines.length;
  }

  totalOrders(): number {
    return this.orders.length;
  }

  pendingValidations(): number {
    return this.orders.filter((o) =>
      (o.status || '').toLowerCase().includes('pending')
    ).length;
  }

  /** Pending orders that also have a prescription file. */
  pendingPrescriptions(): number {
    return this.orders.filter(
      (o) =>
        !!o.prescriptionFile &&
        (o.status || '').toLowerCase().includes('pending')
    ).length;
  }

  lowStock(): Medicine[] {
    return [...this.medicines]
      .filter((m) => m.stockQuantity <= this.LOW_STOCK_THRESHOLD)
      .sort((a, b) => a.stockQuantity - b.stockQuantity);
  }

  lowStockCount(): number {
    return this.lowStock().length;
  }

  outOfStockCount(): number {
    return this.medicines.filter((m) => m.stockQuantity <= 0).length;
  }

  totalRevenue(): number {
    return this.orders
      .filter((o) => (o.status || '').toLowerCase() !== 'rejected')
      .reduce((sum, o) => sum + (o.finalAmount || 0), 0);
  }

  /** Last 6 orders, newest first. */
  recentOrders(): Order[] {
    return [...this.orders]
      .sort((a, b) =>
        new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
      )
      .slice(0, 6);
  }

  // ===== UI helpers =====
  statusChipClass(o: Order): string {
    const s = (o.status || '').toLowerCase();
    if (s.includes('reject'))  return 'bb-status bb-status-rejected';
    if (s.includes('deliver')) return 'bb-status bb-status-delivered';
    if (s.includes('out'))     return 'bb-status bb-status-shipped';
    if (s.includes('pack'))    return 'bb-status bb-status-packed';
    if (s.includes('approve')) return 'bb-status bb-status-approved';
    return 'bb-status bb-status-pending';
  }

  stockBadgeClass(m: Medicine): string {
    if (m.stockQuantity <= 0)  return 'bb-status bb-status-rejected';
    if (m.stockQuantity <= 10) return 'bb-status bb-status-pending';
    return 'bb-status bb-status-approved';
  }
}
