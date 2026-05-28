export interface UserProfile {
  id: number;
  username: string;
  role: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  loyaltyPoints: number;
  totalOrders: number;
  pendingOrders: number;
  totalSpent: number;
  activeSince: string | null;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}
