import { useState } from 'react'

interface AIInsightsProps {
  language: 'ar' | 'en'
}

type Tab = 'drivers' | 'maintenance' | 'eta' | 'anomalies'

interface DriverRisk {
  driver: string
  vehicle: string
  risk: number
  braking: number
  acceleration: number
  speeding: number
  trend: 'up' | 'down' | 'flat'
}

interface MaintenancePrediction {
  vehicle: string
  component: string
  window: string
  confidence: number
  action: string
}

interface ETAPrediction {
  vehicle: string
  destination: string
  distance: string
  eta: string
  savings: string
}

interface Anomaly {
  time: string
  vehicle: string
  title: string
  description: string
  severity: 'low' | 'medium' | 'high'
}

const driverRisks: DriverRisk[] = [
  {
    driver: 'أحمد محمد',
    vehicle: 'شاحنة 102',
    risk: 82,
    braking: 14,
    acceleration: 9,
    speeding: 18,
    trend: 'up',
  },
  {
    driver: 'خالد علي',
    vehicle: 'مركبة 205',
    risk: 67,
    braking: 9,
    acceleration: 7,
    speeding: 11,
    trend: 'up',
  },
  {
    driver: 'محمد سالم',
    vehicle: 'شاحنة 118',
    risk: 48,
    braking: 6,
    acceleration: 5,
    speeding: 6,
    trend: 'down',
  },
  {
    driver: 'سعيد حسن',
    vehicle: 'مركبة 301',
    risk: 31,
    braking: 3,
    acceleration: 4,
    speeding: 3,
    trend: 'down',
  },
  {
    driver: 'عمر عبدالله',
    vehicle: 'شاحنة 407',
    risk: 19,
    braking: 2,
    acceleration: 1,
    speeding: 2,
    trend: 'flat',
  },
]

const maintenancePredictions: MaintenancePrediction[] = [
  {
    vehicle: 'شاحنة 102',
    component: 'بطارية التشغيل',
    window: 'خلال 7 - 14 يوم',
    confidence: 91,
    action: 'فحص البطارية وشحنها واختبار الجهد قبل الرحلة القادمة.',
  },
  {
    vehicle: 'مركبة 205',
    component: 'نظام الفرامل',
    window: 'خلال 14 - 21 يوم',
    confidence: 84,
    action: 'جدولة فحص تيل الفرامل وأقراص الفرامل.',
  },
  {
    vehicle: 'شاحنة 118',
    component: 'الإطارات',
    window: 'خلال 21 - 30 يوم',
    confidence: 78,
    action: 'فحص ضغط الإطارات وعمق النقشة والتآكل غير المنتظم.',
  },
  {
    vehicle: 'مركبة 301',
    component: 'زيت المحرك',
    window: 'خلال 30 - 45 يوم',
    confidence: 73,
    action: 'التخطيط لتغيير الزيت والفلاتر ضمن الصيانة الدورية.',
  },
  {
    vehicle: 'شاحنة 407',
    component: 'نظام التبريد',
    window: 'خلال 45 - 60 يوم',
    confidence: 68,
    action: 'فحص مستوى سائل التبريد والخراطيم ودرجة حرارة التشغيل.',
  },
  {
    vehicle: 'مركبة 512',
    component: 'المولد الكهربائي',
    window: 'خلال 60 - 90 يوم',
    confidence: 64,
    action: 'إجراء اختبار شحن وفحص السير والتوصيلات.',
  },
]

const etaPredictions: ETAPrediction[] = [
  {
    vehicle: 'شاحنة 102',
    destination: 'مستودع الرياض',
    distance: '42 كم',
    eta: '38 دقيقة',
    savings: '12%',
  },
  {
    vehicle: 'مركبة 205',
    destination: 'مطار الملك خالد',
    distance: '31 كم',
    eta: '29 دقيقة',
    savings: '18%',
  },
  {
    vehicle: 'شاحنة 118',
    destination: 'المنطقة الصناعية',
    distance: '57 كم',
    eta: '51 دقيقة',
    savings: '9%',
  },
  {
    vehicle: 'مركبة 301',
    destination: 'مركز التوزيع',
    distance: '24 كم',
    eta: '23 دقيقة',
    savings: '15%',
  },
  {
    vehicle: 'شاحنة 407',
    destination: 'بوابة الشحن',
    distance: '76 كم',
    eta: '1 ساعة و 7 دقائق',
    savings: '11%',
  },
  {
    vehicle: 'مركبة 512',
    destination: 'المستودع الرئيسي',
    distance: '18 كم',
    eta: '17 دقيقة',
    savings: '21%',
  },
]

