import type { LayoutVariant } from "@/constants";

export function getVariantForPath(pathname: string): LayoutVariant {
  if (pathname.startsWith("/admin")) return "admin";
  if (
    pathname === "/guest" ||
    pathname.startsWith("/guest/") ||
    pathname === "/login" ||
    pathname.startsWith("/login/") ||
    pathname === "/draw/guest"
  ) {
    return "guest";
  }
  return "register";
}
