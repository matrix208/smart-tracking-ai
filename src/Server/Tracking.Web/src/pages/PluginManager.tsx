import { useState } from 'react'
import type { Language } from '../App'

interface PluginManagerProps {
  language: Language
}

interface Plugin {
  id: string
  name: string
  version?: string
  description?: string
  icon: string
  active: boolean
}

function PluginManager({ language }: PluginManagerProps) {
  const isArabic = language === 'ar'

  const [repository, setRepository] = useState('plugin.fleetstack.in')
  const [editingRepository, setEditingRepository] = useState(false)
  const [activeTab, setActiveTab] = useState<'installed' | 'available'>(
    'installed',
  )

  const [plugins, setPlugins] = useState<Plugin[]>([
    {
      id: 'gt06',
      name: 'GPS Tracker Plugin',
      version: '2.1.4',
      icon: '🛰️',
      active: true,
    },
    {
      id: 'ai',
      name: 'AI Tracker Plugin',
      version: '3.1.4',
      icon: '📊',
      active: true,
    },
    {
      id: 'analytics',
      name: 'Analytics Dashboard',
      icon: '❗',
      active: true,
    },
    {
      id: 'ruptela',
      name: 'Ruptela',
      version: '4.1.4',
      icon: '🦂',
      active: false,
    },
  ])

  const togglePlugin = (id: string) => {
    setPlugins((current) =>
      current.map((plugin) =>
        plugin.id === id
          ? { ...plugin, active: !plugin.active }
          : plugin,
      ),
    )
  }

  return (
    <div className="smart-page">
      <div className="smart-page-header">
        <h1>{isArabic ? 'إدارة الإضافات' : 'Manage Plugins'}</h1>

        <p>
          {isArabic
            ? 'قم بإدارة الإضافات المثبتة وتحميل إضافات جديدة من مستودع الإضافات.'
            : 'Download new and manage installed plugins.'}
        </p>
      </div>

      <section className="plugin-repository">
        <div className="plugin-repository-row">
          {editingRepository ? (
            <input
              value={repository}
              onChange={(event) => setRepository(event.target.value)}
              className="plugin-repository-input"
              autoFocus
            />
          ) : (
            <div className="plugin-repository-input">
              {repository}
            </div>
          )}

          <button
            type="button"
            className="smart-dark-button"
            onClick={() => setEditingRepository(!editingRepository)}
          >
            {editingRepository
              ? isArabic
                ? 'حفظ'
                : 'Save'
              : isArabic
                ? '✎ تعديل'
                : '✎ Edit'}
          </button>
        </div>

        <div className="plugin-field-label">
          {isArabic ? 'رابط المستودع' : 'Repository URL'}
        </div>

        <div className="plugin-field-hint">
          {isArabic
            ? 'رابط المستودع المستخدم حاليًا للبحث عن الإضافات القابلة للتنزيل.'
            : 'The repository URL currently used for finding downloadable plugins.'}
        </div>
      </section>

      <section className="plugin-manager-panel">
        <div className="plugin-tabs-row">
          <div className="plugin-tabs">
            <span className="plugin-tab-icon">🧩</span>

            <button
              type="button"
              className={
                activeTab === 'installed'
                  ? 'plugin-tab active'
                  : 'plugin-tab'
              }
              onClick={() => setActiveTab('installed')}
            >
              {isArabic ? 'إضافاتي' : 'My Plugins'}
            </button>

            <button
              type="button"
              className={
                activeTab === 'available'
                  ? 'plugin-tab active'
                  : 'plugin-tab'
              }
              onClick={() => setActiveTab('available')}
            >
              {isArabic ? 'الحصول على إضافات' : 'Get More Plugins'}
            </button>
          </div>

          <button
            type="button"
            className="smart-dark-button refresh-plugin-button"
            onClick={() => setPlugins([...plugins])}
          >
            ⟳ {isArabic ? 'تحديث' : 'Refresh'}
          </button>
        </div>

        {activeTab === 'installed' ? (
          <div className="plugin-list">
            {plugins.map((plugin) => (
              <div className="plugin-row" key={plugin.id}>
                <div className="plugin-icon">{plugin.icon}</div>

                <div className="plugin-info">
                  <div className="plugin-name">
                    {plugin.name}
                  </div>

                  <div className="plugin-version">
                    {plugin.version
                      ? `${isArabic ? 'الإصدار' : 'Version'} ${plugin.version} • ${
                          plugin.active
                            ? isArabic
                              ? 'نشط'
                              : 'Active'
                            : isArabic
                              ? 'غير نشط'
                              : 'Inactive'
                        }`
                      : plugin.active
                        ? isArabic
                          ? 'نشط'
                          : 'Active'
                        : isArabic
                          ? 'غير نشط'
                          : 'Inactive'}
                  </div>
                </div>

                <div className="plugin-actions">
                  <button
                    type="button"
                    className="plugin-button"
                  >
                    {isArabic ? 'إعدادات' : 'Configure'}
                  </button>

                  <button
                    type="button"
                    className={
                      plugin.active
                        ? 'plugin-button plugin-disable'
                        : 'plugin-button plugin-enable'
                    }
                    onClick={() => togglePlugin(plugin.id)}
                  >
                    {plugin.active
                      ? isArabic
                        ? 'تعطيل'
                        : 'Disable'
                      : isArabic
                        ? 'تفعيل'
                        : 'Enable'}
                  </button>

                  <button
                    type="button"
                    className="plugin-button plugin-update"
                    disabled
                  >
                    {isArabic ? 'تحديث' : 'Update'}
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="plugin-empty-state">
            <div className="plugin-empty-icon">🧩</div>

            <strong>
              {isArabic
                ? 'الإضافات المتاحة'
                : 'Available Plugins'}
            </strong>

            <p>
              {isArabic
                ? 'سيتم ربط هذا القسم بمستودع الإضافات لاحقًا.'
                : 'This section will be connected to the plugin repository later.'}
            </p>
          </div>
        )}
      </section>

      <footer className="smart-footer">
        <span>
          ← {isArabic ? 'السابق' : 'Previous'}
        </span>

        <span>
          © 2026 Tracking Platform&nbsp; | &nbsp;
          {isArabic ? 'المساعدة' : 'Help'}
          &nbsp; | &nbsp;
          {isArabic ? 'مسجل الدخول كـ: مدير النظام' : 'Logged in as: System Admin'}
        </span>
      </footer>
    </div>
  )
}

export default PluginManager
