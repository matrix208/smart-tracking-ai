export interface LoginResponse {
  accessToken: string
  tokenType: string
  expiresIn: number
  user: {
    id: number
    username: string
    displayName: string
    role: string
  }
}

export interface Device {
  id?: number
  imei: string
  name?: string
  description?: string
  protocol?: string
  isOnline: boolean
  lastSeen: string
  lastLatitude?: number | null
  lastLongitude?: number | null
  lastSpeed?: number | null
  lastCourse?: number | null
  lastPositionTime?: string | null
}

export interface Position {
  deviceId: string
  latitude: number
  longitude: number
  speed: number
  course: number
  valid: boolean
  deviceTime: string
  serverTime: string
}

export interface DeviceState {
  deviceId: string
  lastUpdate: string
  latitude: number
  longitude: number
  speed: number
  course: number
  online: boolean
  ignition: boolean
  satellites: number
  battery?: number | null
  signal?: number | null
}

export interface PluginRepositoryPackage {
  packageId: string
  displayName: string
  description: string
  version: string
  sdkVersion: string
  minServerVersion: string
  manufacturer: string
  company: string
  author: string
  type: number
  assembly: string
  entryPoint: string
  icon: string
  readme: string
  license: string
  defaultPort: number
  supportsTcp: boolean
  supportsUdp: boolean
  permissions: string[]
  dependencies: string[]
}

export interface PluginRuntimeState {
  id: string
  name: string
  version: string
  description: string
  author: string
  manufacturer: string
  defaultPort: number
  supportsTcp: boolean
  supportsUdp: boolean
  models: string[]
  capabilities: string[]
  enabled: boolean
}

const TOKEN_KEY = 'tracking-access-token'

export function getAccessToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function isAuthenticated(): boolean {
  return !!getAccessToken()
}

export async function login(
  username: string,
  password: string,
): Promise<LoginResponse> {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      username,
      password,
    }),
  })

  if (!response.ok) {
    throw new Error(`Login failed: ${response.status}`)
  }

  const data = (await response.json()) as LoginResponse

  localStorage.setItem(TOKEN_KEY, data.accessToken)

  return data
}

export function logout(): void {
  localStorage.removeItem(TOKEN_KEY)
  window.dispatchEvent(new Event('tracking-auth-changed'))
}

async function getJson<T>(url: string): Promise<T> {
  const token = getAccessToken()

  const response = await fetch(url, {
    headers: token
      ? {
          Authorization: `Bearer ${token}`,
        }
      : undefined,
  })

  if (response.status === 401) {
    logout()
    throw new Error('Authentication required')
  }

  if (!response.ok) {
    throw new Error(`API request failed: ${response.status}`)
  }

  return response.json() as Promise<T>
}

export async function setPluginEnabled(
  pluginId: string,
  enabled: boolean,
): Promise<void> {
  const token = getAccessToken()

  const response = await fetch(
    `/api/plugins/${encodeURIComponent(pluginId)}/${enabled ? 'enable' : 'disable'}`,
    {
      method: 'POST',
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (response.status === 401) {
    logout()
    throw new Error('Authentication required')
  }

  if (!response.ok) {
    throw new Error(`Plugin update failed: ${response.status}`)
  }
}

export async function installPlugin(
  pluginId: string,
): Promise<void> {
  const token = getAccessToken()

  const response = await fetch(
    `/api/plugins/${encodeURIComponent(pluginId)}/install`,
    {
      method: 'POST',
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (response.status === 401) {
    logout()
    throw new Error('Authentication required')
  }

  if (!response.ok) {
    let detail = `Plugin installation failed: ${response.status}`

    try {
      const data = (await response.json()) as {
        detail?: string
        message?: string
      }

      detail = data.detail || data.message || detail
    } catch {
      // Keep the HTTP status message when the response has no JSON body.
    }

    throw new Error(detail)
  }
}

export function getPlugins(): Promise<PluginRuntimeState[]> {
  return getJson<PluginRuntimeState[]>('/api/plugins')
}

export function getDevices(): Promise<Device[]> {
  return getJson<Device[]>('/api/devices')
}

export function getPositions(): Promise<Position[]> {
  return getJson<Position[]>('/api/positions')
}

export function getDeviceStates(): Promise<DeviceState[]> {
  return getJson<DeviceState[]>('/api/devicestates')
}



export function getPluginRepository(): Promise<PluginRepositoryPackage[]> {
  return getJson<PluginRepositoryPackage[]>('/api/plugins/repository')
}
