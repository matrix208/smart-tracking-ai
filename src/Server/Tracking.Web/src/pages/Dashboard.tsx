import { useEffect, useMemo, useState } from 'react'

import {
  getDevices,
  getPositions,
  type Device,
  type Position,
} from '../api/trackingApi'

import type { Language } from '../App'

interface DashboardProps {
  language: Language
}

function Dashboard({ language }: DashboardProps) {
  const isArabic = language === 'ar'
  const locale = isArabic ? 'ar-SA' : 'en-US'

  const [devices, setDevices] = useState<Device[]>([])
  const [positions, setPositions] = useState<Position[]>([])
  const [loading, setLoading] = useState(true)
  const [lastUpdate, setLastUpdate] = useState(new Date())
  const [activeTab, setActiveTab] = useState<'overview' | 'vehicles'>(
    'overview',
  )

  const loadData = async () => {
    try {
      const [deviceData, positionData] = await Promise.all([
        getDevices(),
        getPositions(),
      ])

      setDevices(deviceData ?? [])
      setPositions(positionData ?? [])
      setLastUpdate(new Date())
    } catch (error) {
      console.error('Dashboard data loading failed:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()

    const timer = window.setInterval(loadData, 15000)

    return () => window.clearInterval(timer)
  }, [])

  const stats = useMemo(() => {
    const online = devices.filter(
      (device) => device.isOnline === true,
    ).length

    const offline = devices.length - online

    const moving = devices.filter(
      (device) =>
        device.isOnline === true &&
        (device.lastSpeed ?? 0) > 0,
    ).length

    const stopped = devices.filter(
      (device) =>
        device.isOnline === true &&
        (device.lastSpeed ?? 0) === 0,
    ).length

    return {
      total: devices.length,
      online,
      offline,
      moving,
      stopped,
    }
  }, [devices])

  const latestPositions = positions.slice(0, 6)

  const formatDate = (value?: string | null) => {
    if (!value) return '—'

    const date = new Date(value)

    if (Number.isNaN(date.getTime())) return '—'

    return date.toLocaleString(locale)
  }

  const formatTime = (value?: string | null) => {
    if (!value) return '—'

    const date = new Date(value)

    if (Number.isNaN(date.getTime())) return '—'

    return date.toLocaleTimeString(locale)
  }

  return (
    <div className="smart-dashboard">
      {/* ================= PAGE HEADER ================= */}

      <header className="smart-page-header">
        <div>
          <div className="smart-breadcrumb">
            <span>TRACKING PLATFORM</span>
            <span>/</span>
            <strong>
              {isArabic ? 'لوحة التحكم' : 'Dashboard'}
            </strong>
          </div>

          <h1>
            {isArabic ? 'لوحة التحكم' : 'Dashboard'}
          </h1>

          <p>
            {isArabic
              ? 'مراقبة الأسطول وحالة الأجهزة وبيانات التتبع في الوقت الحقيقي.'
              : 'Monitor your fleet, devices and tracking data in real time.'}
          </p>
        </div>

        <div className="smart-header-actions">
          <div className="smart-server-indicator">
            <span />
            <div>
              <strong>GT06 Server</strong>
              <small>Port 5001</small>
            </div>
          </div>

          <button
            type="button"
            className="smart-refresh-button"
            onClick={loadData}
            disabled={loading}
          >
            <span className={loading ? 'smart-refresh-spin' : ''}>
              ↻
            </span>

            {loading
              ? isArabic
                ? 'جاري التحديث'
                : 'Refreshing'
              : isArabic
                ? 'تحديث'
                : 'Refresh'}
          </button>
        </div>
      </header>

      {/* ================= STATS ================= */}

      <section className="smart-dashboard-stats">
        <article className="smart-stat">
          <div className="smart-stat-icon total">
            🚗
          </div>

          <div>
            <span>
              {isArabic ? 'إجمالي المركبات' : 'Total Vehicles'}
            </span>

            <strong>{stats.total}</strong>

            <small>
              {isArabic
                ? 'جميع المركبات المسجلة'
                : 'All registered vehicles'}
            </small>
          </div>
        </article>

        <article className="smart-stat">
          <div className="smart-stat-icon online">
            ●
          </div>

          <div>
            <span>
              {isArabic ? 'متصلة الآن' : 'Online Now'}
            </span>

            <strong>{stats.online}</strong>

            <small>
              {isArabic
                ? 'متصلة بالخادم'
                : 'Currently connected'}
            </small>
          </div>
        </article>

        <article className="smart-stat">
          <div className="smart-stat-icon moving">
            ↗
          </div>

          <div>
            <span>
              {isArabic ? 'تتحرك' : 'Moving'}
            </span>

            <strong>{stats.moving}</strong>

            <small>
              {isArabic
                ? 'مركبات قيد الحركة'
                : 'Vehicles in motion'}
            </small>
          </div>
        </article>

        <article className="smart-stat">
          <div className="smart-stat-icon stopped">
            Ⅱ
          </div>

          <div>
            <span>
              {isArabic ? 'متوقفة' : 'Stopped'}
            </span>

            <strong>{stats.stopped}</strong>

            <small>
              {isArabic
                ? 'متصلة ولكن متوقفة'
                : 'Connected but stopped'}
            </small>
          </div>
        </article>

        <article className="smart-stat">
          <div className="smart-stat-icon offline">
            ○
          </div>

          <div>
            <span>
              {isArabic ? 'غير متصلة' : 'Offline'}
            </span>

            <strong>{stats.offline}</strong>

            <small>
              {isArabic
                ? 'لا يوجد اتصال حالي'
                : 'No current connection'}
            </small>
          </div>
        </article>
      </section>

      {/* ================= TABS ================= */}

      <div className="smart-dashboard-tabs">
        <button
          type="button"
          className={activeTab === 'overview' ? 'active' : ''}
          onClick={() => setActiveTab('overview')}
        >
          {isArabic ? 'نظرة عامة' : 'Overview'}
        </button>

        <button
          type="button"
          className={activeTab === 'vehicles' ? 'active' : ''}
          onClick={() => setActiveTab('vehicles')}
        >
          {isArabic ? 'المركبات' : 'Vehicles'}
        </button>

        <span className="smart-last-update">
          {isArabic ? 'آخر تحديث: ' : 'Last update: '}
          {lastUpdate.toLocaleTimeString(locale)}
        </span>
      </div>

      {/* ================= OVERVIEW ================= */}

      {activeTab === 'overview' && (
        <>
          <section className="smart-dashboard-grid">
            {/* Fleet */}

            <article className="smart-panel">
              <div className="smart-panel-header">
                <div>
                  <span>FLEET STATUS</span>

                  <h2>
                    {isArabic
                      ? 'حالة الأسطول'
                      : 'Fleet Status'}
                  </h2>
                </div>

                <span className="smart-live-badge">
                  <i />
                  {isArabic ? 'مباشر' : 'LIVE'}
                </span>
              </div>

              <div className="smart-fleet-content">
                <div
                  className="smart-fleet-circle"
                  style={{
                    '--fleet-progress':
                      stats.total > 0
                        ? `${(stats.online / stats.total) * 100}%`
                        : '0%',
                  } as React.CSSProperties}
                >
                  <div>
                    <strong>{stats.online}</strong>

                    <span>
                      {isArabic ? 'متصلة' : 'ONLINE'}
                    </span>
                  </div>
                </div>

                <div className="smart-status-list">
                  <div>
                    <span className="green" />
                    <label>
                      {isArabic ? 'متصلة' : 'Online'}
                    </label>
                    <strong>{stats.online}</strong>
                  </div>

                  <div>
                    <span className="blue" />
                    <label>
                      {isArabic ? 'تتحرك' : 'Moving'}
                    </label>
                    <strong>{stats.moving}</strong>
                  </div>

                  <div>
                    <span className="orange" />
                    <label>
                      {isArabic ? 'متوقفة' : 'Stopped'}
                    </label>
                    <strong>{stats.stopped}</strong>
                  </div>

                  <div>
                    <span className="gray" />
                    <label>
                      {isArabic ? 'غير متصلة' : 'Offline'}
                    </label>
                    <strong>{stats.offline}</strong>
                  </div>
                </div>
              </div>
            </article>

            {/* System */}

            <article className="smart-panel">
              <div className="smart-panel-header">
                <div>
                  <span>SYSTEM</span>

                  <h2>
                    {isArabic
                      ? 'حالة النظام'
                      : 'System Status'}
                  </h2>
                </div>

                <span className="smart-system-ok">
                  ● {isArabic ? 'يعمل' : 'Operational'}
                </span>
              </div>

              <div className="smart-system-list">
                <div>
                  <b>API</b>

                  <section>
                    <strong>Tracking API</strong>
                    <small>
                      {isArabic
                        ? 'الخدمة الأساسية'
                        : 'Core service'}
                    </small>
                  </section>

                  <em>
                    ● {isArabic ? 'متصل' : 'Online'}
                  </em>
                </div>

                <div>
                  <b>TCP</b>

                  <section>
                    <strong>GT06 Server</strong>
                    <small>Port 5001</small>
                  </section>

                  <em>
                    ● {isArabic ? 'متصل' : 'Online'}
                  </em>
                </div>

                <div>
                  <b>DB</b>

                  <section>
                    <strong>Database</strong>
                    <small>SQLite</small>
                  </section>

                  <em>
                    ● {isArabic ? 'متصل' : 'Online'}
                  </em>
                </div>

                <div>
                  <b>PLG</b>

                  <section>
                    <strong>GT06 Plugin</strong>
                    <small>
                      {isArabic
                        ? 'البروتوكول محمّل'
                        : 'Protocol loaded'}
                    </small>
                  </section>

                  <em>
                    ● {isArabic ? 'نشط' : 'Active'}
                  </em>
                </div>
              </div>
            </article>
          </section>
        </>
      )}

      {/* ================= VEHICLES ================= */}

      {activeTab === 'vehicles' && (
        <section className="smart-panel smart-vehicles-panel">
          <div className="smart-panel-header">
            <div>
              <span>VEHICLES</span>

              <h2>
                {isArabic ? 'المركبات' : 'Vehicles'}
              </h2>
            </div>

            <span className="smart-count">
              {devices.length}
            </span>
          </div>

          <VehicleTable
            devices={devices}
            language={language}
            loading={loading}
            formatDate={formatDate}
          />
        </section>
      )}

      {/* ================= VEHICLE OVERVIEW ================= */}

      {activeTab === 'overview' && (
        <section className="smart-panel smart-vehicles-panel">
          <div className="smart-panel-header">
            <div>
              <span>VEHICLES</span>

              <h2>
                {isArabic
                  ? 'المركبات'
                  : 'Vehicles'}
              </h2>
            </div>

            <button
              type="button"
              className="smart-link-button"
              onClick={() => setActiveTab('vehicles')}
            >
              {isArabic
                ? 'عرض الكل'
                : 'View all'}
              →
            </button>
          </div>

          <VehicleTable
            devices={devices.slice(0, 5)}
            language={language}
            loading={loading}
            formatDate={formatDate}
          />
        </section>
      )}

      {/* ================= LIVE POSITIONS ================= */}

      <section className="smart-panel">
        <div className="smart-panel-header">
          <div>
            <span>LIVE TELEMETRY</span>

            <h2>
              {isArabic
                ? 'آخر المواقع'
                : 'Latest Positions'}
            </h2>
          </div>

          <span className="smart-last-update">
            {isArabic ? 'تحديث تلقائي كل 15 ثانية' : 'Auto refresh every 15 seconds'}
          </span>
        </div>

        {latestPositions.length > 0 ? (
          <div className="smart-position-grid">
            {latestPositions.map((position, index) => (
              <article
                className="smart-position-card"
                key={`${position.deviceId}-${position.serverTime}-${index}`}
              >
                <div className="smart-position-top">
                  <div className="smart-position-device">
                    <span>GPS</span>

                    <strong>{position.deviceId}</strong>
                  </div>

                  <span
                    className={
                      position.valid
                        ? 'smart-position-valid'
                        : 'smart-position-invalid'
                    }
                  >
                    {position.valid
                      ? isArabic
                        ? 'صالح'
                        : 'VALID'
                      : isArabic
                        ? 'غير صالح'
                        : 'INVALID'}
                  </span>
                </div>

                <div className="smart-position-coordinates">
                  {position.latitude.toFixed(5)}
                  <span>,</span>
                  {position.longitude.toFixed(5)}
                </div>

                <div className="smart-position-meta">
                  <span>
                    <small>
                      {isArabic ? 'السرعة' : 'Speed'}
                    </small>
                    <strong>
                      {position.speed} km/h
                    </strong>
                  </span>

                  <span>
                    <small>
                      {isArabic ? 'الاتجاه' : 'Course'}
                    </small>
                    <strong>
                      {position.course}°
                    </strong>
                  </span>

                  <span>
                    <small>
                      {isArabic ? 'الوقت' : 'Time'}
                    </small>
                    <strong>
                      {formatTime(position.serverTime)}
                    </strong>
                  </span>
                </div>
              </article>
            ))}
          </div>
        ) : (
          <div className="smart-empty">
            {loading
              ? isArabic
                ? 'جاري تحميل بيانات المواقع...'
                : 'Loading position data...'
              : isArabic
                ? 'لا توجد بيانات مواقع'
                : 'No position data available'}
          </div>
        )}
      </section>

      {/* ================= FOOTER ================= */}

      <footer className="smart-dashboard-footer">
        <span>
          © 2026 Tracking Platform
        </span>

        <span>
          {isArabic
            ? 'نظام تتبع وإدارة الأسطول'
            : 'Fleet tracking and management system'}
        </span>

        <span>
          {isArabic ? 'وقت الخادم' : 'Server time'}:{' '}
          {lastUpdate.toLocaleString(locale)}
        </span>
      </footer>
    </div>
  )
}

interface VehicleTableProps {
  devices: Device[]
  language: Language
  loading: boolean
  formatDate: (value?: string | null) => string
}

function VehicleTable({
  devices,
  language,
  loading,
  formatDate,
}: VehicleTableProps) {
  const isArabic = language === 'ar'

  return (
    <div className="smart-table-wrapper">
      <table className="smart-table">
        <thead>
          <tr>
            <th>
              {isArabic ? 'المركبة' : 'Vehicle'}
            </th>

            <th>IMEI</th>

            <th>
              {isArabic ? 'الحالة' : 'Status'}
            </th>

            <th>
              {isArabic ? 'السرعة' : 'Speed'}
            </th>

            <th>
              {isArabic ? 'الموقع' : 'Location'}
            </th>

            <th>
              {isArabic ? 'آخر تحديث' : 'Last Update'}
            </th>
          </tr>
        </thead>

        <tbody>
          {devices.map((device) => (
            <tr key={device.id ?? device.imei}>
              <td>
                <div className="smart-vehicle-name">
                  <span
                    className={
                      device.isOnline
                        ? 'smart-vehicle-dot online'
                        : 'smart-vehicle-dot'
                    }
                  />

                  <div>
                    <strong>
                      {device.name || device.imei}
                    </strong>

                    <small>
                      {device.protocol?.toUpperCase() || 'GT06'}
                    </small>
                  </div>
                </div>
              </td>

              <td className="smart-mono">
                {device.imei}
              </td>

              <td>
                <span
                  className={
                    device.isOnline
                      ? 'smart-status-badge online'
                      : 'smart-status-badge offline'
                  }
                >
                  {device.isOnline
                    ? isArabic
                      ? 'متصل'
                      : 'Online'
                    : isArabic
                      ? 'غير متصل'
                      : 'Offline'}
                </span>
              </td>

              <td>
                <strong className="smart-speed">
                  {device.lastSpeed ?? 0}
                  <small> km/h</small>
                </strong>
              </td>

              <td className="smart-mono">
                {device.lastLatitude != null &&
                device.lastLongitude != null
                  ? `${device.lastLatitude.toFixed(4)}, ${device.lastLongitude.toFixed(4)}`
                  : '—'}
              </td>

              <td className="smart-date">
                {formatDate(device.lastSeen)}
              </td>
            </tr>
          ))}

          {devices.length === 0 && (
            <tr>
              <td colSpan={6}>
                <div className="smart-empty">
                  {loading
                    ? isArabic
                      ? 'جاري تحميل البيانات...'
                      : 'Loading data...'
                    : isArabic
                      ? 'لا توجد مركبات'
                      : 'No vehicles found'}
                </div>
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

export default Dashboard
