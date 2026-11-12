export interface DriverLoginResponse {
  accessToken: string
  driver: {
    id: number
    name: string
    nameAr?: string
    status: DriverStatus
    photoUrl?: string
  }
}

export type DriverStatus = 'available' | 'on_task' | 'offline' | 'on_leave'
export type TaskStatus =
  | 'new' | 'pending_acceptance' | 'accepted'
  | 'in_progress' | 'completed' | 'cancelled' | 'deferred'
export type TripStatus = 'not_started' | 'in_progress' | 'paused' | 'completed'
export type GpsSignal = 'strong' | 'weak' | 'disconnected'

export interface DriverTask {
  id: number
  title: string
  titleAr?: string
  status: TaskStatus
  sequence: number
  recommendedSequence: number
  pickupAddress?: string
  dropoffAddress?: string
  scheduledTime?: string
  distanceKm?: number
}

export interface DriverTrip {
  id: number
  taskId: number
  status: TripStatus
  startPoint?: string
  destination?: string
  distanceKm?: number
  startedAt?: string
  endedAt?: string
}

export interface DriverDashboard {
  status: DriverStatus
  gpsSignal: GpsSignal
  scheduledCount: number
  completedCount: number
  remainingCount: number
  cancelledCount: number
  currentTask?: DriverTask
  nextTask?: DriverTask
  onTimePercentage: number
  averageRating: number
  completedTripsTotal: number
  lastSyncAt?: string
}

export interface DriverNotification {
  id: number
  category: 'task' | 'trip' | 'vehicle' | 'chat' | 'admin' | 'emergency'
  title: string
  titleAr?: string
  body: string
  bodyAr?: string
  createdAt: string
  read: boolean
}

export interface ChatMessage {
  id: number
  threadId: string
  sender: 'driver' | 'management' | 'support' | 'client'
  text: string
  sentAt: string
  status: 'sent' | 'delivered' | 'read'
}

export interface DriverProfile {
  photoUrl?: string
  fullName: string
  fullNameAr?: string
  phone: string
  phoneVerified: boolean
  email?: string
  licenseNumber: string
  licenseExpiry: string
  nationalId: string
  employeeId: string
  joiningDate: string
  status: DriverStatus
  rating: number
}

export interface DriverVehicle {
  type: string
  plateNumber: string
  model: string
  year: number
  color: string
  vin: string
  registrationExpiry: string
  insuranceExpiry: string
  lastMaintenanceDate?: string
  status: 'operational' | 'needs_maintenance' | 'out_of_service'
  fuelLevel?: number
  latitude?: number
  longitude?: number
}

const TOKEN_KEY = 'driver-access-token'

export function getDriverToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function isDriverAuthenticated(): boolean {
  return !!getDriverToken()
}

export function driverLogout(): void {
  localStorage.removeItem(TOKEN_KEY)
  window.dispatchEvent(new Event('driver-auth-changed'))
}

async function driverFetch<T>(
  url: string,
  options: RequestInit = {},
): Promise<T> {
  const token = getDriverToken()

  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  })

  if (response.status === 401) {
    driverLogout()
    throw new Error('Authentication required')
  }

  if (!response.ok) {
    let detail = `Request failed: ${response.status}`
    try {
      const data = (await response.json()) as { message?: string; detail?: string }
      detail = data.message || data.detail || detail
    } catch {
      // no JSON body
    }
    throw new Error(detail)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export async function driverLogin(
  username: string,
  password: string,
): Promise<DriverLoginResponse> {
  const data = await driverFetch<DriverLoginResponse>('/api/driver/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })

  localStorage.setItem(TOKEN_KEY, data.accessToken)
  return data
}

export function getDriverDashboard(): Promise<DriverDashboard> {
  return driverFetch<DriverDashboard>('/api/driver/dashboard')
}

export function getDriverTasks(): Promise<DriverTask[]> {
  return driverFetch<DriverTask[]>('/api/driver/tasks')
}

export function reorderDriverTasks(orderedTaskIds: number[]): Promise<void> {
  return driverFetch<void>('/api/driver/tasks/reorder', {
    method: 'POST',
    body: JSON.stringify({ orderedTaskIds }),
  })
}

export function startDriverTask(taskId: number): Promise<DriverTrip> {
  return driverFetch<DriverTrip>(`/api/driver/tasks/${taskId}/start`, {
    method: 'POST',
  })
}

export function completeDriverTask(taskId: number): Promise<void> {
  return driverFetch<void>(`/api/driver/tasks/${taskId}/complete`, {
    method: 'POST',
  })
}

export function rejectDriverTask(taskId: number, reason: string): Promise<void> {
  return driverFetch<void>(`/api/driver/tasks/${taskId}/reject`, {
    method: 'POST',
    body: JSON.stringify({ reason }),
  })
}

export function pauseDriverTrip(tripId: number): Promise<DriverTrip> {
  return driverFetch<DriverTrip>(`/api/driver/trips/${tripId}/pause`, {
    method: 'POST',
  })
}

export function endDriverTrip(tripId: number): Promise<DriverTrip> {
  return driverFetch<DriverTrip>(`/api/driver/trips/${tripId}/end`, {
    method: 'POST',
  })
}

export function getDriverNotifications(): Promise<DriverNotification[]> {
  return driverFetch<DriverNotification[]>('/api/driver/notifications')
}

export function markNotificationRead(id: number): Promise<void> {
  return driverFetch<void>(`/api/driver/notifications/${id}/read`, {
    method: 'POST',
  })
}

export function markAllNotificationsRead(): Promise<void> {
  return driverFetch<void>('/api/driver/notifications/read-all', {
    method: 'POST',
  })
}

export function getChatMessages(threadId: string): Promise<ChatMessage[]> {
  return driverFetch<ChatMessage[]>(`/api/driver/chat/${threadId}/messages`)
}

export function sendChatMessage(threadId: string, text: string): Promise<ChatMessage> {
  return driverFetch<ChatMessage>(`/api/driver/chat/${threadId}/messages`, {
    method: 'POST',
    body: JSON.stringify({ text }),
  })
}

export function getDriverProfile(): Promise<DriverProfile> {
  return driverFetch<DriverProfile>('/api/driver/profile')
}

export function getDriverVehicle(): Promise<DriverVehicle> {
  return driverFetch<DriverVehicle>('/api/driver/vehicle')
}

export function sendSOS(latitude?: number, longitude?: number): Promise<void> {
  return driverFetch<void>('/api/driver/sos', {
    method: 'POST',
    body: JSON.stringify({ latitude, longitude }),
  })
}
