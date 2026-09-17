import type { UserRole } from "../enums.type";

export interface GoogleLoginRequest {
  credential: string;
  locale: string | null;
}

export interface GetCurrentUserResult {
  id: string;
  fullName: string;
  email: string;
  picture: string;
  whiteCoin: number;
  redCoin: number;
  role: UserRole;
}
