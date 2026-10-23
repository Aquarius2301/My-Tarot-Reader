import { Spin } from "antd";
import { Navigate, Outlet } from "react-router-dom";
import { WEB_URL } from "../url.routes";
import { useGetCurrentUser } from "@/hooks/api";

export default function UserRoute() {
  const { data, isLoading, isError } = useGetCurrentUser();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (isError || !data) {
    return <Navigate to={WEB_URL.guestHome} replace />;
  }

  if (data.role === "admin") {
    return <Navigate to={WEB_URL.adminStatistics} replace />;
  }

  return <Outlet />;
}