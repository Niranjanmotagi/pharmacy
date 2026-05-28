import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';

import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Dashboard } from './pages/dashboard/dashboard';
import { MedicineList } from './pages/medicine-list/medicine-list';
import { AddMedicine } from './pages/add-medicine/add-medicine';
import { Cart } from './pages/cart/cart';
import { Checkout } from './pages/checkout/checkout';
import { Orders } from './pages/orders/orders';
import { Profile } from './pages/profile/profile';

export const routes: Routes = [
  { path: '', redirectTo: 'medicines', pathMatch: 'full' },

  { path: 'login', component: Login },
  { path: 'register', component: Register },

  // Public: anyone can browse medicines
  { path: 'medicines', component: MedicineList },

  // Authenticated routes
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'cart', component: Cart, canActivate: [authGuard] },
  { path: 'checkout', component: Checkout, canActivate: [authGuard] },
  { path: 'orders', component: Orders, canActivate: [authGuard] },
  { path: 'profile', component: Profile, canActivate: [authGuard] },

  // Back-compat: old /loyalty links redirect to /profile
  { path: 'loyalty', redirectTo: 'profile', pathMatch: 'full' },

  // Admin only
  { path: 'add-medicine', component: AddMedicine, canActivate: [adminGuard] },
  { path: 'admin-orders', component: Orders, canActivate: [adminGuard] },

  { path: '**', redirectTo: 'medicines' }
];
