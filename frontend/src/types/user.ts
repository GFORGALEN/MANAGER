export interface UserSummary {
  userId: string
  name: string
  email: string
  phoneNumber?: string | null
  role: 'Admin' | 'PM' | 'Contractor'
  isActive: boolean
  createdAt: string
}

export interface UserDetail extends UserSummary {
  username: string
}

export interface UpdateUserPayload {
  name: string
  email: string
  phoneNumber?: string | null
}

export interface UpdateUserRolePayload {
  role: 'Admin' | 'PM' | 'Contractor'
}

export interface UpdateUserActivePayload {
  isActive: boolean
}
