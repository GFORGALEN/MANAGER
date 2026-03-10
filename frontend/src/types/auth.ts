export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  name: string
  email: string
  phoneNumber?: string | null
  password: string
  confirmPassword: string
}

export interface AuthResponse {
  token: string
  username: string
  role: string
  expiresAtUtc: string
}

export interface CurrentUser {
  id: string
  name: string
  email: string
  phoneNumber?: string | null
  role: 'Admin' | 'PM' | 'Contractor'
  isActive: boolean
}

export interface RegisterResponse {
  message: string
}
