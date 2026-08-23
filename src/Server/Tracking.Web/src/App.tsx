import { useEffect, useState } from 'react'
import Dashboard from './pages/Dashboard'
import LiveMap from './pages/LiveMap'
import './App.css'

export type Language = 'ar' | 'en'


function App() {
  const [page, setPage] = useState<'dashboard' | 'map'>('dashboard')
  const [language, setLanguage] = useState<Language>(() => {
    const saved = localStorage.getItem('tracking-language')
    return saved === 'en' ? 'en' : 'ar'
  })





  useEffect(() => {
    localStorage.setItem('tracking-language', language)
    document.documentElement.lang = language
    document.documentElement.dir = language === 'ar' ? 'rtl' : 'ltr'
    document.body.dir = language === 'ar' ? 'rtl' : 'ltr'
  }, [language])


  const [theme, setTheme] = useState<'dark' | 'light'>(() => {
    return localStorage.getItem('tracking-theme') === 'light'
      ? 'light'
      : 'dark'
  })

  useEffect(() => {
    localStorage.setItem('tracking-theme', theme)
    document.documentElement.dataset.theme = theme
  }, [theme])

  const isArabic = language === 'ar'

  return (
    <div className={`app-shell ${isArabic ? 'rtl' : 'ltr'}`}>
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">T</div>

          <div>
            <div className="brand-name">Tracking</div>
            <div className="brand-sub">PLATFORM</div>
          </div>
        </div>

        <nav className="nav">
          <div className="nav-label">
            {isArabic ? 'الرئيسية' : 'MAIN'}
          </div>

          <button
            type="button"
            className={page === 'dashboard' ? 'active' : ''}
            onClick={() => setPage('dashboard')}
          >
            <span>▦</span>
            {isArabic ? 'لوحة التحكم' : 'Dashboard'}
          </button>

          <button
            type="button"
            className={page === 'map' ? 'active' : ''}
            onClick={() => setPage('map')}
          >
            <span>⌖</span>
            {isArabic ? 'الخريطة الحية' : 'Live Map'}
          </button>

          <a href="#">
            <span>▣</span>
            {isArabic ? 'المركبات' : 'Vehicles'}
          </a>

          <a href="#">
            <span>♙</span>
            {isArabic ? 'السائقون' : 'Drivers'}
          </a>

          <div className="nav-label">
            {isArabic ? 'المراقبة' : 'MONITOR'}
          </div>

          <a href="#">
            <span>⚠</span>
            {isArabic ? 'التنبيهات' : 'Alerts'}
          </a>

          <a href="#">
            <span>⌁</span>
            {isArabic ? 'التقارير' : 'Reports'}
          </a>

          <a href="#">
            <span>◷</span>
            {isArabic ? 'سجل الرحلات' : 'Trip History'}
          </a>

          <div className="nav-label">
            {isArabic ? 'الإعدادات' : 'SETTINGS'}
          </div>

          <a href="#">
            <span>⚙</span>
            {isArabic ? 'الإعدادات' : 'Settings'}
          </a>
        </nav>

        <div className="sidebar-bottom">
          <div className="server-status">
            <span className="status-dot online" />
            <div>
              <strong>GT06 Server</strong>
              <small>Port 5001</small>
            </div>
          </div>

          <button
            type="button"
            className="theme-switch"
            onClick={() =>
              setTheme(theme === 'dark' ? 'light' : 'dark')
            }
            aria-label={
              theme === 'dark'
                ? 'تفعيل الوضع الفاتح'
                : 'تفعيل الوضع الداكن'
            }
          >
            <span className="theme-icon">
              {theme === 'dark' ? '☀' : '☾'}
            </span>
            <span className="theme-label">
              {theme === 'dark' ? 'الوضع الفاتح' : 'الوضع الداكن'}
            </span>
            <span className="theme-state">
              {theme === 'dark' ? 'DARK' : 'LIGHT'}
            </span>
          </button>

          <button
            className="language-switch"
            onClick={() =>
              setLanguage(isArabic ? 'en' : 'ar')
            }
            aria-label={
              isArabic
                ? 'التبديل إلى الإنجليزية'
                : 'Switch to Arabic'
            }
          >
            <span className="lang-current">
              {isArabic ? 'عربي' : 'English'}
            </span>
            <span className="language-arrow">⇄</span>
            <span className="lang-other">
              {isArabic ? 'English' : 'عربي'}
            </span>
          </button>

          <div className="user-card">
            <div className="avatar">ط</div>

            <div>
              <div className="user-name">
                {isArabic ? 'مدير النظام' : 'System Admin'}
              </div>

              <div className="user-role">
                {isArabic ? 'مدير الأسطول' : 'Fleet Manager'}
              </div>
            </div>
          </div>
        </div>
      </aside>

      <main className="main">
        {page === 'dashboard' ? (
          <Dashboard language={language} />
        ) : (
          <LiveMap language={language} />
        )}
      </main>
    </div>
  )
}

export default App
