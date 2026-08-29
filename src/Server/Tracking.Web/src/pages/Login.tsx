import { useState } from 'react'
import type { FormEvent } from 'react'
import { login } from '../api/trackingApi'
import type { Language } from '../App'

interface LoginProps {
  language: Language
  onLoginSuccess: () => void
  onLanguageChange: (language: Language) => void
}

function Login({ language, onLoginSuccess, onLanguageChange }: LoginProps) {
  const isArabic = language === 'ar'

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(true)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!username.trim() || !password) {
      setError(
        isArabic
          ? 'يرجى إدخال اسم المستخدم وكلمة المرور.'
          : 'Please enter your username and password.',
      )
      return
    }

    setLoading(true)
    setError(null)

    try {
      await login(username.trim(), password)
      onLoginSuccess()
    } catch (err) {
      console.error('Login:', err)

      setError(
        err instanceof Error
          ? isArabic && err.message === 'Login failed: 401'
            ? 'اسم المستخدم أو كلمة المرور غير صحيحة.'
            : err.message
          : isArabic
            ? 'تعذر تسجيل الدخول.'
            : 'Unable to sign in.',
      )
    } finally {
      setLoading(false)
    }
  }

  const fillDemoCredentials = () => {
    setUsername('demo')
    setPassword('demo1234')
    setError(null)
  }

  return (
    <div
      className={`login-page ${isArabic ? 'rtl' : 'ltr'}`}
      dir={isArabic ? 'rtl' : 'ltr'}
    >
      <div className="login-card">
        <div className="login-brand">
          <div className="login-brand-title">
            <span>Smart</span>
            <strong>AVL</strong>
          </div>
          <div className="login-brand-subtitle">
            GPS System &amp; GIS Solution
          </div>
        </div>

        <p className="login-tagline">
          {isArabic
            ? 'سجّل الدخول للوصول إلى حسابك'
            : 'Sign in to your account to continue'}
        </p>

        <form className="login-form" onSubmit={handleSubmit}>
          <label htmlFor="login-username">
            {isArabic ? 'اسم المستخدم' : 'User Name'}
          </label>
          <input
            id="login-username"
            type="text"
            value={username}
            onChange={(event) => setUsername(event.target.value)}
            autoComplete="username"
            autoFocus
            disabled={loading}
            placeholder={isArabic ? 'اسم المستخدم' : 'User Name'}
          />

          <label htmlFor="login-password">
            {isArabic ? 'كلمة المرور' : 'Password'}
          </label>
          <input
            id="login-password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            disabled={loading}
            placeholder={isArabic ? 'كلمة المرور' : 'Password'}
          />

          <div className="login-form-row">
            <label className="login-remember">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(event) => setRememberMe(event.target.checked)}
              />
              {isArabic ? 'تذكرني' : 'Remember me'}
            </label>

            <button type="button" className="login-forgot">
              {isArabic ? 'نسيت كلمة المرور؟' : 'Forgot password?'}
            </button>
          </div>

          {error && (
            <div className="login-error" role="alert">
              {error}
            </div>
          )}

          <button type="submit" className="login-submit" disabled={loading}>
            {loading
              ? isArabic
                ? 'جاري تسجيل الدخول...'
                : 'Signing in...'
              : isArabic
                ? 'دخول'
                : 'Sign In'}
          </button>
        </form>

        <button type="button" className="login-register">
          {isArabic ? 'إنشاء حساب' : 'Registration'}
        </button>

        <button type="button" className="login-demo" onClick={fillDemoCredentials}>
          {isArabic ? 'تجربة العرض التوضيحي' : 'Click For Demo'}
        </button>

        <div className="login-language">
          <select
            value={language}
            onChange={(event) => onLanguageChange(event.target.value as Language)}
          >
            <option value="ar">العربية</option>
            <option value="en">English</option>
          </select>
        </div>

        <div className="login-stores">
          <span className="login-store-icon" aria-label="App Store">
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M16.365 1.43c0 1.14-.493 2.27-1.177 3.08-.744.9-2.02 1.6-3.03 1.6-.12-1.14.46-2.32 1.15-3.08.79-.87 2.15-1.53 3.06-1.6zM20.1 17.1c-.5 1.15-.75 1.66-1.4 2.67-.9 1.4-2.16 3.14-3.72 3.16-1.4.02-1.76-.9-3.66-.89-1.9 0-2.3.9-3.7.9-1.56-.02-2.76-1.6-3.66-3-2.5-3.9-2.77-8.48-1.22-10.92 1.1-1.75 2.85-2.77 4.5-2.77 1.68 0 2.74.94 4.13.94 1.35 0 2.16-.94 4.14-.94 1.47 0 3.03.8 4.14 2.18-3.64 2-3.05 7.2.45 8.67z" />
            </svg>
          </span>
          <span className="login-store-icon" aria-label="Google Play">
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M3.6 2.3c-.3.3-.5.8-.5 1.4v16.6c0 .6.2 1.1.5 1.4l.1.1L13.2 12 3.7 2.2l-.1.1z" />
              <path d="m16.3 15.1-3.1-3.1 3.1-3.1 3.6 2.1c1 .6 1 1.5 0 2.1l-3.6 2z" opacity=".7" />
              <path d="M13.2 12 3.7 21.8c.3.3.8.4 1.3.1l11-6.3-2.8-2.6z" />
              <path d="M13.2 12 16 9.2l-11-6.3c-.5-.3-1-.2-1.3.1L13.2 12z" opacity=".85" />
            </svg>
          </span>
        </div>
      </div>

      <div className="login-illustration">
        <svg viewBox="0 0 620 420" className="login-illustration-svg">
          <circle cx="470" cy="80" r="130" fill="var(--accent-soft)" />
          <circle cx="90" cy="110" r="14" fill="var(--accent-soft)" />
          <circle cx="310" cy="210" r="150" fill="none" stroke="var(--line)" strokeWidth="1.5" />
          <circle cx="310" cy="210" r="100" fill="none" stroke="var(--line)" strokeWidth="1.5" />

          <g fontFamily="Cairo, sans-serif" fontSize="13" fill="var(--muted)" textAnchor="middle">
            <circle cx="380" cy="95" r="22" fill="var(--accent)" />
            <text x="380" y="60" fontWeight="700" fill="var(--ink)">GPS</text>

            <circle cx="465" cy="180" r="28" fill="var(--accent)" />
            <text x="465" y="130" fontWeight="700" fill="var(--ink)">Telematics</text>
            <text x="465" y="146">Telemetry</text>

            <circle cx="240" cy="150" r="18" fill="var(--accent)" />
            <text x="210" y="110" fontWeight="700" fill="var(--ink)">Bluetooth</text>
            <text x="210" y="126">Wi-Fi / LTE</text>

            <circle cx="195" cy="250" r="20" fill="var(--accent)" />
            <text x="165" y="295" fontWeight="700" fill="var(--ink)">Engine Data</text>
            <text x="165" y="311">Chassis Data</text>

            <circle cx="310" cy="210" r="18" fill="var(--ink)" />
            <text x="310" y="248" fontWeight="700" fill="var(--ink)">OEM</text>

            <circle cx="425" cy="270" r="13" fill="var(--accent)" />
            <text x="425" y="298" fontWeight="700" fill="var(--ink)">PGN</text>
          </g>

          <path d="M170 360 L560 360 L520 320 L210 320 Z" fill="var(--accent)" opacity=".12" />
          <g transform="translate(180,270)">
            <rect x="0" y="40" width="300" height="60" rx="20" fill="var(--panel)" stroke="var(--line)" />
            <circle cx="55" cy="102" r="20" fill="var(--ink)" />
            <circle cx="245" cy="102" r="20" fill="var(--ink)" />
            <rect x="30" y="8" width="240" height="38" rx="14" fill="var(--panel)" stroke="var(--line)" />
          </g>
        </svg>

        <p className="login-illustration-caption">
          {isArabic
            ? 'مراقبة الأسطول وتتبعه في الوقت الحقيقي'
            : 'Real-time fleet monitoring and tracking'}
        </p>
      </div>
    </div>
  )
}

export default Login
