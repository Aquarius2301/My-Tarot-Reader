import type { UserRole } from "@/constants";

export interface LoginRequest {
  credential: string;
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
