import {
  DisconnectOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import type { ErrorType } from "./types";

export const ERROR_CONFIG: Record<
  ErrorType,
  {
    titleKey: string;
    descKey: string;
    Icon: typeof DisconnectOutlined;
  }
> = {
  offline: {
    titleKey: "component.error.offlineTitle",
    descKey: "component.error.offlineDesc",
    Icon: DisconnectOutlined,
  },
  server: {
    titleKey: "component.error.serverTitle",
    descKey: "component.error.serverDesc",
    Icon: ExclamationCircleOutlined,
  },
  timeout: {
    titleKey: "component.error.timeoutTitle",
    descKey: "component.error.timeoutDesc",
    Icon: ClockCircleOutlined,
  },
  generic: {
    titleKey: "component.error.genericTitle",
    descKey: "component.error.genericDesc",
    Icon: ReloadOutlined,
  },
};
