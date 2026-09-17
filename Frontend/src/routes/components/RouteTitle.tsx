import { useDocumentTitle } from "@/hooks/custom";

interface RouteTitleProps {
  titleKey?: string;
  children: React.ReactNode;
}

export default function RouteTitle({ titleKey, children }: RouteTitleProps) {
  useDocumentTitle(titleKey);

  return children;
}
