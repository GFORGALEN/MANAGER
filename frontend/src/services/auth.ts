const TOKEN_KEY = 'construction_management_token'
const USERNAME_KEY = 'construction_management_username'
const ROLE_KEY = 'construction_management_role'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setSession(token: string, username: string, role: string): void {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USERNAME_KEY, username)
  localStorage.setItem(ROLE_KEY, role)
}

export function clearSession(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USERNAME_KEY)
  localStorage.removeItem(ROLE_KEY)
}

export function isAuthenticated(): boolean {
  return Boolean(getToken())
}

export function getCurrentUser() {
  return {
    username: localStorage.getItem(USERNAME_KEY) ?? 'Guest',
    role: localStorage.getItem(ROLE_KEY) ?? 'Unknown',
  }
}
