import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LoyaltyService } from '../../services/loyalty.service';
import { LoyaltyTransaction } from '../../models/loyalty.model';
import { Navbar } from '../../components/navbar/navbar';

@Component({
  selector: 'app-loyalty',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './loyalty.html'
})
export class Loyalty implements OnInit {
  points = 0;
  transactions: LoyaltyTransaction[] = [];
  loading = true;

  constructor(private loyaltyService: LoyaltyService) {}

  ngOnInit(): void {
    this.loyaltyService.getPoints().subscribe({
      next: (r: { points: number }) => (this.points = r?.points ?? 0),
      error: () => (this.points = 0)
    });

    this.loyaltyService.getTransactions().subscribe({
      next: (t: LoyaltyTransaction[]) => {
        this.transactions = t ?? [];
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }
}
