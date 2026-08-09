import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  getDevices,
  getPositions,
  getDeviceStates,
  type Device,
  type Position,
  type DeviceState,
} from '../api/trackingApi'
import { StatCard } from '../components/StatCard'
import { TrackingMap } from '../components/map/TrackingMap'
import type { Language } from '../App'

function formatTime(
  value?: string | null,
  language: Language = 'ar',
) {
  if (!value) return '—'

  return new Date(value).toLocaleTimeString(
    language === 'ar' ? 'ar-SA' : 'en-US',
    {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    },
  )
}

function formatCoordinate(value?: number | null) {
  if (value == null) return '—'
  return value.toFixed(6)
}

function getDeviceStatus(
  device: Device,
  state?: DeviceState,
) {
  if (!device.isOnline) return 'offline'

  const speed =
    state?.speed ??
    device.lastSpeed ??
    0

  if (speed > 3) return 'moving'

  return 'idle'
}

export function Dashboard({
  language,
}: {
  language: Language
}) {
  const isArabic = language === 'ar'

  const [devices, setDevices] = useState<Device[]>([])
  const [, setPositions] = useState<Position[]>([])
  const [states, setStates] = useState<DeviceState[]>([])
  const [selectedImei, setSelectedImei] =
    useState<string | null>(null)

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [lastRefresh, setLastRefresh] =
    useState<Date | null>(null)

  const loadData = useCallback(async () => {
    try {
      setError(null)

      const [
        deviceData,
        positionData,
        stateData,
      ] = await Promise.all([
        getDevices(),
        getPositions(),
        getDeviceStates(),
      ])

      setDevices(deviceData)
      setPositions(positionData)
      setStates(stateData)
      setLastRefresh(new Date())

      setSelectedImei((current) => {
        if (
          current &&
          deviceData.some(
            (device) => device.imei === current,
          )
        ) {
          return current
        }

        return deviceData[0]?.imei ?? null
      })
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : isArabic
            ? 'تعذر الاتصال بالخادم'
            : 'Unable to connect to server',
      )
    } finally {
      setLoading(false)
    }
  }, [isArabic])

  useEffect(() => {
    void loadData()

    const timer = window.setInterval(
      () => void loadData(),
      10_000,
    )

    return () => window.clearInterval(timer)
  }, [loadData])

  const onlineDevices = useMemo(
    () =>
      devices.filter(
        (device) => device.isOnline,
      ),
    [devices],
  )

  const offlineDevices = useMemo(
    () =>
      devices.filter(
        (device) => !device.isOnline,
      ),
    [devices],
  )

  const movingDevices = useMemo(
    () =>
      devices.filter((device) => {
        const state = states.find(
          (item) => item.deviceId === device.imei,
        )

        return getDeviceStatus(device, state) === 'moving'
      }),
    [devices, states],
  )

  const idleDevices = useMemo(
    () =>
      devices.filter((device) => {
        const state = states.find(
          (item) => item.deviceId === device.imei,
        )

        return getDeviceStatus(device, state) === 'idle'
      }),
    [devices, states],
  )

  const selectedDevice = useMemo(
    () =>
      devices.find(
        (device) =>
          device.imei === selectedImei,
      ) ?? null,
    [devices, selectedImei],
  )

  const selectedState = useMemo(
    () =>
      states.find(
        (state) =>
          state.deviceId === selectedImei,
      ) ?? null,
    [states, selectedImei],
  )

  return (
    <div className="dashboard">
      <div className="topbar">
        <div>
          <div className="page-title">
            {isArabic
              ? 'أسطول اليوم'
              : "Today's Fleet"}

            <span className="accent">
              {' '}
              — {onlineDevices.length}{' '}
              {isArabic
                ? 'مركبة متصلة'
                : 'vehicles online'}
            </span>
          </div>

          <div className="page-sub">
            {isArabic
              ? 'آخر تحديث للمواقع: '
              : 'Last location update: '}

            <span className="num">
              {lastRefresh
                ? formatTime(
                    lastRefresh.toISOString(),
                    language,
                  )
                : '—'}
            </span>

            {' · '}

            {isArabic
              ? 'البيانات من Tracking API'
              : 'Live data from Tracking API'}
          </div>
        </div>

        <div className="topbar-actions">
          <div className="api-indicator">
            <span className="status-dot online" />
            <span>
              {isArabic
                ? 'متصل بالخادم'
                : 'API Connected'}
            </span>
          </div>

          <button
            className="btn-primary"
            onClick={() => void loadData()}
          >
            ↻
            {isArabic ? ' تحديث' : ' Refresh'}
          </button>
        </div>
      </div>

      {error && (
        <div className="error-banner">
          <span>⚠</span>

          <div>
            <strong>
              {isArabic
                ? 'تعذر تحميل البيانات'
                : 'Unable to load data'}
            </strong>

            <div>{error}</div>
          </div>

          <button
            onClick={() => void loadData()}
          >
            {isArabic
              ? 'إعادة المحاولة'
              : 'Retry'}
          </button>
        </div>
      )}

      <div className="stats-row">
        <StatCard
          title={
            isArabic
              ? 'إجمالي الأجهزة'
              : 'Total Devices'
          }
          value={devices.length}
          subtitle={
            isArabic
              ? 'الأجهزة المسجلة'
              : 'Registered devices'
          }
          icon="▣"
        />

        <StatCard
          title={
            isArabic
              ? 'متصل الآن'
              : 'Online Now'
          }
          value={onlineDevices.length}
          subtitle={
            isArabic
              ? 'اتصال نشط'
              : 'Active connections'
          }
          icon="●"
        />

        <StatCard
          title={
            isArabic
              ? 'تتحرك'
              : 'Moving'
          }
          value={movingDevices.length}
          subtitle={
            isArabic
              ? 'أجهزة سرعتها أكبر من 3 كم/س'
              : 'Speed above 3 km/h'
          }
          icon="↗"
        />

        <StatCard
          title={
            isArabic
              ? 'غير متصل'
              : 'Offline'
          }
          value={offlineDevices.length}
          subtitle={
            isArabic
              ? 'أجهزة غير متصلة'
              : 'Disconnected devices'
          }
          icon="○"
        />
      </div>

      <div className="grid-main">
        <div className="panel map-panel">
          <div className="panel-head">
            <div>
              <div className="panel-title">
                {isArabic
                  ? 'خريطة التتبع اللحظي'
                  : 'Live Tracking Map'}

                <span>
                  {isArabic
                    ? 'تحديث كل 10 ثوانٍ'
                    : 'Updates every 10 seconds'}
                </span>
              </div>
            </div>

            <div className="live-badge">
              <span className="status-dot online" />
              LIVE
            </div>
          </div>

          <div className="map-container">
            <TrackingMap
              devices={devices}
              selectedImei={selectedImei}
              onSelect={setSelectedImei}
            />
          </div>
        </div>

        <div className="panel">
          <div className="panel-head">
            <div className="panel-title">
              {isArabic
                ? 'حالة الأجهزة'
                : 'Device Status'}

              <span>
                {isArabic
                  ? `${devices.length} جهاز`
                  : `${devices.length} devices`}
              </span>
            </div>
          </div>

          <div className="device-summary">
            <div className="summary-item">
              <span className="summary-dot moving" />
              <span>
                {isArabic
                  ? 'تتحرك'
                  : 'Moving'}
              </span>
              <strong>
                {movingDevices.length}
              </strong>
            </div>

            <div className="summary-item">
              <span className="summary-dot idle" />
              <span>
                {isArabic
                  ? 'متوقفة'
                  : 'Idle'}
              </span>
              <strong>
                {idleDevices.length}
              </strong>
            </div>

            <div className="summary-item">
              <span className="summary-dot offline" />
              <span>
                {isArabic
                  ? 'غير متصلة'
                  : 'Offline'}
              </span>
              <strong>
                {offlineDevices.length}
              </strong>
            </div>
          </div>

          <div className="selected-device-box">
            <div className="panel-title">
              {isArabic
                ? 'الجهاز المحدد'
                : 'Selected Device'}
            </div>

            {selectedDevice ? (
              <>
                <div className="selected-imei">
                  {selectedDevice.imei}
                </div>

                <div
                  className={`selected-status ${
                    selectedDevice.isOnline
                      ? 'online'
                      : 'offline'
                  }`}
                >
                  <span className="status-dot" />

                  {selectedDevice.isOnline
                    ? isArabic
                      ? 'متصل'
                      : 'ONLINE'
                    : isArabic
                      ? 'غير متصل'
                      : 'OFFLINE'}
                </div>

                <div className="selected-grid">
                  <div>
                    <small>
                      {isArabic
                        ? 'السرعة'
                        : 'Speed'}
                    </small>
                    <strong>
                      {(
                        selectedState?.speed ??
                        selectedDevice.lastSpeed ??
                        0
                      ).toFixed(0)}{' '}
                      km/h
                    </strong>
                  </div>

                  <div>
                    <small>
                      {isArabic
                        ? 'الاتجاه'
                        : 'Course'}
                    </small>
                    <strong>
                      {(
                        selectedState?.course ??
                        selectedDevice.lastCourse ??
                        0
                      ).toFixed(0)}
                      °
                    </strong>
                  </div>

                  <div>
                    <small>Latitude</small>
                    <strong>
                      {formatCoordinate(
                        selectedState?.latitude ??
                          selectedDevice.lastLatitude,
                      )}
                    </strong>
                  </div>

                  <div>
                    <small>Longitude</small>
                    <strong>
                      {formatCoordinate(
                        selectedState?.longitude ??
                          selectedDevice.lastLongitude,
                      )}
                    </strong>
                  </div>
                </div>
              </>
            ) : (
              <div className="empty-state">
                {isArabic
                  ? 'اختر جهازًا من القائمة'
                  : 'Select a device'}
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="panel devices-panel">
        <div className="panel-head">
          <div className="panel-title">
            {isArabic
              ? 'قائمة الأجهزة'
              : 'Devices'}

            <span>
              {isArabic
                ? `${devices.length} جهاز`
                : `${devices.length} devices`}
            </span>
          </div>

          <div className="device-count">
            {onlineDevices.length}{' '}
            {isArabic ? 'متصل' : 'online'}
          </div>
        </div>

        {loading ? (
          <div className="loading-state">
            {isArabic
              ? 'جاري تحميل الأجهزة...'
              : 'Loading devices...'}
          </div>
        ) : devices.length === 0 ? (
          <div className="empty-state">
            {isArabic
              ? 'لا توجد أجهزة مسجلة'
              : 'No registered devices'}
          </div>
        ) : (
          <div className="device-table-wrap">
            <table>
              <thead>
                <tr>
                  <th>
                    {isArabic
                      ? 'الجهاز'
                      : 'Device'}
                  </th>

                  <th>
                    {isArabic
                      ? 'الحالة'
                      : 'Status'}
                  </th>

                  <th>
                    {isArabic
                      ? 'السرعة'
                      : 'Speed'}
                  </th>

                  <th>Latitude</th>

                  <th>Longitude</th>

                  <th>
                    {isArabic
                      ? 'آخر اتصال'
                      : 'Last Seen'}
                  </th>
                </tr>
              </thead>

              <tbody>
                {devices.map((device) => {
                  const state = states.find(
                    (item) =>
                      item.deviceId ===
                      device.imei,
                  )

                  const status =
                    getDeviceStatus(
                      device,
                      state,
                    )

                  return (
                    <tr
                      key={device.imei}
                      className={
                        device.imei ===
                        selectedImei
                          ? 'selected-row'
                          : ''
                      }
                      onClick={() =>
                        setSelectedImei(
                          device.imei,
                        )
                      }
                    >
                      <td>
                        <div className="device-cell">
                          <div
                            className={`device-icon ${status}`}
                          >
                            {status ===
                            'moving'
                              ? '↗'
                              : status ===
                                  'idle'
                                ? '◷'
                                : '○'}
                          </div>

                          <div>
                            <div className="device-name">
                              {device.name ||
                                device.imei}
                            </div>

                            <div className="device-imei">
                              {device.imei}
                            </div>
                          </div>
                        </div>
                      </td>

                      <td>
                        <span
                          className={`status-pill ${status}`}
                        >
                          <span className="d" />

                          {status ===
                          'moving'
                            ? isArabic
                              ? 'تتحرك'
                              : 'Moving'
                            : status ===
                                'idle'
                              ? isArabic
                                ? 'متوقفة'
                                : 'Idle'
                              : isArabic
                                ? 'غير متصلة'
                                : 'Offline'}
                        </span>
                      </td>

                      <td className="num">
                        {(
                          state?.speed ??
                          device.lastSpeed ??
                          0
                        ).toFixed(0)}{' '}
                        km/h
                      </td>

                      <td className="num">
                        {formatCoordinate(
                          state?.latitude ??
                            device.lastLatitude,
                        )}
                      </td>

                      <td className="num">
                        {formatCoordinate(
                          state?.longitude ??
                            device.lastLongitude,
                        )}
                      </td>

                      <td className="num">
                        {formatTime(
                          device.lastSeen,
                          language,
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="system-footer">
        <span>
          {isArabic
            ? 'Tracking Platform — البيانات الحقيقية من الخادم'
            : 'Tracking Platform — Live data from server'}
        </span>

        <span>
          {isArabic
            ? 'آخر تحديث'
            : 'Last refresh'}:{' '}
          {lastRefresh
            ? formatTime(
                lastRefresh.toISOString(),
                language,
              )
            : '—'}
        </span>
      </div>
    </div>
  )
}
