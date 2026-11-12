import { useEffect, useMemo } from 'react'
import {
  MapContainer,
  Marker,
  Popup,
  TileLayer,
  useMap,
} from 'react-leaflet'
import L from 'leaflet'
import type { Device } from '../../api/trackingApi'

import 'leaflet/dist/leaflet.css'

interface TrackingMapProps {
  devices: Device[]
  selectedImei: string | null
  onSelect: (imei: string) => void
}

function createVehicleIcon(
  online: boolean,
  course: number,
  selected: boolean,
) {
  const rotation = Number.isFinite(course) ? course : 0

  return L.divIcon({
    className: 'vehicle-marker-wrapper',
    html: `
      <div
        class="vehicle-marker
          ${online ? 'vehicle-online' : 'vehicle-offline'}
          ${selected ? 'vehicle-selected' : ''}"
        style="transform: rotate(${rotation}deg)"
      >
        <div class="vehicle-arrow"></div>
        <div class="vehicle-body"></div>
      </div>
    `,
    iconSize: [42, 42],
    iconAnchor: [21, 21],
    popupAnchor: [0, -22],
  })
}

function SelectedDeviceController({
  devices,
  selectedImei,
}: TrackingMapProps) {
  const map = useMap()

  useEffect(() => {
    if (!selectedImei) return

    const device = devices.find(
      (item) => item.imei === selectedImei,
    )

    if (
      !device ||
      device.lastLatitude == null ||
      device.lastLongitude == null
    ) {
      return
    }

    map.flyTo(
      [device.lastLatitude, device.lastLongitude],
      Math.max(map.getZoom(), 14),
      {
        duration: 0.7,
      },
    )
  }, [devices, selectedImei, map])

  return null
}

export function TrackingMap({
  devices,
  selectedImei,
  onSelect,
}: TrackingMapProps) {
  const validDevices = useMemo(
    () =>
      devices.filter(
        (device) =>
          device.lastLatitude != null &&
          device.lastLongitude != null,
      ),
    [devices],
  )

  const center: [number, number] =
    validDevices.length > 0
      ? [
          validDevices[0].lastLatitude!,
          validDevices[0].lastLongitude!,
        ]
      : [24.7136, 46.6753]

  return (
    <MapContainer
      center={center}
      zoom={12}
      className="tracking-map"
      zoomControl
      preferCanvas
    >
      <TileLayer
        attribution="&copy; OpenStreetMap contributors"
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      <SelectedDeviceController
        devices={devices}
        selectedImei={selectedImei}
        onSelect={onSelect}
      />

      {validDevices.map((device) => {
        const selected =
          device.imei === selectedImei

        return (
          <Marker
            key={device.imei}
            position={[
              device.lastLatitude!,
              device.lastLongitude!,
            ]}
            icon={createVehicleIcon(
              device.isOnline,
              device.lastCourse ?? 0,
              selected,
            )}
            eventHandlers={{
              click: () => onSelect(device.imei),
            }}
          >
            <Popup>
              <div className="device-popup">
                <div className="popup-title">
                  <strong>
                    {device.name || 'مركبة'}
                  </strong>

                  <span
                    className={
                      device.isOnline
                        ? 'popup-online'
                        : 'popup-offline'
                    }
                  >
                    {device.isOnline
                      ? 'متصل'
                      : 'غير متصل'}
                  </span>
                </div>

                <div className="popup-imei">
                  {device.imei}
                </div>

                <div className="popup-grid">
                  <div>
                    <small>السرعة</small>
                    <strong>
                      {device.lastSpeed != null
                        ? `${device.lastSpeed.toFixed(0)} km/h`
                        : '—'}
                    </strong>
                  </div>

                  <div>
                    <small>الاتجاه</small>
                    <strong>
                      {device.lastCourse != null
                        ? `${device.lastCourse.toFixed(0)}°`
                        : '—'}
                    </strong>
                  </div>

                  <div>
                    <small>Latitude</small>
                    <strong>
                      {device.lastLatitude!.toFixed(6)}
                    </strong>
                  </div>

                  <div>
                    <small>Longitude</small>
                    <strong>
                      {device.lastLongitude!.toFixed(6)}
                    </strong>
                  </div>
                </div>
              </div>
            </Popup>
          </Marker>
        )
      })}
    </MapContainer>
  )
}
