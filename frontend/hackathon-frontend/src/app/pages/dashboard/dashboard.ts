import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
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

  constructor(
    public auth: AuthService,
    private router: Router,
    private medicineService: MedicineService,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    if (this.auth.isAdmin()) {
      this.loadAdminStats();
    } else {
      this.loading = false;
    }
  }

  private loadAdminStats(): void {
    forkJoin({
      medicines: this.medicineService
        .getAll()
        .pipe(catchError(() => of<Medicine[]>([]))),
      orders: this.orderService
        .allOrders()
        .pipe(catchError(() => of<Order[]>([])))
    }).subscribe(
      ({ medicines, orders }: { medicines: Medicine[]; orders: Order[] }) => {
        this.medicines = medicines ?? [];
        this.orders = orders ?? [];
        this.loading = false;
      }
    );
  }

  totalMedicines(): number { return this.medicines.length; }
  totalOrders(): number { return this.orders.length; }

  pendingValidations(): number {
    return this.orders.filter((o) =>
      (o.status || '').toLowerCase().includes('pending')
    ).length;
  }

  lowStockCount(threshold = 50): number {
    return this.medicines.filter((m) => m.stockQuantity <= threshold).length;
  }

  totalRevenue(): number {
    return this.orders
      .filter((o) => (o.status || '').toLowerCase() !== 'rejected')
      .reduce((sum, o) => sum + (o.finalAmount || 0), 0);
  }
}
