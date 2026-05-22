export interface UserCreateRequest {
  role: string
  login: string
  email: string
  password: string
}

export interface UserUpdateRequest {
  login?: string
  email?: string
  password?: string
}

export interface UserResponse {
  id: number
  login: string
  email: string
  role: string
  createdAt: string
}
