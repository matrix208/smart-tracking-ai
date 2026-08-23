
import type { Language } from '../App'
import type { Device } from '../api/trackingApi'

interface DeviceTableProps {
  devices: Device[]
  language: Language
}

function formatDate(
  value?: string,
  language: Language = 'ar',
) {
  if (!value) return '—'

  return new Date(value).toLocaleString(
    language === 'ar' ? 'ar-SA' : 'en-US',
    {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    },
  )
}

function formatSpeed(
  speed?: number | null,
) {
  if (speed == null) return '—'

  return `${speed.toFixed(1)} km/h`
}

export function DeviceTable({
  devices,
  language,
}: DeviceTableProps) {
  const isArabic = language === 'ar'

  return (
    <div className="table-container">

      <table>

        <thead>

          <tr>

            <th>
              {isArabic ? 'الجهاز' : 'Device'}
            </th>

            <th>
              IMEI
            </th>

            <th>
              {isArabic ? 'الحالة' : 'Status'}
            </th>

            <th>
              {isArabic ? 'السرعة' : 'Speed'}
            </th>

            <th>
              {isArabic ? 'آخر ظهور' : 'Last Seen'}
            </th>

            <th>
              {isArabic ? 'الموقع' : 'Location'}
            </th>

          </tr>

        </thead>

        <tbody>

          {devices.map((device) => (

            <tr key={device.imei}>

              <td>

                <div className="device-name">

                  <span
                    className={`device-dot ${
                      device.isOnline
                        ? 'online'
                        : 'offline'
                    }`}
                  />

                  <div>

                    <strong>
                      {device.name || device.imei}
                    </strong>

                    {device.protocol && (
                      <small>
                        {device.protocol}
                      </small>
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
                    device.isOnline
                      ? 'status-online'
                      : 'status-offline'
                  }`}
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
                {formatSpeed(device.lastSpeed)}
              </td>

              <td>
                {formatDate(
                  device.lastSeen,
                  language,
                )}
              </td>

              <td>

                {device.lastLatitude != null &&
                device.lastLongitude != null
                  ? `${device.lastLatitude.toFixed(
                      5,
                    )}, ${device.lastLongitude.toFixed(
                      5,
                    )}`
                  : '—'}

              </td>

            </tr>

          ))}

          {devices.length === 0 && (

            <tr>

              <td
                colSpan={6}
                className="empty"
              >
                {isArabic
                  ? 'لا توجد أجهزة'
                  : 'No devices found'}
              </td>

            </tr>

          )}

        </tbody>

      </table>

    </div>
  )
}