const anomalies: Anomaly[] = [
  {
    time: '14:08',
    vehicle: 'شاحنة 102',
    title: 'انحراف عن المسار',
    description: 'تم رصد انحراف يقارب 1.8 كم عن المسار المخطط.',
    severity: 'high',
  },
  {
    time: '13:42',
    vehicle: 'مركبة 205',
    title: 'سرعة غير معتادة',
    description: 'ارتفاع السرعة عن النمط المعتاد للمركبة في هذه المنطقة.',
    severity: 'medium',
  },
  {
    time: '12:55',
    vehicle: 'شاحنة 118',
    title: 'توقف طويل',
    description: 'توقف لمدة 47 دقيقة في موقع غير مسجل كنقطة توقف.',
    severity: 'medium',
  },
  {
    time: '11:31',
    vehicle: 'مركبة 301',
    title: 'وقت تشغيل غير معتاد',
    description: 'نشاط للمركبة خارج نمط التشغيل المعتاد.',
    severity: 'low',
  },
  {
    time: '10:16',
    vehicle: 'شاحنة 407',
    title: 'اشتباه سرقة',
    description: 'حركة غير متوقعة بعد توقف المركبة مع تغير مفاجئ في المسار.',
    severity: 'high',
  },
  {
    time: '09:48',
    vehicle: 'مركبة 512',
    title: 'انحراف بسيط عن المسار',
    description: 'تغير محدود في المسار مع عودة المركبة للمسار المخطط.',
    severity: 'low',
  },
]

function getRiskLevel(risk: number): 'low' | 'medium' | 'high' {
  if (risk >= 70) return 'high'
  if (risk >= 40) return 'medium'
  return 'low'
}

function RiskBadge({ risk }: { risk: number }) {
  const level = getRiskLevel(risk)

  return (
    <div className="ai-risk-cell">
      <div className="ai-progress-track">
        <div
          className={`ai-progress-fill ${level}`}
          style={{ width: `${risk}%` }}
        />
      </div>
      <span className={`ai-risk-badge ${level}`}>
        {risk}
      </span>
    </div>
  )
}

function Trend({
  trend,
  isArabic,
}: {
  trend: DriverRisk['trend']
  isArabic: boolean
}) {
  if (trend === 'up') {
    return (
      <span className="ai-trend up">
        ↗ {isArabic ? 'تدهور' : 'Worsening'}
      </span>
    )
  }

  if (trend === 'down') {
    return (
      <span className="ai-trend down">
        ↘ {isArabic ? 'تحسّن' : 'Improving'}
      </span>
    )
  }

  return (
    <span className="ai-trend flat">
      → {isArabic ? 'مستقر' : 'Stable'}
    </span>
  )
}

