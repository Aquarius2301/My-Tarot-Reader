import type { UserRole } from "@/constants";

export interface LoginRequest {
  credential: string;
  /** The user's UI locale ("vi" or "en"); used to localize the welcome email. */
  locale?: string;
}

export interface UserResponse {
  id: string;
  fullName: string;
  email: string;
  picture: string;
  whiteCoin: number;
  redCoin: number;
  role: UserRole;
}
