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

export function getDevices(): Promise<Device[]> {
  return getJson<Device[]>('/api/devices')
}

export function getPositions(): Promise<Position[]> {
  return getJson<Position[]>('/api/positions')
}

export function getDeviceStates(): Promise<DeviceState[]> {
  return getJson<DeviceState[]>('/api/devicestates')
}
