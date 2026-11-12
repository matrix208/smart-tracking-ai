import { useEffect, useRef, useState } from 'react'
import {
  MapContainer,
  Marker,
  Popup,
  TileLayer,
  useMap,
} from 'react-leaflet'
import L from 'leaflet'

import 'leaflet/dist/leaflet.css'

import {
  getDevices,
  type Device,
} from '../api/trackingApi'

import type { Language } from '../App'

type LiveMapProps = {
  language: Language
}

const defaultCenter: [number, number] = [24.7136, 46.6753]

function MapController({
  devices,
}: {
  devices: Device[]
}) {
  const map = useMap()

  useEffect(() => {
    const validDevices = devices.filter(
      (device) =>
        device.lastLatitude != null &&
        device.lastLongitude != null,
    )

    if (validDevices.length === 0) {
      map.setView(defaultCenter, 12)
      return
    }

    const bounds = L.latLngBounds(
      validDevices.map((device) => [
        device.lastLatitude!,
        device.lastLongitude!,
      ]),
    )

    map.fitBounds(bounds, {
      padding: [60, 60],
      maxZoom: 16,
    })
  }, [devices, map])

  return null
}

function createVehicleIcon(isOnline: boolean) {
  return L.divIcon({
    className: 'vehicle-marker-wrapper',
    html: `
      <div class="vehicle-marker ${isOnline ? 'online' : 'offline'}">
        <div class="vehicle-marker-pulse"></div>
        <div class="vehicle-marker-icon">🚘</div>
      </div>
    `,
    iconSize: [46, 46],
    iconAnchor: [23, 23],
    popupAnchor: [0, -24],
  })
}

function LiveMap({ language }: LiveMapProps) {
  const isArabic = language === 'ar'

  const [devices, setDevices] = useState<Device[]>([])
  const [loading, setLoading] = useState(true)
  const [lastUpdate, setLastUpdate] = useState(new Date())

  const timerRef = useRef<number | null>(null)

  const loadDevices = async () => {
    try {
      const data = await getDevices()

      setDevices(data ?? [])
      setLastUpdate(new Date())
    } catch (error) {
      console.error('Live map loading failed:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDevices()

    timerRef.current = window.setInterval(
      loadDevices,
      10000,
    )

    return () => {
      if (timerRef.current !== null) {
        window.clearInterval(timerRef.current)
      }
    }
  }, [])

  const mappedDevices = devices.filter(
    (device) =>
      device.lastLatitude != null &&
      device.lastLongitude != null,
  )

  return (
    <main className="live-map-page">

      <section className="live-map-header">

        <div>
          <div className="dashboard-hero-kicker">
            <span className="hero-live-dot" />
            LIVE TRACKING
          </div>

          <h1>
            {isArabic
              ? 'الخريطة الحية'
              : 'Live Map'}
          </h1>

          <p>
            {isArabic
              ? 'مراقبة مواقع المركبات الحالية على الخريطة لحظة بلحظة.'
              : 'Monitor the current vehicle locations in real time.'}
          </p>
        </div>

        <div className="live-map-status">

          <span className="status-dot online" />

          <strong>
            {mappedDevices.length}
          </strong>

          <span>
            {isArabic
              ? 'مركبة على الخريطة'
              : 'vehicles on map'}
          </span>

        </div>

      </section>

      <section className="live-map-card">

        <div className="live-map-toolbar">

          <div className="map-toolbar-title">

            <span className="map-toolbar-icon">
              ⌖
            </span>

            <div>
              <strong>
                {isArabic
                  ? 'الموقع الحالي'
                  : 'Current Location'}
              </strong>

              <small>
                {isArabic
                  ? `آخر تحديث ${lastUpdate.toLocaleTimeString('ar-SA')}`
                  : `Last update ${lastUpdate.toLocaleTimeString('en-US')}`}
              </small>
            </div>

          </div>

          <button
            type="button"
            className="refresh-button"
            onClick={loadDevices}
            disabled={loading}
          >
            <span
              className={
                loading ? 'refresh-spin' : ''
              }
            >
              ↻
            </span>

            {loading
              ? isArabic
                ? 'جاري التحديث'
                : 'Updating'
              : isArabic
                ? 'تحديث'
                : 'Refresh'}
          </button>

        </div>

        <div className="real-map">

          <MapContainer
            center={defaultCenter}
            zoom={12}
            scrollWheelZoom={true}
            zoomControl={true}
          >

            <TileLayer
              attribution='&copy; OpenStreetMap contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <MapController devices={mappedDevices} />

            {mappedDevices.map((device) => (

              <Marker
                key={device.imei}
                position={[
                  device.lastLatitude!,
                  device.lastLongitude!,
                ]}
                icon={createVehicleIcon(
                  device.isOnline,
                )}
              >

                <Popup>

                  <div className="vehicle-popup">

                    <strong>
                      {device.name ||
                        device.imei}
                    </strong>

                    <span>
                      IMEI: {device.imei}
                    </span>

                    <span>
                      {isArabic
                        ? 'الحالة: '
                        : 'Status: '}

                      {device.isOnline
                        ? isArabic
                          ? 'متصل'
                          : 'Online'
                        : isArabic
                          ? 'غير متصل'
                          : 'Offline'}
                    </span>

                    <span>
                      {isArabic
                        ? 'السرعة: '
                        : 'Speed: '}

                      {device.lastSpeed ?? 0}
                      {' km/h'}
                    </span>

                    <span className="popup-coordinates">
                      {device.lastLatitude!.toFixed(5)}
                      {' , '}
                      {device.lastLongitude!.toFixed(5)}
                    </span>

                  </div>

                </Popup>

              </Marker>

            ))}

          </MapContainer>

          {mappedDevices.length === 0 && !loading && (

            <div className="map-empty-overlay">

              <div className="map-empty-icon">
                ⌖
              </div>

              <strong>
                {isArabic
                  ? 'لا توجد مركبات بإحداثيات'
                  : 'No vehicles with coordinates'}
              </strong>

              <span>
                {isArabic
                  ? 'تأكد من وصول بيانات GPS من جهاز التتبع.'
                  : 'Make sure GPS data is being received.'}
              </span>

            </div>

          )}

        </div>

      </section>

    </main>
  )
}

export default LiveMap
