import { useEffect, useMemo, useState } from 'react'

import { getDevices, getPositions, type Device } from '../api/trackingApi'
import { DeviceTable } from '../components/DeviceTable'
import { StatCard } from '../components/StatCard'
import type { Language } from '../App'

type Position = {
  deviceId: string
  latitude: number
  longitude: number
  speed: number
  course: number
  valid: boolean
  deviceTime: string
  serverTime: string
}

interface DashboardProps {
  language: Language
}

function Dashboard({ language }: DashboardProps) {
  const isArabic = language === 'ar'

  const [devices, setDevices] = useState<Device[]>([])
  const [positions, setPositions] = useState<Position[]>([])
  const [loading, setLoading] = useState(true)
  const [lastUpdate, setLastUpdate] = useState(new Date())

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

  const latestPositions = positions.slice(0, 8)

  const systemItems = [
    {
      code: 'API',
      title: 'Tracking API',
      subtitle: isArabic ? 'الخدمة الأساسية' : 'Core service',
      status: isArabic ? 'متصل' : 'Online',
    },
    {
      code: 'TCP',
      title: 'GT06 Server',
      subtitle: 'Port 5001',
      status: isArabic ? 'متصل' : 'Online',
    },
    {
      code: 'DB',
      title: 'Database',
      subtitle: 'SQLite',
      status: isArabic ? 'متصل' : 'Online',
    },
    {
      code: 'PLG',
      title: 'GT06 Plugin',
      subtitle: isArabic ? 'البروتوكول محمّل' : 'Protocol loaded',
      status: isArabic ? 'نشط' : 'Active',
    },
  ]

  const locale = isArabic ? 'ar-SA' : 'en-US'

  return (
    <main className="dashboard-page">

      <section className="dashboard-hero">
        <div className="dashboard-hero-content">

          <div className="dashboard-hero-kicker">
            <span className="hero-live-dot" />
            TRACKING PLATFORM
          </div>

          <h1>
            {isArabic ? 'لوحة التحكم' : 'Dashboard'}
          </h1>

          <p>
            {isArabic
              ? 'مركز المراقبة الرئيسي لأسطولك — تابع المركبات والاتصالات وبيانات التتبع لحظة بلحظة.'
              : 'Your fleet command center — monitor vehicles, connections and tracking data in real time.'}
          </p>

          <div className="hero-meta">

            <span className="hero-meta-item">
              <span className="meta-dot green" />

              {isArabic
                ? 'النظام يعمل'
                : 'System operational'}
            </span>

            <span className="hero-divider" />

            <span className="hero-meta-item mono">
              {isArabic ? 'آخر تحديث ' : 'Last update '}

              {lastUpdate.toLocaleTimeString(locale)}
            </span>

          </div>
        </div>

        <div className="hero-orbit">

          <div className="orbit orbit-one" />
          <div className="orbit orbit-two" />
          <div className="orbit orbit-three" />

          <div className="orbit-core">
            <span>GPS</span>
            <strong>{stats.online}</strong>
            <small>
              {isArabic ? 'متصل' : 'ONLINE'}
            </small>
          </div>

          {devices.slice(0, 6).map((device, index) => (
            <span
              key={device.id ?? device.imei ?? index}
              className={`orbit-point orbit-point-${index + 1} ${
                device.isOnline ? 'online' : 'offline'
              }`}
              title={device.imei}
            />
          ))}

        </div>
      </section>

      <section className="stats-grid dashboard-stats">

        <StatCard
          title={isArabic ? 'إجمالي المركبات' : 'Total Vehicles'}
          value={stats.total}
          icon="🚘"
          subtitle={
            isArabic
              ? 'جميع المركبات المسجلة'
              : 'All registered vehicles'
          }
        />

        <StatCard
          title={isArabic ? 'متصلة الآن' : 'Online Now'}
          value={stats.online}
          icon="●"
          subtitle={
            isArabic
              ? 'متصلة بالخادم الآن'
              : 'Currently connected'
          }
        />

        <StatCard
          title={isArabic ? 'تتحرك' : 'Moving'}
          value={stats.moving}
          icon="↗"
          subtitle={
            isArabic
              ? 'مركبات قيد الحركة'
              : 'Vehicles in motion'
          }
        />

        <StatCard
          title={isArabic ? 'متوقفة' : 'Stopped'}
          value={stats.stopped}
          icon="Ⅱ"
          subtitle={
            isArabic
              ? 'متصلة ولكن متوقفة'
              : 'Connected but stopped'
          }
        />

        <StatCard
          title={isArabic ? 'غير متصلة' : 'Offline'}
          value={stats.offline}
          icon="○"
          subtitle={
            isArabic
              ? 'لا يوجد اتصال حالي'
              : 'No current connection'
          }
        />

      </section>

      <section className="dashboard-grid dashboard-main-grid">

        <article className="dashboard-card fleet-overview-card">

          <div className="card-heading">

            <div>
              <span className="card-kicker">
                FLEET OVERVIEW
              </span>

              <h2>
                {isArabic
                  ? 'نظرة عامة على الأسطول'
                  : 'Fleet Overview'}
              </h2>
            </div>

            <button
              type="button"
              className="refresh-button"
              onClick={loadData}
              disabled={loading}
            >
              <span className={loading ? 'refresh-spin' : ''}>
                ↻
              </span>

              {loading
                ? isArabic
                  ? 'جاري التحديث'
                  : 'Refreshing'
                : isArabic
                  ? 'تحديث البيانات'
                  : 'Refresh data'}
            </button>

          </div>

          <div className="fleet-overview">

            <div className="fleet-chart">

              <div className="chart-grid-line line-1" />
              <div className="chart-grid-line line-2" />
              <div className="chart-grid-line line-3" />

              <div className="chart-ring ring-large" />
              <div className="chart-ring ring-small" />

              <div className="chart-center">
                <span>ACTIVE</span>

                <strong>{stats.online}</strong>

                <small>
                  {isArabic
                    ? 'مركبة متصلة'
                    : 'connected vehicles'}
                </small>
              </div>

              <div className="chart-scan" />

            </div>

            <div className="fleet-breakdown">

              <div className="breakdown-title">
                <span>
                  {isArabic
                    ? 'حالة المركبات'
                    : 'Vehicle Status'}
                </span>

                <strong>{stats.total}</strong>
              </div>

              <div className="breakdown-item">

                <div className="breakdown-label">
                  <span className="legend-dot green" />

                  <span>
                    {isArabic ? 'متحركة' : 'Moving'}
                  </span>
                </div>

                <strong>{stats.moving}</strong>

              </div>

              <div className="breakdown-item">

                <div className="breakdown-label">
                  <span className="legend-dot amber" />

                  <span>
                    {isArabic ? 'متوقفة' : 'Stopped'}
                  </span>
                </div>

                <strong>{stats.stopped}</strong>

              </div>

              <div className="breakdown-item">

                <div className="breakdown-label">
                  <span className="legend-dot red" />

                  <span>
                    {isArabic ? 'غير متصلة' : 'Offline'}
                  </span>
                </div>

                <strong>{stats.offline}</strong>

              </div>

              <div className="fleet-progress">

                <div
                  className="fleet-progress-fill"
                  style={{
                    width:
                      stats.total > 0
                        ? `${Math.round(
                            (stats.online / stats.total) * 100,
                          )}%`
                        : '0%',
                  }}
                />

              </div>

              <small className="fleet-progress-label">

                {stats.total > 0
                  ? Math.round(
                      (stats.online / stats.total) * 100,
                    )
                  : 0}
                %{' '}
                {isArabic
                  ? 'من الأسطول متصل'
                  : 'of fleet connected'}

              </small>

            </div>
          </div>
        </article>

        <article className="dashboard-card system-health-card">

          <div className="card-heading">

            <div>

              <span className="card-kicker">
                SYSTEM HEALTH
              </span>

              <h2>
                {isArabic
                  ? 'صحة النظام'
                  : 'System Health'}
              </h2>

            </div>

            <span className="system-health-badge">

              <span className="status-dot online" />

              {isArabic ? 'سليم' : 'Healthy'}

            </span>

          </div>

          <div className="health-list">

            {systemItems.map((item) => (
              <div className="health-item" key={item.code}>

                <span className="health-icon">
                  {item.code}
                </span>

                <div className="health-info">

                  <strong>{item.title}</strong>
                  <span>{item.subtitle}</span>

                </div>

                <b className="health-ok">

                  <span />

                  {item.status}

                </b>

              </div>
            ))}

          </div>

          <div className="system-footer">

            <span>
              {isArabic
                ? 'جميع الخدمات الأساسية تعمل بشكل طبيعي'
                : 'All core services are operating normally'}
            </span>

            <span className="mono">
              100%
            </span>

          </div>

        </article>

      </section>

      <section className="dashboard-card devices-section">

        <div className="card-heading">

          <div>

            <span className="card-kicker">
              VEHICLES
            </span>

            <h2>
              {isArabic ? 'المركبات' : 'Vehicles'}
            </h2>

            <p className="card-description">

              {isArabic
                ? 'قائمة المركبات المسجلة وحالتها الحالية.'
                : 'Registered vehicles and their current status.'}

            </p>

          </div>

          <div className="record-count">
            <strong>{devices.length}</strong>

            <span>
              {isArabic ? 'مركبة' : 'vehicles'}
            </span>
          </div>

        </div>

        <DeviceTable
          devices={devices}
          language={language}
        />

      </section>

      <section className="dashboard-card positions-section">

        <div className="card-heading">

          <div>

            <span className="card-kicker">
              LATEST TELEMETRY
            </span>

            <h2>
              {isArabic
                ? 'آخر بيانات التتبع'
                : 'Latest Telemetry'}
            </h2>

            <p className="card-description">

              {isArabic
                ? 'أحدث المواقع والبيانات المستلمة من أجهزة التتبع.'
                : 'Latest locations and data received from tracking devices.'}

            </p>

          </div>

          <div className="telemetry-live">

            <span className="status-dot online" />

            LIVE

          </div>

        </div>

        {latestPositions.length === 0 ? (

          <div className="empty-state">

            <div className="empty-icon">
              ⌁
            </div>

            <strong>
              {isArabic
                ? 'لا توجد بيانات تتبع حديثة'
                : 'No recent tracking data'}
            </strong>

            <span>
              {isArabic
                ? 'ستظهر بيانات الأجهزة هنا عند وصولها.'
                : 'Device data will appear here when received.'}
            </span>

          </div>

        ) : (

          <div className="telemetry-table-wrapper">

            <table className="telemetry-table">

              <thead>

                <tr>

                  <th>
                    {isArabic ? 'الجهاز' : 'Device'}
                  </th>

                  <th>
                    {isArabic
                      ? 'الإحداثيات'
                      : 'Coordinates'}
                  </th>

                  <th>
                    {isArabic ? 'السرعة' : 'Speed'}
                  </th>

                  <th>
                    {isArabic ? 'الاتجاه' : 'Course'}
                  </th>

                  <th>
                    GPS
                  </th>

                  <th>
                    {isArabic
                      ? 'وقت الخادم'
                      : 'Server Time'}
                  </th>

                </tr>

              </thead>

              <tbody>

                {latestPositions.map((position, index) => (

                  <tr
                    key={`${position.deviceId}-${position.serverTime}-${index}`}
                  >

                    <td className="mono device-id-cell">

                      <span className="device-status-indicator" />

                      {position.deviceId}

                    </td>

                    <td className="mono coordinates-cell">

                      {position.latitude.toFixed(5)}

                      <span>,</span>

                      {position.longitude.toFixed(5)}

                    </td>

                    <td className="speed-cell">

                      <strong>
                        {position.speed}
                      </strong>

                      <span>
                        km/h
                      </span>

                    </td>

                    <td className="course-cell">

                      <span
                        style={{
                          transform: `rotate(${position.course}deg)`,
                        }}
                      >
                        ↑
                      </span>

                      {position.course}°

                    </td>

                    <td>

                      <span
                        className={`gps-badge ${
                          position.valid
                            ? 'valid'
                            : 'invalid'
                        }`}
                      >

                        <span />

                        {position.valid
                          ? isArabic
                            ? 'صالح'
                            : 'Valid'
                          : isArabic
                            ? 'بدون إشارة'
                            : 'No Fix'}

                      </span>

                    </td>

                    <td className="time-cell">

                      {new Date(
                        position.serverTime,
                      ).toLocaleString(locale)}

                    </td>

                  </tr>

                ))}

              </tbody>

            </table>

          </div>
        )}

      </section>

    </main>
  )
}

export default Dashboard
