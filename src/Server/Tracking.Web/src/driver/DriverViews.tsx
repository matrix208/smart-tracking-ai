import { useEffect, useState } from 'react'
import type { DriverLanguage } from './DriverApp'
import {
  getDriverDashboard, getDriverTasks, reorderDriverTasks,
  startDriverTask, completeDriverTask, rejectDriverTask,
  pauseDriverTrip, endDriverTrip,
  getDriverNotifications, markNotificationRead, markAllNotificationsRead,
  getChatMessages, sendChatMessage,
  getDriverProfile, getDriverVehicle,
  type DriverDashboard, type DriverTask, type DriverTrip,
  type DriverNotification, type ChatMessage, type DriverProfile, type DriverVehicle,
} from './api/driverApi'

interface LangProps { language: DriverLanguage }

const statusLabel = (s: string, ar: boolean) => {
  const map: Record<string, [string, string]> = {
    available: ['Available', 'متاح'],
    on_task: ['On Task', 'في مهمة'],
    offline: ['Offline', 'غير متصل'],
    on_leave: ['On Leave', 'إجازة'],
    new: ['New', 'جديدة'],
    pending_acceptance: ['Pending', 'بانتظار القبول'],
    accepted: ['Accepted', 'مقبولة'],
    in_progress: ['In Progress', 'جارية'],
    completed: ['Completed', 'مكتملة'],
    cancelled: ['Cancelled', 'ملغاة'],
    deferred: ['Deferred', 'مؤجلة'],
  }
  const entry = map[s]
  if (!entry) return s
  return ar ? entry[1] : entry[0]
}

/* ================= DASHBOARD ================= */
export function DriverDashboardView({
  language, onOpenTasks, onOpenTrip,
}: LangProps & { onOpenTasks: () => void; onOpenTrip: (taskId: number) => void }) {
  const isArabic = language === 'ar'
  const [data, setData] = useState<DriverDashboard | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getDriverDashboard()
      .then(setData)
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load'))
  }, [])

  if (error) return <div className="driver-error-box">{error}</div>
  if (!data) return <div className="driver-loading">{isArabic ? 'جاري التحميل...' : 'Loading...'}</div>

  return (
    <div className="driver-view">
      <div className="driver-status-card">
        <span className={`driver-status-pill ${data.status}`}>{statusLabel(data.status, isArabic)}</span>
        <span className={`driver-gps-pill ${data.gpsSignal}`}>
          GPS: {data.gpsSignal === 'strong' ? (isArabic ? 'قوي' : 'Strong')
            : data.gpsSignal === 'weak' ? (isArabic ? 'ضعيف' : 'Weak')
            : (isArabic ? 'غير متصل' : 'Disconnected')}
        </span>
      </div>

      <div className="driver-progress-summary">
        <div><strong>{data.completedCount}/{data.scheduledCount}</strong><span>{isArabic ? 'مهام مكتملة' : 'Completed'}</span></div>
        <div className="driver-progress-bar">
          <div style={{ width: `${data.scheduledCount ? (data.completedCount / data.scheduledCount) * 100 : 0}%` }} />
        </div>
      </div>

      {data.currentTask && (
        <div className="driver-task-highlight" onClick={() => onOpenTrip(data.currentTask!.id)}>
          <span className="driver-task-tag">{isArabic ? 'المهمة الحالية' : 'Current Task'}</span>
          <strong>{isArabic ? data.currentTask.titleAr ?? data.currentTask.title : data.currentTask.title}</strong>
          <small>{data.currentTask.dropoffAddress}</small>
        </div>
      )}

      {data.nextTask && (
        <div className="driver-task-highlight secondary">
          <span className="driver-task-tag">{isArabic ? 'المهمة التالية' : 'Next Task'}</span>
          <strong>{isArabic ? data.nextTask.titleAr ?? data.nextTask.title : data.nextTask.title}</strong>
          <small>{data.nextTask.scheduledTime}</small>
        </div>
      )}

      <div className="driver-kpi-grid">
        <div><strong>{data.completedTripsTotal}</strong><span>{isArabic ? 'رحلات مكتملة' : 'Completed Trips'}</span></div>
        <div><strong>{data.averageRating.toFixed(1)}</strong><span>{isArabic ? 'متوسط التقييم' : 'Avg Rating'}</span></div>
        <div><strong>{data.onTimePercentage}%</strong><span>{isArabic ? 'نسبة الالتزام بالوقت' : 'On-Time'}</span></div>
      </div>

      <div className="driver-quick-actions">
        <button onClick={onOpenTasks}>{isArabic ? 'عرض المهام' : 'View Tasks'}</button>
        {data.currentTask && (
          <button onClick={() => onOpenTrip(data.currentTask!.id)}>
            {isArabic ? 'إدارة الرحلة' : 'Manage Trip'}
          </button>
        )}
      </div>

      {data.lastSyncAt && (
        <div className="driver-sync-note">
          {isArabic ? 'آخر مزامنة: ' : 'Last sync: '}{data.lastSyncAt}
        </div>
      )}
    </div>
  )
}

