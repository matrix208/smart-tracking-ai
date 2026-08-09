import { useEffect, useRef, useState } from 'react'
import { Dashboard } from './pages/Dashboard'
import './App.css'

export type Language = 'ar' | 'en'

const ACCENT_PRESETS = [
  { hex: '#17e3a8', ar: 'أخضر إشارة', en: 'Signal green' },
  { hex: '#5b9dff', ar: 'أزرق', en: 'Blue' },
  { hex: '#f4b740', ar: 'كهرماني', en: 'Amber' },
  { hex: '#ff5d6c', ar: 'أحمر', en: 'Red' },
  { hex: '#a78bfa', ar: 'بنفسجي', en: 'Purple' },
  { hex: '#f472b6', ar: 'وردي', en: 'Pink' },
]

function hexToRgb(hex: string) {
  const clean = hex.replace('#', '')
  const full =
    clean.length === 3
      ? clean.split('').map((c) => c + c).join('')
      : clean
  const value = Number.parseInt(full, 16)

  if (Number.isNaN(value)) {
    return { r: 23, g: 227, b: 168 }
  }

  return {
    r: (value >> 16) & 255,
    g: (value >> 8) & 255,
    b: value & 255,
  }
}

function App() {
  const [language, setLanguage] = useState<Language>(() => {
    const saved = localStorage.getItem('tracking-language')
    return saved === 'en' ? 'en' : 'ar'
  })

  const [accentColor, setAccentColor] = useState<string>(() => {
    return localStorage.getItem('tracking-accent') || '#17e3a8'
  })

  const [colorMenuOpen, setColorMenuOpen] = useState(false)
  const colorPickerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!colorMenuOpen) return

    function handleClickOutside(event: MouseEvent) {
      if (
        colorPickerRef.current &&
        !colorPickerRef.current.contains(event.target as Node)
      ) {
        setColorMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () =>
      document.removeEventListener('mousedown', handleClickOutside)
  }, [colorMenuOpen])

  useEffect(() => {
    localStorage.setItem('tracking-language', language)
    document.documentElement.lang = language
    document.documentElement.dir = language === 'ar' ? 'rtl' : 'ltr'
    document.body.dir = language === 'ar' ? 'rtl' : 'ltr'
  }, [language])

  useEffect(() => {
    localStorage.setItem('tracking-accent', accentColor)

    const { r, g, b } = hexToRgb(accentColor)
    const root = document.documentElement

    root.style.setProperty('--signal', accentColor)
    root.style.setProperty('--signal-dim', `rgba(${r}, ${g}, ${b}, 0.11)`)
  }, [accentColor])

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

          <a className="active" href="#">
            <span>▦</span>
            {isArabic ? 'لوحة التحكم' : 'Dashboard'}
          </a>

          <a href="#">
            <span>⌖</span>
            {isArabic ? 'الخريطة الحية' : 'Live Map'}
          </a>

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

          <div className="color-picker" ref={colorPickerRef}>
            <button
              type="button"
              className="color-picker-trigger"
              onClick={() => setColorMenuOpen((open) => !open)}
              aria-expanded={colorMenuOpen}
              aria-label={
                isArabic
                  ? 'تخصيص لون الواجهة'
                  : 'Customize accent color'
              }
            >
              <span
                className="color-swatch"
                style={{ background: accentColor }}
              />
              <span className="color-picker-label">
                {isArabic ? 'لون الواجهة' : 'Accent color'}
              </span>
              <span className="color-picker-caret">
                {colorMenuOpen ? '▴' : '▾'}
              </span>
            </button>

            {colorMenuOpen && (
              <div className="color-picker-panel">
                <div className="color-swatches">
                  {ACCENT_PRESETS.map((preset) => (
                    <button
                      key={preset.hex}
                      type="button"
                      className={`swatch-btn ${
                        accentColor.toLowerCase() === preset.hex
                          ? 'active'
                          : ''
                      }`}
                      style={{ background: preset.hex }}
                      title={isArabic ? preset.ar : preset.en}
                      aria-label={isArabic ? preset.ar : preset.en}
                      onClick={() => {
                        setAccentColor(preset.hex)
                        setColorMenuOpen(false)
                      }}
                    />
                  ))}
                </div>

                <label className="custom-color-row">
                  <span>
                    {isArabic ? 'لون مخصص' : 'Custom color'}
                  </span>
                  <input
                    type="color"
                    value={accentColor}
                    onChange={(event) =>
                      setAccentColor(event.target.value)
                    }
                  />
                </label>
              </div>
            )}
          </div>

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
        <Dashboard language={language} />
      </main>
    </div>
  )
}

export default App
