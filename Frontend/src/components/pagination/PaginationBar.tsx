import { Pagination, theme } from "antd";

export interface PaginationBarProps {
  total: number;
  page: number;
  pageSize: number;
  onChange: (page: number, pageSize: number) => void;
  disabled?: boolean;
}

export default function PaginationBar({
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