import { AdminLayout, GuestLayout, RegisterLayout } from "@/layouts";
import type { GetCurrentUserResult } from "@/types";

export default function NotFoundLayoutRoute({
  user,
}: {
  user?: GetCurrentUserResult;
}) {
  if (!user) {
    return <GuestLayout title="page.notFound.title" />;
  }

  if (user.role === "admin") {
    return <AdminLayout title="page.notFound.title" />;
  }

  return (
    <RegisterLayout
      title="page.notFound.title"
      displayName={user.fullName}
      picture={user.picture}
      redCoins={user.redCoin}
      whiteCoins={user.whiteCoin}
    />
  );
}
