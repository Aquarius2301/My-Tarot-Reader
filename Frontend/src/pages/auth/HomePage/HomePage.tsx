import { HeroSection } from "@/pages/shared/home";
import { useGetCurrentUser } from "@/hooks/api";
import { Spin } from "antd";
import StreakCard from "./components/StreakCard";

export default function AuthHomePage() {
  const { data: user, isLoading } = useGetCurrentUser();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto" }}>
      <HeroSection role={user?.role} />
      <StreakCard />
    </div>
  );
}
