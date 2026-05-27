import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MedicineService } from '../../services/medicine.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { Medicine } from '../../models/medicine.model';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

type SortKey = 'name' | 'priceAsc' | 'priceDesc' | 'stock';

@Component({
  selector: 'app-medicine-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Navbar],
  templateUrl: './medicine-list.html',
  styleUrl: './medicine-list.css'
})
export class MedicineList implements OnInit {
  medicines: Medicine[] = [];

  loading = true;
  message = '';
  showDetailsId: number | null = null;

  // Filters
  search = '';
  selectedCategory = '';
  selectedDosage = '';
  selectedPackaging = '';
  rxOnly = false;
  minPrice = 0;
  maxPrice = 1000;
  sortBy: SortKey = 'name';

  // Pagination
  pageSize = 12;
  page = 1;

  constructor(
    private medicineService: MedicineService,
    private cartService: CartService,
    public auth: AuthService,
    private toast: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.medicineService.getAll().subscribe({
      next: (data: Medicine[]) => {
        this.medicines = data ?? [];

        // Initialise the slider range to the actual data range
        if (this.medicines.length) {
          const prices = this.medicines.map((m) => m.price);
          const min = Math.floor(Math.min(...prices));
          const max = Math.ceil(Math.max(...prices));
          this.minPrice = min;
          this.maxPrice = max;
        }
        this.loading = false;
      },
      error: (err: unknown) => {
        this.loading = false;
        this.toast.error(
          'Could not load medicines.',
          readErrorMessage(err, 'Server error.')
        );
      }
    });
  }

  categories(): string[] {
    return Array.from(new Set(this.medicines.map((m) => m.category))).sort();
  }
  dosageForms(): string[] {
    return Array.from(new Set(this.medicines.map((m) => m.dosageForm))).sort();
  }
  packagingTypes(): string[] {
    return Array.from(
      new Set(this.medicines.map((m) => m.packagingType))
    ).sort();
  }

  priceRange(): { min: number; max: number } {
    if (!this.medicines.length) return { min: 0, max: 1000 };
    const prices = this.medicines.map((m) => m.price);
    return {
      min: Math.floor(Math.min(...prices)),
      max: Math.ceil(Math.max(...prices))
    };
  }

  filtered(): Medicine[] {
    const q = this.search.toLowerCase().trim();

    let list = this.medicines.filter((m) => {
      const matchesSearch =
        !q ||
        m.name.toLowerCase().includes(q) ||
        m.category.toLowerCase().includes(q) ||
        (m.composition || '').toLowerCase().includes(q) ||
        (m.manufacturer || '').toLowerCase().includes(q);

      const matchesCategory =
        !this.selectedCategory || m.category === this.selectedCategory;
      const matchesDosage =
        !this.selectedDosage || m.dosageForm === this.selectedDosage;
      const matchesPackaging =
        !this.selectedPackaging || m.packagingType === this.selectedPackaging;
      const matchesRx = !this.rxOnly || m.requiresPrescription;
      const matchesPrice =
        m.price >= this.minPrice && m.price <= this.maxPrice;

      return (
        matchesSearch &&
        matchesCategory &&
        matchesDosage &&
        matchesPackaging &&
        matchesRx &&
        matchesPrice
      );
    });

    switch (this.sortBy) {
      case 'priceAsc':
        list = [...list].sort((a, b) => a.price - b.price);
        break;
      case 'priceDesc':
        list = [...list].sort((a, b) => b.price - a.price);
        break;
      case 'stock':
        list = [...list].sort((a, b) => b.stockQuantity - a.stockQuantity);
        break;
      default:
        list = [...list].sort((a, b) => a.name.localeCompare(b.name));
    }

    return list;
  }

  // Pagination helpers
  totalPages(): number {
    return Math.max(1, Math.ceil(this.filtered().length / this.pageSize));
  }
  paged(): Medicine[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  }
  pages(): number[] {
    const total = this.totalPages();
    const out: number[] = [];
    for (let i = 1; i <= total; i++) out.push(i);
    return out;
  }
  setPage(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    this.page = p;
  }
  // Snap back to page 1 whenever filters change
  onFilterChange() {
    this.page = 1;
  }

  resetFilters() {
    this.search = '';
    this.selectedCategory = '';
    this.selectedDosage = '';
    this.selectedPackaging = '';
    this.rxOnly = false;
    this.sortBy = 'name';
    const r = this.priceRange();
    this.minPrice = r.min;
    this.maxPrice = r.max;
    this.page = 1;
  }

  toggleDetails(id: number) {
    this.showDetailsId = this.showDetailsId === id ? null : id;
  }

  addToCart(med: Medicine) {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.cartService.addToCart(med.id, 1).subscribe({
      next: () => this.toast.success(`${med.name} added to cart`),
      error: (err: unknown) =>
        this.toast.error(
          'Could not add to cart',
          readErrorMessage(err, 'Server error.')
        )
    });
  }

  delete(med: Medicine): void {
    if (!confirm(`Delete ${med.name}?`)) return;
    this.medicineService.delete(med.id).subscribe({
      next: () => {
        this.toast.success(`${med.name} deleted.`);
        this.load();
      },
      error: (err: unknown) =>
        this.toast.error('Delete failed.', readErrorMessage(err, 'Server error.'))
    });
  }
}
