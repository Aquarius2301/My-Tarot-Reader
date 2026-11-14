import { useGetCurrentUser } from "@/hooks/api";
import { AppLoading } from "@/components";
import { Navigate } from "react-router-dom";
import { registerRoutes, WEB_URL } from "./url.routes";
import { RegisterLayout } from "@/layouts";
import { useRouteTitle } from "@/hooks/custom/useRouteTitle";

export default function RegisterRoute() {
  const { data, isLoading, isError } = useGetCurrentUser();
  const title = useRouteTitle(registerRoutes);

  if (isLoading) {
    return <AppLoading />;
  }

  if (isError || !data) {
    return <Navigate to={WEB_URL.guestHome} replace />;
  }

  return (
    <RegisterLayout
      title={title}
      displayName={data.fullName}
      picture={data.picture}
      redCoins={data.redCoin}
      whiteCoins={data.whiteCoin}
    />
  );
}