function AIInsights({ language }: AIInsightsProps) {
  const [activeTab, setActiveTab] = useState<Tab>('drivers')
  const isArabic = language === 'ar'

  const tabs: Array<{
    id: Tab
    icon: string
    ar: string
    en: string
  }> = [
    {
      id: 'drivers',
      icon: '🚗',
      ar: 'سلوك السائقين',
      en: 'Driver Behavior',
    },
    {
      id: 'maintenance',
      icon: '🔧',
      ar: 'الصيانة الوقائية',
      en: 'Predictive Maintenance',
    },
    {
      id: 'eta',
      icon: '🕐',
      ar: 'التنبؤ بالوصول',
      en: 'ETA Prediction',
    },
    {
      id: 'anomalies',
      icon: '⚠️',
      ar: 'كشف الشذوذ',
      en: 'Anomaly Detection',
    },
  ]

  const highRiskCount = driverRisks.filter(
    (driver) => driver.risk >= 70,
  ).length

  const maintenanceCount = maintenancePredictions.length

  return (
    <div className={`smart-page ai-page ${isArabic ? 'rtl' : 'ltr'}`}>
      <section className="smart-page-header">
        <div>
          <span>
            {isArabic ? 'التحليلات الذكية' : 'SMART ANALYTICS'}
          </span>
          <h1>
            {isArabic ? 'AI Insights' : 'AI Insights'}
          </h1>
          <p>
            {isArabic
              ? 'تحليلات تنبؤية تجريبية لسلوك السائقين والصيانة والوصول والشذوذ.'
              : 'Experimental predictive analytics for driver behavior, maintenance, ETA and anomalies.'}
          </p>
        </div>

        <div className="ai-badge-simulated">
          <span />
          {isArabic ? 'بيانات محاكاة تجريبية' : 'Experimental Simulated Data'}
        </div>
      </section>

      <section className="ai-stats-grid">
        <div className="ai-stat-card">
          <div className="ai-stat-icon risk">⚠</div>
          <div>
            <span>{isArabic ? 'مخاطر مرتفعة' : 'High Risk'}</span>
            <strong>{highRiskCount}</strong>
            <small>
              {isArabic
                ? 'سائقون يحتاجون للمراجعة'
                : 'Drivers requiring review'}
            </small>
          </div>
        </div>

        <div className="ai-stat-card">
          <div className="ai-stat-icon maint">🔧</div>
          <div>
            <span>{isArabic ? 'تنبؤات الصيانة' : 'Maintenance Predictions'}</span>
            <strong>{maintenanceCount}</strong>
            <small>
              {isArabic
                ? 'حالات متوقعة'
                : 'Predicted cases'}
            </small>
          </div>
        </div>

        <div className="ai-stat-card">
          <div className="ai-stat-icon eta">⌁</div>
          <div>
            <span>{isArabic ? 'تنبؤات الوصول' : 'ETA Predictions'}</span>
            <strong>{etaPredictions.length}</strong>
            <small>
              {isArabic
                ? 'مركبات قيد التنبؤ'
                : 'Vehicles being predicted'}
            </small>
          </div>
        </div>

        <div className="ai-stat-card">
          <div className="ai-stat-icon anomaly">!</div>
          <div>
            <span>{isArabic ? 'أحداث شاذة' : 'Anomalies'}</span>
            <strong>{anomalies.length}</strong>
            <small>
              {isArabic
                ? 'أحداث مرصودة'
                : 'Detected events'}
            </small>
          </div>
        </div>
      </section>

      <div className="ai-section-tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            className={activeTab === tab.id ? 'active' : ''}
            onClick={() => setActiveTab(tab.id)}
          >
            <span>{tab.icon}</span>
            {isArabic ? tab.ar : tab.en}
          </button>
        ))}
      </div>

      {activeTab === 'drivers' && (
        <section className="ai-panel smart-panel">
          <div className="smart-panel-header">
            <div>
              <span>
                {isArabic
                  ? 'تحليل المخاطر السلوكية'
                  : 'BEHAVIORAL RISK ANALYSIS'}
              </span>
              <h2>
                {isArabic
                  ? 'ترتيب السائقين حسب درجة الخطورة'
                  : 'Drivers ranked by risk score'}
              </h2>
            </div>
          </div>

          <div className="smart-table-wrapper">
            <table className="smart-table">
              <thead>
                <tr>
                  <th>{isArabic ? 'السائق' : 'Driver'}</th>
                  <th>{isArabic ? 'المركبة' : 'Vehicle'}</th>
                  <th>{isArabic ? 'درجة الخطورة' : 'Risk Score'}</th>
                  <th>{isArabic ? 'فرملة مفاجئة' : 'Hard Braking'}</th>
                  <th>{isArabic ? 'تسارع مفاجئ' : 'Hard Acceleration'}</th>
                  <th>{isArabic ? 'تجاوز سرعة' : 'Speeding'}</th>
                  <th>{isArabic ? 'الاتجاه' : 'Trend'}</th>
                </tr>
              </thead>
              <tbody>
                {driverRisks.map((driver) => (
                  <tr key={`${driver.driver}-${driver.vehicle}`}>
                    <td>
                      <strong>{driver.driver}</strong>
                    </td>
                    <td>{driver.vehicle}</td>
                    <td>
                      <RiskBadge risk={driver.risk} />
                    </td>
                    <td>{driver.braking}</td>
                    <td>{driver.acceleration}</td>
                    <td>{driver.speeding}</td>
                    <td>
                      <Trend
                        trend={driver.trend}
                        isArabic={isArabic}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {activeTab === 'maintenance' && (
        <div className="ai-card-grid">
          {maintenancePredictions.map((prediction) => (
            <article
              className="ai-predict-card"
              key={`${prediction.vehicle}-${prediction.component}`}
            >
              <div className="ai-predict-head">
                <div>
                  <span className="ai-predict-vehicle">
                    {prediction.vehicle}
                  </span>
                  <h3>{prediction.component}</h3>
                </div>

                <span className="ai-confidence-pill">
                  {prediction.confidence}%{' '}
                  {isArabic ? 'ثقة' : 'confidence'}
                </span>
              </div>

              <div className="ai-predict-window">
                <span>
                  {isArabic ? 'الإطار المتوقع' : 'Expected window'}
                </span>
                <strong>{prediction.window}</strong>
              </div>

              <div className="ai-predict-action">
                <strong>
                  {isArabic ? 'الإجراء المقترح: ' : 'Recommended action: '}
                </strong>
                {prediction.action}
              </div>
            </article>
          ))}
        </div>
      )}

      {activeTab === 'eta' && (
        <div className="ai-card-grid">
          {etaPredictions.map((prediction) => (
            <article
              className="ai-predict-card"
              key={`${prediction.vehicle}-${prediction.destination}`}
            >
              <div className="ai-predict-head">
                <div>
                  <span className="ai-predict-vehicle">
                    {prediction.vehicle}
                  </span>
                  <h3>{prediction.destination}</h3>
                </div>

                <span className="ai-confidence-pill">
                  ETA
                </span>
              </div>

              <div className="ai-eta-meta">
                <div>
                  <small>
                    {isArabic ? 'المسافة المتبقية' : 'Remaining'}
                  </small>
                  <strong>{prediction.distance}</strong>
                </div>

                <div>
                  <small>
                    {isArabic ? 'الوصول المتوقع' : 'Expected arrival'}
                  </small>
                  <strong>{prediction.eta}</strong>
                </div>

                <div>
                  <small>
                    {isArabic ? 'توفير المسار' : 'Route saving'}
                  </small>
                  <strong className="ai-savings">
                    {prediction.savings}
                  </strong>
                </div>
              </div>

              <div className="ai-predict-action">
                {isArabic
                  ? 'التنبؤ مبني على المسار الحالي وبيانات الحركة المحاكاة.'
                  : 'Prediction is based on the current route and simulated traffic data.'}
              </div>
            </article>
          ))}
        </div>
      )}

      {activeTab === 'anomalies' && (
        <section className="ai-panel smart-panel">
          <div className="smart-panel-header">
            <div>
              <span>
                {isArabic
                  ? 'المراقبة الذكية'
                  : 'SMART MONITORING'}
              </span>
              <h2>
                {isArabic
                  ? 'الأحداث غير المعتادة'
                  : 'Unusual events'}
              </h2>
            </div>
          </div>

          <div className="ai-anomaly-list">
            {anomalies.map((anomaly) => (
              <article
                className="ai-anomaly-item"
                key={`${anomaly.time}-${anomaly.vehicle}-${anomaly.title}`}
              >
                <span
                  className={`ai-severity-dot ${anomaly.severity}`}
                />

                <span className="ai-anomaly-time">
                  {anomaly.time}
                </span>

                <div className="ai-anomaly-body">
                  <div className="ai-anomaly-title">
                    <strong>{anomaly.title}</strong>
                    <span>{anomaly.vehicle}</span>
                  </div>

                  <p>{anomaly.description}</p>
                </div>

                <span
                  className={`ai-severity-pill ${anomaly.severity}`}
                >
                  {anomaly.severity === 'high'
                    ? isArabic
                      ? 'مرتفع'
                      : 'High'
                    : anomaly.severity === 'medium'
                      ? isArabic
                        ? 'متوسط'
                        : 'Medium'
                      : isArabic
                        ? 'منخفض'
                        : 'Low'}
                </span>
              </article>
            ))}
          </div>
        </section>
      )}
    </div>
  )
}

export default AIInsights
