import { MainLayout } from "@/layouts";
import { Outlet } from "react-router-dom";

export default function PublicLayout() {
  return (
    <MainLayout>
      <Outlet />
    </MainLayout>
  );
}
