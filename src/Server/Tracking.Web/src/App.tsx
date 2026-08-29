import { useEffect, useState } from 'react'
import { isAuthenticated, logout } from './api/trackingApi'

import Dashboard from './pages/Dashboard'
import LiveMap from './pages/LiveMap'
import PluginManager from './pages/PluginManager'
import Login from './pages/Login'

import './App.css'

export type Language = 'ar' | 'en'

type Page =
  | 'dashboard'
  | 'map'
  | 'plugins'

function App() {
  const [page, setPage] = useState<Page>('dashboard')
  const [authenticated, setAuthenticated] = useState(() => isAuthenticated())

  useEffect(() => {
    const handleAuthChanged = () => {
      setAuthenticated(isAuthenticated())
    }

    window.addEventListener('tracking-auth-changed', handleAuthChanged)

    return () => {
      window.removeEventListener('tracking-auth-changed', handleAuthChanged)
    }
  }, [])

  const [language, setLanguage] = useState<Language>(() => {
    const saved = localStorage.getItem('tracking-language')
    return saved === 'en' ? 'en' : 'ar'
  })

  const [theme, setTheme] = useState<'dark' | 'light'>(() => {
    return localStorage.getItem('tracking-theme') === 'dark'
      ? 'dark'
      : 'light'
  })

  useEffect(() => {
    localStorage.setItem('tracking-language', language)

    document.documentElement.lang = language
    document.documentElement.dir =
      language === 'ar' ? 'rtl' : 'ltr'

    document.body.dir =
      language === 'ar' ? 'rtl' : 'ltr'
  }, [language])

  useEffect(() => {
    localStorage.setItem('tracking-theme', theme)
    document.documentElement.dataset.theme = theme
  }, [theme])

  const isArabic = language === 'ar'

  if (!authenticated) {
    return (
      <div className={`smart-app ${isArabic ? 'rtl' : 'ltr'}`}>
        <Login
          language={language}
          onLoginSuccess={() => setAuthenticated(true)}
          onLanguageChange={setLanguage}
        />
      </div>
    )
  }


  const pageTitle =
    page === 'dashboard'
      ? isArabic
        ? 'لوحة التحكم'
        : 'Dashboard'
      : page === 'map'
        ? isArabic
          ? 'الخريطة الحية'
          : 'Live Map'
        : isArabic
          ? 'إدارة الإضافات'
          : 'Plugin Manager'

  return (
    <div className={`smart-app ${isArabic ? 'rtl' : 'ltr'}`}>
      {/* ================= TOP BAR ================= */}

      <header className="smart-topbar">
        <div className="smart-topbar-left">
          <div className="smart-logo">
            <div className="smart-logo-text">
              <span className="smart-logo-smart">
                Smart
              </span>
              <span className="smart-logo-avl">
                AVL
              </span>
            </div>

            <div className="smart-logo-sub">
              GPS System &amp; GIS Solution
            </div>
          </div>

          <div className="smart-page-title">
            {pageTitle}
          </div>
        </div>

        <div className="smart-topbar-right">
          <button
            type="button"
            className="smart-icon-button"
            aria-label={
              isArabic ? 'الإشعارات' : 'Notifications'
            }
          >
            🔔
          </button>

          <div className="smart-avatar">
            ط
          </div>

          <div className="smart-admin-name">
            {isArabic
              ? 'مدير النظام'
              : 'System Admin'}
          </div>

          <button
            type="button"
            className="smart-logout-button"
            onClick={() => {
              logout()
              setAuthenticated(false)
              setPage('dashboard')
            }}
          >
            {isArabic ? 'خروج' : 'Logout'}
          </button>
          <button
            type="button"
            className="smart-language-button"
            onClick={() =>
              setLanguage(isArabic ? 'en' : 'ar')
            }
          >
            {isArabic ? 'EN' : 'ع'}
          </button>

          <button
            type="button"
            className="smart-theme-button"
            onClick={() =>
              setTheme(
                theme === 'light' ? 'dark' : 'light',
              )
            }
            aria-label={
              theme === 'light'
                ? 'Dark mode'
                : 'Light mode'
            }
          >
            {theme === 'light' ? '☾' : '☀'}
          </button>
        </div>
      </header>

      {/* ================= LAYOUT ================= */}

      <div className="smart-layout">
        {/* ================= SIDEBAR ================= */}

        <aside className="smart-sidebar">
          <div className="smart-hamburger">
            ☰
          </div>

          <nav className="smart-navigation">
            <button
              type="button"
              className={
                page === 'dashboard'
                  ? 'smart-nav-item active'
                  : 'smart-nav-item'
              }
              onClick={() => setPage('dashboard')}
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  📊
                </span>
                {isArabic
                  ? 'لوحة التحكم'
                  : 'Dashboard'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className={
                page === 'map'
                  ? 'smart-nav-item active'
                  : 'smart-nav-item'
              }
              onClick={() => setPage('map')}
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  🗺
                </span>
                {isArabic
                  ? 'الخريطة الحية'
                  : 'Live Map'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className="smart-nav-item"
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  👥
                </span>
                {isArabic
                  ? 'إدارة المستخدمين'
                  : 'User Management'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className="smart-nav-item"
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  🛡️
                </span>
                {isArabic
                  ? 'الصلاحيات'
                  : 'Permissions'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className="smart-nav-item"
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  🚗
                </span>
                {isArabic
                  ? 'المركبات'
                  : 'Units'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className="smart-nav-item"
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  📈
                </span>
                {isArabic
                  ? 'التقارير'
                  : 'Reports'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            {/* PLUGIN MANAGER */}

            <button
              type="button"
              className={
                page === 'plugins'
                  ? 'smart-nav-item active'
                  : 'smart-nav-item'
              }
              onClick={() => setPage('plugins')}
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  🧩
                </span>
                {isArabic
                  ? 'الإضافات'
                  : 'Plugins'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>

            <button
              type="button"
              className="smart-nav-item"
            >
              <span className="smart-nav-label">
                <span className="smart-nav-icon">
                  ⚙️
                </span>
                {isArabic
                  ? 'الإعدادات'
                  : 'Settings'}
              </span>

              <span className="smart-nav-plus">
                +
              </span>
            </button>
          </nav>
        </aside>

        {/* ================= MAIN ================= */}

        <main className="smart-main">
          {page === 'dashboard' && (
            <Dashboard language={language} />
          )}

          {page === 'map' && (
            <LiveMap language={language} />
          )}

          {page === 'plugins' && (
            <PluginManager language={language} />
          )}
        </main>
      </div>
    </div>
  )
}

export default App
