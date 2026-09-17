import { Spin } from "antd";
import { Navigate, Outlet } from "react-router-dom";
import { WEB_URL } from "../url.routes";
import { MainLayout } from "@/components";
import { useGetCurrentUser } from "@/hooks/api";

export default function ProtectedRoute() {
  const { data, isLoading, isError } = useGetCurrentUser();

  if (isLoading) {
    return (
      <MainLayout>
        <Spin fullscreen />
      </MainLayout>
    );
  }

  if (isError || !data) {
    return <Navigate to={WEB_URL.guestHome} replace />;
  }

  return (
    <MainLayout role={data.role} user={data}>
      <Outlet />
    </MainLayout>
  );
}
