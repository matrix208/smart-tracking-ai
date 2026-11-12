import { useEffect, useRef, useState } from 'react'
import {
  getLoginMedia,
  uploadLoginMedia,
  setLoginMediaUrl,
  resetLoginMedia,
  type LoginMediaConfig,
  type LoginMediaType,
} from '../api/settingsApi'
import type { Language } from '../App'

interface SettingsProps {
  language: Language
}

function Settings({ language }: SettingsProps) {
  const isArabic = language === 'ar'
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [current, setCurrent] = useState<LoginMediaConfig | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const [mode, setMode] = useState<'upload' | 'url'>('upload')
  const [urlType, setUrlType] = useState<LoginMediaType>('image')
  const [urlValue, setUrlValue] = useState('')
  const [saving, setSaving] = useState(false)

  const load = () => {
    setLoading(true)
    getLoginMedia()
      .then(setCurrent)
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    setSaving(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await uploadLoginMedia(file)
      setCurrent(updated)
      setSuccess(isArabic ? 'تم رفع الملف وحفظه بنجاح' : 'File uploaded and saved successfully')
    } catch (err) {
      setError(err instanceof Error ? err.message : isArabic ? 'فشل الرفع' : 'Upload failed')
    } finally {
      setSaving(false)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  const handleUrlSave = async () => {
    if (!urlValue.trim()) {
      setError(isArabic ? 'أدخل رابطًا صحيحًا' : 'Enter a valid URL')
      return
    }
    setSaving(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await setLoginMediaUrl(urlType, urlValue.trim())
      setCurrent(updated)
      setSuccess(isArabic ? 'تم حفظ الرابط بنجاح' : 'URL saved successfully')
      setUrlValue('')
    } catch (err) {
      setError(err instanceof Error ? err.message : isArabic ? 'فشل الحفظ' : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  const handleReset = async () => {
    setSaving(true)
    setError(null)
    setSuccess(null)
    try {
      await resetLoginMedia()
      setCurrent({ type: null, url: null })
      setSuccess(isArabic ? 'تمت إعادة الضبط الافتراضي' : 'Reset to default')
    } catch (err) {
      setError(err instanceof Error ? err.message : isArabic ? 'فشل إعادة الضبط' : 'Reset failed')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="settings-page">
      <header className="smart-page-header">
        <div>
          <div className="smart-breadcrumb">
            <span>TRACKING PLATFORM</span>
            <span>/</span>
            <strong>{isArabic ? 'الإعدادات' : 'Settings'}</strong>
          </div>
          <h1>{isArabic ? 'إعدادات النظام' : 'System Settings'}</h1>
          <p>
            {isArabic
              ? 'تحكم بمظهر صفحة تسجيل الدخول والوسيطة المعروضة (صورة أو فيديو).'
              : 'Control the login page appearance and the media (image or video) shown there.'}
          </p>
        </div>
      </header>

      <section className="smart-panel settings-panel">
        <div className="smart-panel-header">
          <div>
            <span>{isArabic ? 'صفحة الدخول' : 'LOGIN PAGE'}</span>
            <h2>{isArabic ? 'وسيطة صفحة تسجيل الدخول' : 'Login Illustration Media'}</h2>
          </div>
        </div>

        <div className="settings-media-body">
          {/* ---------- Preview ---------- */}
          <div className="settings-preview">
            <span className="settings-preview-label">
              {isArabic ? 'المعاينة الحالية' : 'Current Preview'}
            </span>

            <div className="settings-preview-box">
              {loading && <span className="settings-preview-placeholder">{isArabic ? 'جاري التحميل...' : 'Loading...'}</span>}

              {!loading && current?.url && current.type === 'image' && (
                <img src={current.url} alt="Login media" />
              )}

              {!loading && current?.url && current.type === 'video' && (
                <video src={current.url} autoPlay loop muted playsInline />
              )}

              {!loading && !current?.url && (
                <span className="settings-preview-placeholder">
                  {isArabic ? 'لا توجد وسيطة مخصصة — يظهر الرسم الافتراضي' : 'No custom media — default illustration is shown'}
                </span>
              )}
            </div>

            {current?.url && (
              <button
                type="button"
                className="plugin-button"
                onClick={handleReset}
                disabled={saving}
              >
                {isArabic ? 'إعادة الضبط الافتراضي' : 'Reset to Default'}
              </button>
            )}
          </div>

          {/* ---------- Editor ---------- */}
          <div className="settings-editor">
            <div className="settings-mode-tabs">
              <button
                type="button"
                className={mode === 'upload' ? 'active' : ''}
                onClick={() => setMode('upload')}
              >
                {isArabic ? 'رفع ملف' : 'Upload File'}
              </button>
              <button
                type="button"
                className={mode === 'url' ? 'active' : ''}
                onClick={() => setMode('url')}
              >
                {isArabic ? 'لصق رابط' : 'Paste URL'}
              </button>
            </div>

            {mode === 'upload' && (
              <div className="settings-upload-block">
                <p>{isArabic ? 'اختر صورة (JPG/PNG) أو فيديو (MP4) من جهازك.' : 'Choose an image (JPG/PNG) or video (MP4) from your device.'}</p>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*,video/*"
                  onChange={handleFileChange}
                  disabled={saving}
                />
              </div>
            )}

            {mode === 'url' && (
              <div className="settings-url-block">
                <div className="settings-type-toggle">
                  <button
                    type="button"
                    className={urlType === 'image' ? 'active' : ''}
                    onClick={() => setUrlType('image')}
                  >
                    {isArabic ? 'صورة' : 'Image'}
                  </button>
                  <button
                    type="button"
                    className={urlType === 'video' ? 'active' : ''}
                    onClick={() => setUrlType('video')}
                  >
                    {isArabic ? 'فيديو' : 'Video'}
                  </button>
                </div>

                <input
                  type="text"
                  className="settings-url-input"
                  placeholder={isArabic ? 'https://example.com/media.jpg' : 'https://example.com/media.jpg'}
                  value={urlValue}
                  onChange={(e) => setUrlValue(e.target.value)}
                  disabled={saving}
                />

                <button
                  type="button"
                  className="smart-dark-button"
                  onClick={handleUrlSave}
                  disabled={saving}
                >
                  {saving ? (isArabic ? 'جاري الحفظ...' : 'Saving...') : (isArabic ? 'حفظ الرابط' : 'Save URL')}
                </button>
              </div>
            )}

            {error && <div className="settings-alert error">{error}</div>}
            {success && <div className="settings-alert success">{success}</div>}
          </div>
        </div>
      </section>
    </div>
  )
}

export default Settings
