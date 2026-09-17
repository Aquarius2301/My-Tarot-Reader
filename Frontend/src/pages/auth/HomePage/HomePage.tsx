import { HeroSection } from "@/pages/shared/home";
import { useGetCurrentUser } from "@/hooks/api";
import { Spin } from "antd";

export default function AuthHomePage() {
  const { data: user, isLoading } = useGetCurrentUser();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  return <HeroSection role={user?.role} />;
}
