import { Pagination, theme } from "antd";
import type { PaginationBarProps } from "./types";

export function PaginationBar({
  total,
  page,
  pageSize,
  onChange,
  disabled = false,
}: PaginationBarProps) {
  const { token } = theme.useToken();

  if (total <= pageSize) {
    return null;
  }

  return (
    <Pagination
      current={page}
      pageSize={pageSize}
      total={total}
      onChange={onChange}
      showSizeChanger={false}
      hideOnSinglePage
      align="end"
      disabled={disabled}
      style={{ marginTop: token.marginLG }}
    />
  );
}
