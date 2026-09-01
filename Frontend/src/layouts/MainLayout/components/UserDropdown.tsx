import { Avatar, Dropdown, type MenuProps } from "antd";
import { UserOutlined } from "@ant-design/icons";
import type { UserResponse } from "@/types";
import type { Palette } from "./LayoutFooter";

export interface UserDropdownProps {
  user?: UserResponse;
  palette: Palette;
  items: MenuProps["items"];
}

export default function UserDropdown({
  user,
  palette,
  items,
}: UserDropdownProps) {
  return (
    <Dropdown menu={{ items }} placement="bottomRight" arrow>
      <Avatar
        src={user?.picture}
        icon={<UserOutlined />}
        style={{
          backgroundColor: palette.primary,
          cursor: "pointer",
        }}
      />
    </Dropdown>
  );
}