/* ================= TASKS ================= */
export function DriverTasksView({
  language, onStartTrip,
}: LangProps & { onStartTrip: (taskId: number) => void }) {
  const isArabic = language === 'ar'
  const [tasks, setTasks] = useState<DriverTask[]>([])
  const [error, setError] = useState<string | null>(null)

  const load = () => {
    getDriverTasks()
      .then((data) => setTasks(data.slice().sort((a, b) => a.sequence - b.sequence)))
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load'))
  }

  useEffect(() => { load() }, [])

  const hasActiveTask = tasks.some((t) => t.status === 'in_progress')

  const move = async (index: number, direction: -1 | 1) => {
    const next = tasks.slice()
    const target = index + direction
    if (target < 0 || target >= next.length) return
    ;[next[index], next[target]] = [next[target], next[index]]
    setTasks(next)
    try {
      await reorderDriverTasks(next.map((t) => t.id))
    } catch {
      load()
    }
  }

  const handleStart = async (taskId: number) => {
    try {
      await startDriverTask(taskId)
      onStartTrip(taskId)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to start task')
    }
  }

  const handleComplete = async (taskId: number) => {
    try {
      await completeDriverTask(taskId)
      load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to complete task')
    }
  }

  const handleReject = async (taskId: number) => {
    const reason = window.prompt(isArabic ? 'سبب الرفض؟' : 'Reason for rejection?') ?? ''
    if (!reason.trim()) return
    try {
      await rejectDriverTask(taskId, reason.trim())
      load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to reject task')
    }
  }

  if (error) return <div className="driver-error-box">{error}</div>

  return (
    <div className="driver-view">
      <h2 className="driver-view-title">{isArabic ? 'مهامي' : 'My Tasks'}</h2>

      {tasks.map((task, index) => (
        <div key={task.id} className={`driver-task-card ${task.status}`}>
          <div className="driver-task-card-top">
            <span className={`driver-task-status ${task.status}`}>
              {statusLabel(task.status, isArabic)}
            </span>
            <div className="driver-reorder-buttons">
              <button disabled={index === 0 || hasActiveTask} onClick={() => move(index, -1)}>↑</button>
              <button disabled={index === tasks.length - 1 || hasActiveTask} onClick={() => move(index, 1)}>↓</button>
            </div>
          </div>

          <strong>{isArabic ? task.titleAr ?? task.title : task.title}</strong>
          {task.pickupAddress && <small>{isArabic ? 'الاستلام: ' : 'Pickup: '}{task.pickupAddress}</small>}
          {task.dropoffAddress && <small>{isArabic ? 'التسليم: ' : 'Drop-off: '}{task.dropoffAddress}</small>}
          {task.distanceKm != null && <small>{task.distanceKm} km</small>}

          <div className="driver-task-actions">
            {(task.status === 'new' || task.status === 'pending_acceptance' || task.status === 'accepted') && !hasActiveTask && (
              <>
                <button className="driver-btn-primary" onClick={() => handleStart(task.id)}>
                  {isArabic ? 'بدء المهمة' : 'Start Task'}
                </button>
                <button className="driver-btn-outline" onClick={() => handleReject(task.id)}>
                  {isArabic ? 'رفض' : 'Reject'}
                </button>
              </>
            )}
            {task.status === 'in_progress' && (
              <button className="driver-btn-primary" onClick={() => handleComplete(task.id)}>
                {isArabic ? 'إنهاء المهمة' : 'Complete Task'}
              </button>
            )}
          </div>
        </div>
      ))}

      {tasks.length === 0 && (
        <div className="driver-empty">{isArabic ? 'لا توجد مهام حاليًا' : 'No tasks right now'}</div>
      )}
    </div>
  )
}

