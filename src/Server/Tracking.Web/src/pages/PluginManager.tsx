import { useCallback, useEffect, useState } from 'react'

import {
  getPluginRepository,
  getPlugins,
  installPlugin,
  setPluginEnabled,
} from '../api/trackingApi'
import type { Language } from '../App'

interface PluginManagerProps {
  language: Language
}

interface Plugin {
  id: string
  name: string
  description?: string
  version?: string
  author?: string
  manufacturer?: string
  assembly?: string
  sdkVersion?: string
  defaultPort?: number
  supportsTcp?: boolean
  supportsUdp?: boolean
  models?: string[]
  capabilities?: string[]
  enabled: boolean
}

function PluginManager({ language }: PluginManagerProps) {
  const isArabic = language === 'ar'

  const [repository, setRepository] = useState('plugin.fleetstack.in')
  const [editingRepository, setEditingRepository] = useState(false)
  const [activeTab, setActiveTab] = useState<'installed' | 'available'>('installed')

  const [plugins, setPlugins] = useState<Plugin[]>([])
  const [repositoryPlugins, setRepositoryPlugins] = useState<
    import('../api/trackingApi').PluginRepositoryPackage[]
  >([])
  const [repositoryLoading, setRepositoryLoading] = useState(false)
  const [installingPluginId, setInstallingPluginId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)



  const loadPlugins = useCallback(async () => {
    setError(null)

    try {
      const data = await getPlugins()
      setPlugins(data)
    } catch (err) {
      console.error('Plugin Manager:', err)

      setError(
        err instanceof Error
          ? err.message
          : isArabic
            ? 'حدث خطأ أثناء تحميل الإضافات.'
            : 'Failed to load plugins.',
      )
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [isArabic])

  const loadRepository = useCallback(async () => {
    setRepositoryLoading(true)

    try {
      const data = await getPluginRepository()
      setRepositoryPlugins(data)
    } catch (err) {
      console.error('Plugin Repository:', err)

      setRepositoryPlugins([])
      setError(
        err instanceof Error
          ? err.message
          : isArabic
            ? 'تعذر تحميل مستودع الإضافات.'
            : 'Failed to load plugin repository.',
      )
    } finally {
      setRepositoryLoading(false)
    }
  }, [isArabic])

  useEffect(() => {
    void loadPlugins()
  }, [loadPlugins])

  useEffect(() => {
    if (activeTab === 'available') {
      void loadRepository()
    }
  }, [activeTab, loadRepository])

  const handleRefresh = async () => {
    setRefreshing(true)
    await loadPlugins()
  }

  const handleRepositorySave = () => {
    setEditingRepository(false)
  }

  const handlePluginToggle = async (plugin: Plugin) => {
    try {
      setError(null)

      await setPluginEnabled(plugin.id, !plugin.enabled)

      const latest = await getPlugins()
      setPlugins(latest)
    } catch (err) {
      console.error('Plugin Manager:', err)

      setError(
        err instanceof Error
          ? err.message
          : isArabic
            ? 'تعذر تغيير حالة الإضافة.'
            : 'Failed to change plugin state.',
      )
    }
  }

  const handlePluginInstall = async (
    plugin: import('../api/trackingApi').PluginRepositoryPackage,
  ) => {
    try {
      setError(null)
      setInstallingPluginId(plugin.packageId)

      await installPlugin(plugin.packageId)

      await loadPlugins()

      setActiveTab('installed')
    } catch (err) {
      console.error('Plugin Manager:', err)

      setError(
        err instanceof Error
          ? err.message
          : isArabic
            ? 'تعذر تثبيت الإضافة.'
            : 'Failed to install plugin.',
      )
    } finally {
      setInstallingPluginId(null)
    }
  }

  return (
    <div className={`smart-page plugin-manager-page ${isArabic ? 'rtl' : 'ltr'}`}>
      <div className="plugin-manager-content">

        <div className="smart-page-header">
          <h1>{isArabic ? 'إدارة الإضافات' : 'Manage Plugins'}</h1>

          <p>
            {isArabic
              ? 'قم بإدارة الإضافات المثبتة وتحميل إضافات جديدة من مستودع الإضافات.'
              : 'Download new and manage installed plugins. Plugins that are currently installed on the server are found under the tab "My Plugins". You can download more plugins from the tab "Get More Plugins".'}
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
              className="smart-dark-button plugin-edit-button"
              onClick={() => {
                if (editingRepository) {
                  handleRepositorySave()
                } else {
                  setEditingRepository(true)
                }
              }}
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
          </div>

          <div className="plugin-refresh-row">
            <button
              type="button"
              className="smart-dark-button refresh-plugin-button"
              onClick={() => void handleRefresh()}
              disabled={refreshing}
            >
              <span className={refreshing ? 'plugin-refresh-spin' : ''}>
                ⟳
              </span>

              {refreshing
                ? isArabic
                  ? 'جاري التحديث...'
                  : 'Refreshing...'
                : isArabic
                  ? 'تحديث'
                  : 'Refresh'}
            </button>
          </div>

          {activeTab === 'installed' && (
            <>
              {loading && (
                <div className="plugin-state">
                  <div className="plugin-state-icon">⟳</div>
                  <strong>
                    {isArabic
                      ? 'جاري تحميل الإضافات...'
                      : 'Loading plugins...'}
                  </strong>
                </div>
              )}

              {!loading && error && (
                <div className="plugin-state plugin-error-state">
                  <div className="plugin-state-icon">!</div>

                  <strong>
                    {isArabic
                      ? 'تعذر تحميل الإضافات'
                      : 'Unable to load plugins'}
                  </strong>

                  <p>{error}</p>

                  <button
                    type="button"
                    className="plugin-retry-button"
                    onClick={() => void handleRefresh()}
                  >
                    {isArabic ? 'إعادة المحاولة' : 'Retry'}
                  </button>
                </div>
              )}

              {!loading && !error && plugins.length === 0 && (
                <div className="plugin-state">
                  <div className="plugin-state-icon">🧩</div>

                  <strong>
                    {isArabic
                      ? 'لا توجد إضافات مثبتة'
                      : 'No installed plugins'}
                  </strong>

                  <p>
                    {isArabic
                      ? 'لم يتم تحميل أي إضافات من Plugin Runtime.'
                      : 'No plugins were loaded by the Plugin Runtime.'}
                  </p>
                </div>
              )}

              {!loading && !error && plugins.length > 0 && (
                <div className="plugin-grid">
                  {plugins.map((plugin) => (
                    <article
                      className={`plugin-card ${
                        plugin.enabled
                          ? 'plugin-card-enabled'
                          : 'plugin-card-disabled'
                      }`}
                      key={plugin.id}
                    >
                      <div className="plugin-card-header">
                        <div className="plugin-card-identity">
                          <div className="plugin-card-icon">
                            {plugin.id === 'gt06' ? '🛰️' : '🧩'}
                          </div>

                          <div>
                            <h3 className="plugin-card-name">
                              {plugin.name}
                            </h3>

                            <div className="plugin-card-version">
                              {plugin.version
                                ? `v${plugin.version}`
                                : plugin.id}
                            </div>
                          </div>
                        </div>

                        <div
                          className={`plugin-card-status ${
                            plugin.enabled
                              ? 'plugin-card-status-enabled'
                              : 'plugin-card-status-disabled'
                          }`}
                        >
                          <span className="plugin-status-dot" />
                          {plugin.enabled
                            ? isArabic
                              ? 'نشط'
                              : 'Enabled'
                            : isArabic
                              ? 'معطل'
                              : 'Disabled'}
                        </div>
                      </div>

                      <p className="plugin-card-description">
                        {plugin.description ||
                          (isArabic
                            ? 'لا يوجد وصف لهذه الإضافة.'
                            : 'No description available.')}
                      </p>

                      <div className="plugin-card-meta">
                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المعرّف' : 'ID'}
                          </span>
                          <strong>{plugin.id}</strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المنفذ' : 'Port'}
                          </span>
                          <strong>
                            {plugin.defaultPort || '—'}
                          </strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المصنّع' : 'Manufacturer'}
                          </span>
                          <strong>
                            {plugin.manufacturer || '—'}
                          </strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'الإصدار' : 'SDK'}
                          </span>
                          <strong>
                            {plugin.sdkVersion || '—'}
                          </strong>
                        </div>
                      </div>

                      {plugin.capabilities &&
                        plugin.capabilities.length > 0 && (
                          <div className="plugin-card-section">
                            <span className="plugin-card-section-title">
                              {isArabic ? 'القدرات' : 'Capabilities'}
                            </span>

                            <div className="plugin-chip-list">
                              {plugin.capabilities.map((capability) => (
                                <span
                                  className="plugin-chip"
                                  key={capability}
                                >
                                  {capability}
                                </span>
                              ))}
                            </div>
                          </div>
                        )}

                      {plugin.models &&
                        plugin.models.length > 0 && (
                          <div className="plugin-card-section">
                            <span className="plugin-card-section-title">
                              {isArabic ? 'الموديلات' : 'Models'}
                            </span>

                            <div className="plugin-chip-list">
                              {plugin.models.map((model) => (
                                <span
                                  className="plugin-chip"
                                  key={model}
                                >
                                  {model}
                                </span>
                              ))}
                            </div>
                          </div>
                        )}

                      <div className="plugin-card-footer">
                        <button
                          type="button"
                          className={
                            plugin.enabled
                              ? 'plugin-button plugin-disable'
                              : 'plugin-button plugin-enable'
                          }
                          onClick={() =>
                            void handlePluginToggle(plugin)
                          }
                        >
                          {plugin.enabled
                            ? isArabic
                              ? 'تعطيل'
                              : 'Disable'
                            : isArabic
                              ? 'تفعيل'
                              : 'Enable'}
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </>
          )}

          {activeTab === 'available' && (
            <>
              {repositoryLoading && (
                <div className="plugin-state">
                  <div className="plugin-state-icon">⟳</div>
                  <strong>
                    {isArabic
                      ? 'جاري تحميل مستودع الإضافات...'
                      : 'Loading plugin repository...'}
                  </strong>
                </div>
              )}

              {!repositoryLoading && repositoryPlugins.length === 0 && (
                <div className="plugin-state">
                  <div className="plugin-state-icon">🧩</div>
                  <strong>
                    {isArabic
                      ? 'لا توجد إضافات متاحة'
                      : 'No plugins available'}
                  </strong>
                  <p>
                    {isArabic
                      ? 'لم يتم العثور على إضافات في المستودع.'
                      : 'No plugins were found in the repository.'}
                  </p>
                </div>
              )}

              {!repositoryLoading && repositoryPlugins.length > 0 && (
                <div className="plugin-grid">
                  {repositoryPlugins.map((plugin) => (
                    <article
                      className="plugin-card"
                      key={plugin.packageId}
                    >
                      <div className="plugin-card-header">
                        <div className="plugin-card-identity">
                          <div className="plugin-card-icon">
                            {plugin.packageId === 'gt06' ? '🛰️' : '🧩'}
                          </div>

                          <div>
                            <h3 className="plugin-card-name">
                              {plugin.displayName}
                            </h3>

                            <div className="plugin-card-version">
                              v{plugin.version}
                            </div>
                          </div>
                        </div>

                        <div className="plugin-card-status">
                          <span className="plugin-status-dot" />
                          {isArabic ? 'متاح' : 'Available'}
                        </div>
                      </div>

                      <p className="plugin-card-description">
                        {plugin.description ||
                          (isArabic
                            ? 'لا يوجد وصف لهذه الإضافة.'
                            : 'No description available.')}
                      </p>

                      <div className="plugin-card-meta">
                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المعرّف' : 'ID'}
                          </span>
                          <strong>{plugin.packageId}</strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المنفذ' : 'Port'}
                          </span>
                          <strong>{plugin.defaultPort || '—'}</strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            {isArabic ? 'المصنّع' : 'Manufacturer'}
                          </span>
                          <strong>{plugin.manufacturer || '—'}</strong>
                        </div>

                        <div className="plugin-meta-item">
                          <span className="plugin-meta-label">
                            SDK
                          </span>
                          <strong>{plugin.sdkVersion || '—'}</strong>
                        </div>
                      </div>

                      <div className="plugin-card-section">
                        <span className="plugin-card-section-title">
                          {isArabic ? 'الاتصال' : 'Transport'}
                        </span>

                        <div className="plugin-chip-list">
                          {plugin.supportsTcp && (
                            <span className="plugin-chip">TCP</span>
                          )}

                          {plugin.supportsUdp && (
                            <span className="plugin-chip">UDP</span>
                          )}
                        </div>
                      </div>

                      {plugin.permissions.length > 0 && (
                        <div className="plugin-card-section">
                          <span className="plugin-card-section-title">
                            {isArabic ? 'الصلاحيات' : 'Permissions'}
                          </span>

                          <div className="plugin-chip-list">
                            {plugin.permissions.map((permission) => (
                              <span
                                className="plugin-chip"
                                key={permission}
                              >
                                {permission}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      {plugin.dependencies.length > 0 && (
                        <div className="plugin-card-section">
                          <span className="plugin-card-section-title">
                            {isArabic ? 'الاعتماديات' : 'Dependencies'}
                          </span>

                          <div className="plugin-chip-list">
                            {plugin.dependencies.map((dependency) => (
                              <span
                                className="plugin-chip"
                                key={dependency}
                              >
                                {dependency}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="plugin-card-footer">
                        <button
                          type="button"
                          className="plugin-button plugin-enable"
                          onClick={() => void handlePluginInstall(plugin)}
                          disabled={installingPluginId === plugin.packageId}
                        >
                          {installingPluginId === plugin.packageId
                            ? isArabic
                              ? 'جاري التثبيت...'
                              : 'Installing...'
                            : isArabic
                              ? 'تثبيت'
                              : 'Install'}
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </>
          )}

        </section>

        <footer className="smart-footer plugin-manager-footer">

          <span className="plugin-previous">
            ← {isArabic ? 'السابق' : 'Previous'}
          </span>

          <span>
            © 2026 Tracking Platform&nbsp; | &nbsp;
            {isArabic ? 'المساعدة' : 'Help'}
            &nbsp; | &nbsp;
            {isArabic
              ? 'مسجل الدخول كـ: مدير النظام'
              : 'Logged in as: System Admin'}
          </span>

        </footer>

      </div>
    </div>
  )
}

export default PluginManager
