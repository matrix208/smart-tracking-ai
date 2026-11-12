import { useEffect, useState } from 'react'
import {
  isDriverAuthenticated,
  driverLogin,
  driverLogout,
  sendSOS,
} from './api/driverApi'
import {
  DriverDashboardView,
  DriverTasksView,
  DriverTripView,
  DriverNotificationsView,
  DriverChatView,
  DriverProfileView,
} from './DriverViews'
import './driver.css'

export type DriverLanguage = 'ar' | 'en'
type DriverView = 'dashboard' | 'tasks' | 'trip' | 'notifications' | 'chat' | 'profile'

function DriverApp() {
  const [language, setLanguage] = useState<DriverLanguage>(() => {
    const saved = localStorage.getItem('driver-language')
    return saved === 'en' ? 'en' : 'ar'
  })
  const [authenticated, setAuthenticated] = useState(() => isDriverAuthenticated())
  const [view, setView] = useState<DriverView>('dashboard')
  const [activeTaskId, setActiveTaskId] = useState<number | null>(null)

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [loginLoading, setLoginLoading] = useState(false)
  const [loginError, setLoginError] = useState<string | null>(null)

  const [sosConfirming, setSosConfirming] = useState(false)
  const [sosSending, setSosSending] = useState(false)

  const isArabic = language === 'ar'

  useEffect(() => {
    const handler = () => setAuthenticated(isDriverAuthenticated())
    window.addEventListener('driver-auth-changed', handler)
    return () => window.removeEventListener('driver-auth-changed', handler)
  }, [])

  useEffect(() => {
    localStorage.setItem('driver-language', language)
    document.documentElement.dir = isArabic ? 'rtl' : 'ltr'
    document.documentElement.lang = language
  }, [language, isArabic])

  const handleLogin = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!username.trim() || !password) {
      setLoginError(isArabic ? 'أدخل اسم المستخدم وكلمة المرور' : 'Enter username and password')
      return
    }
    setLoginLoading(true)
    setLoginError(null)
    try {
      await driverLogin(username.trim(), password)
      setAuthenticated(true)
    } catch (err) {
      setLoginError(
        err instanceof Error
          ? err.message
          : isArabic ? 'تعذر تسجيل الدخول' : 'Unable to sign in',
      )
    } finally {
      setLoginLoading(false)
    }
  }

  const handleSOS = async () => {
    setSosSending(true)
    try {
      let coords: GeolocationCoordinates | undefined
      if (navigator.geolocation) {
        coords = await new Promise<GeolocationCoordinates | undefined>((resolve) => {
          navigator.geolocation.getCurrentPosition(
            (pos) => resolve(pos.coords),
            () => resolve(undefined),
            { timeout: 3000 },
          )
        })
      }
      await sendSOS(coords?.latitude, coords?.longitude)
    } catch {
      // best-effort even if it fails; SOS UI still confirms attempt
    } finally {
      setSosSending(false)
      setSosConfirming(false)
    }
  }

  if (!authenticated) {
    return (
      <div className={`driver-app ${isArabic ? 'rtl' : 'ltr'}`} dir={isArabic ? 'rtl' : 'ltr'}>
        <div className="driver-login-page">
          <div className="driver-login-card">
            <div className="driver-login-brand">
              <span>Smart</span>
              <strong>AVL</strong>
              <small>{isArabic ? 'تطبيق السائق' : 'Driver App'}</small>
            </div>

            <form onSubmit={handleLogin} className="driver-login-form">
              <label>{isArabic ? 'اسم المستخدم' : 'Username'}</label>
              <input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                autoFocus
                disabled={loginLoading}
                placeholder={isArabic ? 'اسم المستخدم' : 'Username'}
              />

              <label>{isArabic ? 'كلمة المرور' : 'Password'}</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={loginLoading}
                placeholder={isArabic ? 'كلمة المرور' : 'Password'}
              />

              {loginError && <div className="driver-login-error">{loginError}</div>}

              <button type="submit" disabled={loginLoading}>
                {loginLoading
                  ? isArabic ? 'جاري الدخول...' : 'Signing in...'
                  : isArabic ? 'دخول' : 'Sign In'}
              </button>
            </form>

            <button
              type="button"
              className="driver-lang-toggle"
              onClick={() => setLanguage(isArabic ? 'en' : 'ar')}
            >
              {isArabic ? 'English' : 'العربية'}
            </button>
          </div>
        </div>
      </div>
    )
  }

  const navItems: { id: DriverView; icon: string; label: string }[] = [
    { id: 'dashboard', icon: '🏠', label: isArabic ? 'الرئيسية' : 'Home' },
    { id: 'tasks', icon: '📋', label: isArabic ? 'المهام' : 'Tasks' },
    { id: 'trip', icon: '🧭', label: isArabic ? 'الرحلة' : 'Trip' },
    { id: 'notifications', icon: '🔔', label: isArabic ? 'الإشعارات' : 'Alerts' },
    { id: 'chat', icon: '💬', label: isArabic ? 'الدردشة' : 'Chat' },
    { id: 'profile', icon: '👤', label: isArabic ? 'حسابي' : 'Profile' },
  ]

  return (
    <div className={`driver-app ${isArabic ? 'rtl' : 'ltr'}`} dir={isArabic ? 'rtl' : 'ltr'}>
      <header className="driver-topbar">
        <div className="driver-topbar-brand">Smart<strong>AVL</strong></div>
        <div className="driver-topbar-actions">
          <button onClick={() => setLanguage(isArabic ? 'en' : 'ar')}>
            {isArabic ? 'EN' : 'ع'}
          </button>
          <button onClick={() => { driverLogout(); }}>
            {isArabic ? 'خروج' : 'Logout'}
          </button>
        </div>
      </header>

      <main className="driver-main">
        {view === 'dashboard' && (
          <DriverDashboardView
            language={language}
            onOpenTasks={() => setView('tasks')}
            onOpenTrip={(taskId) => { setActiveTaskId(taskId); setView('trip') }}
          />
        )}
        {view === 'tasks' && (
          <DriverTasksView
            language={language}
            onStartTrip={(taskId) => { setActiveTaskId(taskId); setView('trip') }}
          />
        )}
        {view === 'trip' && (
          <DriverTripView language={language} taskId={activeTaskId} onDone={() => setView('tasks')} />
        )}
        {view === 'notifications' && <DriverNotificationsView language={language} />}
        {view === 'chat' && <DriverChatView language={language} />}
        {view === 'profile' && <DriverProfileView language={language} />}
      </main>

      <nav className="driver-bottom-nav">
        {navItems.map((item) => (
          <button
            key={item.id}
            className={view === item.id ? 'active' : ''}
            onClick={() => setView(item.id)}
          >
            <span>{item.icon}</span>
            {item.label}
          </button>
        ))}
      </nav>

      {/* SOS is always visible and cannot be hidden */}
      <button
        type="button"
        className="driver-sos-fab"
        onClick={() => setSosConfirming(true)}
        aria-label="SOS"
      >
        SOS
      </button>

      {sosConfirming && (
        <div className="driver-sos-overlay">
          <div className="driver-sos-dialog">
            <h3>{isArabic ? 'تأكيد نداء الطوارئ' : 'Confirm Emergency Alert'}</h3>
            <p>
              {isArabic
                ? 'سيتم إرسال موقعك الحالي وتنبيه فوري للإدارة والدعم الفني.'
                : 'Your current location will be sent with an immediate alert to management and support.'}
            </p>
            <div className="driver-sos-actions">
              <button className="driver-sos-cancel" onClick={() => setSosConfirming(false)} disabled={sosSending}>
                {isArabic ? 'إلغاء' : 'Cancel'}
              </button>
              <button className="driver-sos-confirm" onClick={handleSOS} disabled={sosSending}>
                {sosSending
                  ? isArabic ? 'جاري الإرسال...' : 'Sending...'
                  : isArabic ? 'إرسال SOS' : 'Send SOS'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default DriverApp
