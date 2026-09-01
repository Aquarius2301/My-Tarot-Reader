import { WEB_URL } from "@/constants";
import { useGetMe } from "@/hooks/api";
import { MainLayout } from "@/layouts";
import { Spin } from "antd";
import { Navigate, Outlet } from "react-router-dom";

export default function ProtectedRoute() {
  const { data, isLoading, isError } = useGetMe();

  if (isLoading) {
    return (
      <MainLayout>
        <Spin fullscreen />
      </MainLayout>
    );
  }

  if (isError || !data) {
    return <Navigate to={WEB_URL.HOME} replace />;
  }

  return (
    <MainLayout role={data.role} user={data}>
      <Outlet />
    </MainLayout>
  );
}
