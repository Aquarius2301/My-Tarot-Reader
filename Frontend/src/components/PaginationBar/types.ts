export interface PaginationBarProps {
  total: number;
  page: number;
  pageSize: number;
  onChange: (page: number, pageSize: number) => void;
  disabled?: boolean;
}
