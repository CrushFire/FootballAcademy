export interface ApiResponse<T> {
  success: boolean
  data: T
  error?: string
}

export interface Filter {
  filters?: Record<string, unknown>
  sort?: { fields: SortField[] }
  pagination?: Pagination
}

export interface SortField {
  field: string
  direction: 'asc' | 'desc'
}

export interface Pagination {
  page: number
  pageSize: number
}
