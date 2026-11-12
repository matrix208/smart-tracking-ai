import { getAccessToken } from './trackingApi'

export interface RoleSummary {
  id: number
  name: string
  description: string
  isSystemRole: boolean
  permissionCount: number
  userCount: number
}

export interface RoleDetail {
  id: number
  name: string
  description: string
  isSystemRole: boolean
  permissionKeys: string[]
}

export interface Permission {
  id: number
  key: string
  name: string
  description: string
  category: string
}

export interface UserSummary {
  id: number
  username: string
  displayName: string
  isActive: boolean
  roles: string[]
}

async function apiFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const token = getAccessToken()

  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  })

  if (!response.ok) {
    let detail = `Request failed: ${response.status}`
    try {
      const data = (await response.json()) as { message?: string }
      detail = data.message || detail
    } catch {
      // no JSON body
    }
    throw new Error(detail)
  }

  if (response.status === 204) return undefined as T

  return response.json() as Promise<T>
}

export const getRoles = () => apiFetch<RoleSummary[]>('/api/roles')
export const getRole = (id: number) => apiFetch<RoleDetail>(`/api/roles/${id}`)
export const createRole = (name: string, description: string) =>
  apiFetch<{ id: number }>('/api/roles', {
    method: 'POST',
    body: JSON.stringify({ name, description, permissionKeys: [] }),
  })
export const updateRole = (id: number, name: string, description: string) =>
  apiFetch<void>(`/api/roles/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ name, description }),
  })
export const deleteRole = (id: number) =>
  apiFetch<void>(`/api/roles/${id}`, { method: 'DELETE' })
export const setRolePermissions = (id: number, permissionKeys: string[]) =>
  apiFetch<void>(`/api/roles/${id}/permissions`, {
    method: 'PUT',
    body: JSON.stringify({ permissionKeys }),
  })

export const getPermissions = () => apiFetch<Permission[]>('/api/permissions')

export const getUsers = () => apiFetch<UserSummary[]>('/api/users')
export const createUser = (
  username: string,
  password: string,
  displayName: string,
  roleIds: number[],
) =>
  apiFetch<void>('/api/users', {
    method: 'POST',
    body: JSON.stringify({ username, password, displayName, roleIds }),
  })
export const setUserRoles = (userId: number, roleIds: number[]) =>
  apiFetch<void>(`/api/users/${userId}/roles`, {
    method: 'PUT',
    body: JSON.stringify({ roleIds }),
  })
export const setUserActive = (userId: number, isActive: boolean) =>
  apiFetch<void>(`/api/users/${userId}/status`, {
    method: 'PUT',
    body: JSON.stringify({ isActive }),
  })
