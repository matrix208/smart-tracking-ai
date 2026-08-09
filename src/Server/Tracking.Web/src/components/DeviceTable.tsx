import type { Device } from '../api/trackingApi'

interface DeviceTableProps {
  devices: Device[]
}

function formatDate(value?: string) {
  if (!value) return '—'

  return new Date(value).toLocaleString('ar-SA', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

function formatSpeed(speed?: number | null) {
  if (speed == null) return '—'
  return `${speed.toFixed(1)} km/h`
}

export function DeviceTable({ devices }: DeviceTableProps) {
  return (
    <div className="table-container">
      <table>
        <thead>
          <tr>
            <th>الجهاز</th>
            <th>IMEI</th>
            <th>الحالة</th>
            <th>السرعة</th>
            <th>آخر ظهور</th>
            <th>الموقع</th>
          </tr>
        </thead>

        <tbody>
          {devices.map((device) => (
            <tr key={device.imei}>
              <td>
                <div className="device-name">
                  <span
                    className={`device-dot ${
                      device.isOnline ? 'online' : 'offline'
                    }`}
                  />
                  <div>
                    <strong>
                      {device.name || device.imei}
                    </strong>

                    {device.protocol && (
                      <small>{device.protocol}</small>
                    )}
                  </div>
                </div>
              </td>

              <td className="imei">
                {device.imei}
              </td>

              <td>
                <span
                  className={`status ${
                    device.isOnline ? 'status-online' : 'status-offline'
                  }`}
                >
                  {device.isOnline ? 'متصل' : 'غير متصل'}
                </span>
              </td>

              <td>
                {formatSpeed(device.lastSpeed)}
              </td>

              <td>
                {formatDate(device.lastSeen)}
              </td>

              <td>
                {device.lastLatitude != null &&
                device.lastLongitude != null
                  ? `${device.lastLatitude.toFixed(5)}, ${device.lastLongitude.toFixed(5)}`
                  : '—'}
              </td>
            </tr>
          ))}

          {devices.length === 0 && (
            <tr>
              <td colSpan={6} className="empty">
                لا توجد أجهزة
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