/* ================= TRIP ================= */
export function DriverTripView({
  language, taskId, onDone,
}: LangProps & { taskId: number | null; onDone: () => void }) {
  const isArabic = language === 'ar'
  const [trip, setTrip] = useState<DriverTrip | null>(null)
  const [error, setError] = useState<string | null>(null)

  if (!taskId) {
    return <div className="driver-empty">{isArabic ? 'لا توجد رحلة نشطة' : 'No active trip'}</div>
  }

  const handlePause = async () => {
    if (!trip) return
    try { setTrip(await pauseDriverTrip(trip.id)) }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed') }
  }

  const handleEnd = async () => {
    if (!trip) return
    try {
      const finished = await endDriverTrip(trip.id)
      setTrip(finished)
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed') }
  }

  return (
    <div className="driver-view">
      <h2 className="driver-view-title">{isArabic ? 'إدارة الرحلة' : 'Trip Management'}</h2>

      {error && <div className="driver-error-box">{error}</div>}

      <div className="driver-trip-map-placeholder">
        {isArabic ? 'خريطة المسار' : 'Route Map'}
      </div>

      {trip && (
        <div className="driver-trip-summary">
          <div><span>{isArabic ? 'الحالة' : 'Status'}</span><strong>{statusLabel(trip.status, isArabic)}</strong></div>
          {trip.distanceKm != null && (
            <div><span>{isArabic ? 'المسافة' : 'Distance'}</span><strong>{trip.distanceKm} km</strong></div>
          )}
        </div>
      )}

      <div className="driver-trip-actions">
        <button className="driver-btn-outline" onClick={handlePause}>{isArabic ? 'إيقاف مؤقت' : 'Pause'}</button>
        <button className="driver-btn-primary" onClick={handleEnd}>{isArabic ? 'إنهاء الرحلة' : 'End Trip'}</button>
      </div>

      {trip?.status === 'completed' && (
        <div className="driver-trip-done">
          <p>{isArabic ? 'تم إنهاء الرحلة بنجاح.' : 'Trip completed successfully.'}</p>
          <button className="driver-btn-primary" onClick={onDone}>{isArabic ? 'رجوع للمهام' : 'Back to Tasks'}</button>
        </div>
      )}
    </div>
  )
}

/* ================= NOTIFICATIONS ================= */
export function DriverNotificationsView({ language }: LangProps) {
  const isArabic = language === 'ar'
  const [items, setItems] = useState<DriverNotification[]>([])

  const load = () => getDriverNotifications().then(setItems).catch(() => setItems([]))
  useEffect(() => { load() }, [])

  return (
    <div className="driver-view">
      <div className="driver-view-header-row">
        <h2 className="driver-view-title">{isArabic ? 'الإشعارات' : 'Notifications'}</h2>
        <button className="driver-btn-outline sm" onClick={() => markAllNotificationsRead().then(load)}>
          {isArabic ? 'تعليم الكل كمقروء' : 'Mark all read'}
        </button>
      </div>

      {items.map((n) => (
        <div
          key={n.id}
          className={`driver-notification-item ${n.category} ${n.read ? '' : 'unread'}`}
          onClick={() => markNotificationRead(n.id).then(load)}
        >
          <strong>{isArabic ? n.titleAr ?? n.title : n.title}</strong>
          <p>{isArabic ? n.bodyAr ?? n.body : n.body}</p>
          <small>{n.createdAt}</small>
        </div>
      ))}

      {items.length === 0 && (
        <div className="driver-empty">{isArabic ? 'لا توجد إشعارات' : 'No notifications'}</div>
      )}
    </div>
  )
}

/* ================= CHAT ================= */
export function DriverChatView({ language }: LangProps) {
  const isArabic = language === 'ar'
  const threadId = 'management'
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [text, setText] = useState('')

  const load = () => getChatMessages(threadId).then(setMessages).catch(() => setMessages([]))
  useEffect(() => { load() }, [])

  const handleSend = async () => {
    if (!text.trim()) return
    const value = text.trim()
    setText('')
    try {
      await sendChatMessage(threadId, value)
      load()
    } catch {
      // no-op, could show retry state
    }
  }

  return (
    <div className="driver-view driver-chat-view">
      <h2 className="driver-view-title">{isArabic ? 'الدردشة مع الإدارة' : 'Chat with Management'}</h2>

      <div className="driver-chat-messages">
        {messages.map((m) => (
          <div key={m.id} className={`driver-chat-bubble ${m.sender === 'driver' ? 'mine' : ''}`}>
            <p>{m.text}</p>
            <small>{m.sentAt} · {m.status}</small>
          </div>
        ))}
        {messages.length === 0 && (
          <div className="driver-empty">{isArabic ? 'ابدأ المحادثة' : 'Start the conversation'}</div>
        )}
      </div>

      <div className="driver-chat-input-row">
        <input
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSend()}
          placeholder={isArabic ? 'اكتب رسالة...' : 'Type a message...'}
        />
        <button onClick={handleSend}>{isArabic ? 'إرسال' : 'Send'}</button>
      </div>
    </div>
  )
}

