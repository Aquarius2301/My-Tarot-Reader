/** Query parameters for list endpoints that are paginated on the backend. */
export interface PageParams {
  page: number;
  pageSize: number;
}

/** Common envelope returned by paginated list endpoints. */
export interface PaginatedResult<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}