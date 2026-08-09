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

async function getJson<T>(url: string): Promise<T> {
  const response = await fetch(url)

  if (!response.ok) {
    throw new Error(
      `API request failed: ${response.status}`,
    )
  }

  return response.json() as Promise<T>
}

export function getDevices() {
  return getJson<Device[]>('/api/devices')
}

export function getPositions() {
  return getJson<Position[]>('/api/positions')
}

export function getDeviceStates() {
  return getJson<DeviceState[]>('/api/devicestates')
}