/* ================= PROFILE ================= */
export function DriverProfileView({ language }: LangProps) {
  const isArabic = language === 'ar'
  const [profile, setProfile] = useState<DriverProfile | null>(null)
  const [vehicle, setVehicle] = useState<DriverVehicle | null>(null)
  const [revealSensitive, setRevealSensitive] = useState(false)

  useEffect(() => {
    getDriverProfile().then(setProfile).catch(() => setProfile(null))
    getDriverVehicle().then(setVehicle).catch(() => setVehicle(null))
  }, [])

  const mask = (value: string) =>
    revealSensitive ? value : `••••${value.slice(-4)}`

  if (!profile) return <div className="driver-loading">{isArabic ? 'جاري التحميل...' : 'Loading...'}</div>

  return (
    <div className="driver-view">
      <div className="driver-profile-card">
        <div className="driver-avatar-lg">{profile.fullName.slice(0, 1)}</div>
        <strong>{isArabic ? profile.fullNameAr ?? profile.fullName : profile.fullName}</strong>
        <span>{statusLabel(profile.status, isArabic)} · ⭐ {profile.rating.toFixed(1)}</span>
      </div>

      <div className="driver-profile-fields">
        <div><span>{isArabic ? 'الجوال' : 'Phone'}</span><strong>{profile.phone} {profile.phoneVerified ? '✔️' : ''}</strong></div>
        {profile.email && <div><span>{isArabic ? 'البريد' : 'Email'}</span><strong>{profile.email}</strong></div>}
        <div><span>{isArabic ? 'رقم الرخصة' : 'License No.'}</span><strong>{mask(profile.licenseNumber)}</strong></div>
        <div><span>{isArabic ? 'انتهاء الرخصة' : 'License Expiry'}</span><strong>{profile.licenseExpiry}</strong></div>
        <div><span>{isArabic ? 'الهوية/الإقامة' : 'National ID'}</span><strong>{mask(profile.nationalId)}</strong></div>
        <div><span>{isArabic ? 'رقم الموظف' : 'Employee ID'}</span><strong>{profile.employeeId}</strong></div>
        <div><span>{isArabic ? 'تاريخ الانضمام' : 'Joining Date'}</span><strong>{profile.joiningDate}</strong></div>
      </div>

      <button className="driver-btn-outline sm" onClick={() => setRevealSensitive((v) => !v)}>
        {revealSensitive ? (isArabic ? 'إخفاء البيانات الحساسة' : 'Hide sensitive data')
          : (isArabic ? 'إظهار البيانات الحساسة' : 'Show sensitive data')}
      </button>

      {vehicle && (
        <>
          <h3 className="driver-view-title sm">{isArabic ? 'بيانات المركبة' : 'Vehicle'}</h3>
          <div className="driver-profile-fields">
            <div><span>{isArabic ? 'النوع' : 'Type'}</span><strong>{vehicle.type}</strong></div>
            <div><span>{isArabic ? 'رقم اللوحة' : 'Plate'}</span><strong>{vehicle.plateNumber}</strong></div>
            <div><span>{isArabic ? 'الموديل' : 'Model'}</span><strong>{vehicle.model} ({vehicle.year})</strong></div>
            <div><span>{isArabic ? 'الحالة' : 'Status'}</span><strong>{vehicle.status}</strong></div>
            {vehicle.fuelLevel != null && (
              <div><span>{isArabic ? 'الوقود' : 'Fuel'}</span><strong>{vehicle.fuelLevel}%</strong></div>
            )}
          </div>
        </>
      )}
    </div>
  )
}
