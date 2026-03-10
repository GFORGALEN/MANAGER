import type { CurrentUser } from '@/types/auth'

const TOKEN_KEY = 'construction_management_token'
const CURRENT_USER_KEY = 'construction_management_current_user'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setSession(token: string, currentUser: CurrentUser): void {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(CURRENT_USER_KEY, JSON.stringify(currentUser))
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token)
}

export function setCurrentUser(currentUser: CurrentUser): void {
  localStorage.setItem(CURRENT_USER_KEY, JSON.stringify(currentUser))
}

export function clearSession(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(CURRENT_USER_KEY)
}

export function isAuthenticated(): boolean {
  return Boolean(getToken())
}

export function getCurrentUser(): CurrentUser | null {
  const rawValue = localStorage.getItem(CURRENT_USER_KEY)
  if (!rawValue) {
    return null
  }

  try {
    return JSON.parse(rawValue) as CurrentUser
  } catch {
    return null
  }
}

export function getCurrentRole(): CurrentUser['role'] | null {
  return getCurrentUser()?.role ?? null
}

export function getCurrentUserLabel(): string {
  const currentUser = getCurrentUser()
  return currentUser?.name ?? currentUser?.email ?? 'Guest'
}

export function getDefaultRouteForRole(role: string | null | undefined): string {
  return role === 'Contractor' ? '/worker/dashboard' : '/projects'
}

export function hasRole(roles?: string[]): boolean {
  if (!roles || roles.length === 0) {
    return true
  }

  const currentRole = getCurrentRole()
  return currentRole ? roles.includes(currentRole) : false
}
