interface StatCardProps {
  title: string
  value: number | string
  subtitle: string
  icon: string
}

export function StatCard({
  title,
  value,
  subtitle,
  icon,
}: StatCardProps) {
  return (
    <div className="stat-card">
      <div className="stat-icon">{icon}</div>

      <div className="stat-content">
        <span className="stat-title">{title}</span>
        <strong className="stat-value">{value}</strong>
        <span className="stat-subtitle">{subtitle}</span>
      </div>
    </div>
  )
}
