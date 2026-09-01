import { Spin } from "antd";
import { HeroSection } from "@/pages/common/hero";
import { useGetMe } from "@/hooks/api";

export default function AuthHomePage() {
  const { data: user, isLoading } = useGetMe();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  return <HeroSection role={user?.role} />;
}
