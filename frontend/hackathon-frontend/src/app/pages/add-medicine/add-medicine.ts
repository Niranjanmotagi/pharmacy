import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MedicineService } from '../../services/medicine.service';
import { ToastService } from '../../services/toast.service';
import { Medicine } from '../../models/medicine.model';
import { Navbar } from '../../components/navbar/navbar';
import { readErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-add-medicine',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Navbar],
  templateUrl: './add-medicine.html'
})
export class AddMedicine implements OnInit {
  med: Partial<Medicine> = this.emptyMedicine();

  medicines: Medicine[] = [];
  filteredMedicines: Medicine[] = [];
  loading = true;
  saving = false;

  // Inventory list controls
  search = '';

  constructor(
    private medicineService: MedicineService,
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
        this.applyFilter();
        this.loading = false;
      },
      error: (err: unknown) => {
        this.loading = false;
        this.toast.error(
          'Failed to load medicines.',
          readErrorMessage(err, 'Server error.')
        );
      }
    });
  }

  applyFilter(): void {
    const q = this.search.toLowerCase().trim();
    this.filteredMedicines = !q
      ? this.medicines
      : this.medicines.filter(
          (m) =>
            m.name.toLowerCase().includes(q) ||
            (m.category || '').toLowerCase().includes(q) ||
            (m.manufacturer || '').toLowerCase().includes(q)
        );
  }

  emptyMedicine(): Partial<Medicine> {
    return {
      name: '',
      category: '',
      manufacturer: '',
      composition: '',
      dosageForm: '',
      strength: '',
      packagingType: '',
      price: 0,
      stockQuantity: 0,
      description: '',
      requiresPrescription: false,
      imageUrl: ''
    };
  }

  save() {
    if (!this.med.name?.trim() || !this.med.category?.trim()) {
      this.toast.error('Name and category are required.');
      return;
    }
    if ((this.med.price ?? 0) <= 0) {
      this.toast.error('Price must be greater than 0.');
      return;
    }
    if ((this.med.stockQuantity ?? -1) < 0) {
      this.toast.error('Stock must be 0 or higher.');
      return;
    }

    this.saving = true;
    this.medicineService.add(this.med).subscribe({
      next: () => {
        this.saving = false;
        this.toast.success(`${this.med.name} added.`);
        this.med = this.emptyMedicine();
        this.load();
      },
      error: (err: unknown) => {
        this.saving = false;
        this.toast.error('Add failed', readErrorMessage(err, 'Server error.'));
      }
    });
  }

  updateStock(m: Medicine, newQty: number) {
    if (Number.isNaN(Number(newQty)) || Number(newQty) < 0) {
      this.toast.error('Stock must be 0 or higher.');
      return;
    }
    const updated: Medicine = { ...m, stockQuantity: Number(newQty) };
    this.medicineService.update(m.id, updated).subscribe({
      next: () => {
        this.toast.success(`${m.name} stock updated.`);
        this.load();
      },
      error: (err: unknown) =>
        this.toast.error(
          'Stock update failed.',
          readErrorMessage(err, 'Server error.')
        )
    });
  }

  delete(m: Medicine): void {
    if (!confirm(`Delete ${m.name}?`)) return;
    this.medicineService.delete(m.id).subscribe({
      next: () => {
        this.toast.success(`${m.name} deleted.`);
        this.load();
      },
      error: (err: unknown) =>
        this.toast.error('Delete failed.', readErrorMessage(err, 'Server error.'))
    });
  }
}
