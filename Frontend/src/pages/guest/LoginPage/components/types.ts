import type { ReactNode } from "react";

// Interface to avoid the 'readonly' issue with Antd List
export interface BenefitItem {
  key: string;
  icon: ReactNode;
  title: string;
  description: string;
}