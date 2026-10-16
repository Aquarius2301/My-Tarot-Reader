import { Spin } from "antd";
import { Navigate, Outlet } from "react-router-dom";
import { WEB_URL } from "../url.routes";
import { useGetCurrentUser } from "@/hooks/api";

export default function AdminRoute() {
  const { data, isLoading, isError } = useGetCurrentUser();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  // Not signed in at all -> send to the guest home.
  if (isError || !data) {
    return <Navigate to={WEB_URL.guestHome} replace />;
  }

  // Signed in but not an admin -> send to the main site home.
  if (data.role !== "admin") {
    return <Navigate to={WEB_URL.home} replace />;
  }

  return <Outlet />;
}
