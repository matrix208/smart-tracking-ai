import { useEffect, useMemo, useState } from 'react'
import {
  getRoles, getRole, createRole, updateRole, deleteRole, setRolePermissions,
  getPermissions, getUsers, setUserRoles,
  type RoleSummary, type RoleDetail, type Permission, type UserSummary,
} from '../api/rolesApi'
import type { Language } from '../App'

interface Props { language: Language }

function RolesPermissions({ language }: Props) {
  const isArabic = language === 'ar'

  const [roles, setRoles] = useState<RoleSummary[]>([])
  const [permissions, setPermissions] = useState<Permission[]>([])
  const [users, setUsers] = useState<UserSummary[]>([])

  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null)
  const [roleDetail, setRoleDetail] = useState<RoleDetail | null>(null)
  const [checkedKeys, setCheckedKeys] = useState<Set<string>>(new Set())

  const [newRoleName, setNewRoleName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const loadAll = () => {
    getRoles().then(setRoles).catch((e) => setError(e.message))
    getPermissions().then(setPermissions).catch((e) => setError(e.message))
    getUsers().then(setUsers).catch((e) => setError(e.message))
  }

  useEffect(loadAll, [])

  useEffect(() => {
    if (selectedRoleId == null) {
      setRoleDetail(null)
      return
    }
    getRole(selectedRoleId).then((detail) => {
      setRoleDetail(detail)
      setCheckedKeys(new Set(detail.permissionKeys))
    }).catch((e) => setError(e.message))
  }, [selectedRoleId])

  const grouped = useMemo(() => {
    const map = new Map<string, Permission[]>()
    for (const p of permissions) {
      if (!map.has(p.category)) map.set(p.category, [])
      map.get(p.category)!.push(p)
    }
    return Array.from(map.entries())
  }, [permissions])

  const togglePermission = (key: string) => {
    setCheckedKeys((prev) => {
      const next = new Set(prev)
      if (next.has(key)) next.delete(key)
      else next.add(key)
      return next
    })
  }

  const handleSavePermissions = async () => {
    if (selectedRoleId == null) return
    setSaving(true)
    setError(null)
    try {
      await setRolePermissions(selectedRoleId, Array.from(checkedKeys))
      loadAll()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    } finally {
      setSaving(false)
    }
  }

  const handleCreateRole = async () => {
    if (!newRoleName.trim()) return
    setSaving(true)
    setError(null)
    try {
      const created = await createRole(newRoleName.trim(), '')
      setNewRoleName('')
      loadAll()
      setSelectedRoleId(created.id)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    } finally {
      setSaving(false)
    }
  }

  const handleDeleteRole = async (id: number) => {
    if (!window.confirm(isArabic ? 'حذف هذا الدور؟' : 'Delete this role?')) return
    setError(null)
    try {
      await deleteRole(id)
      if (selectedRoleId === id) setSelectedRoleId(null)
      loadAll()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    }
  }

  const handleRenameRole = async (id: number, currentName: string) => {
    const name = window.prompt(isArabic ? 'اسم الدور الجديد' : 'New role name', currentName)
    if (!name || !name.trim()) return
    try {
      await updateRole(id, name.trim(), '')
      loadAll()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    }
  }

  const toggleUserRole = async (user: UserSummary, roleName: string, roleId: number) => {
    const hasRole = user.roles.includes(roleName)
    const currentRoleIds = roles.filter((r) => user.roles.includes(r.name)).map((r) => r.id)
    const nextRoleIds = hasRole
      ? currentRoleIds.filter((id) => id !== roleId)
      : [...currentRoleIds, roleId]

    try {
      await setUserRoles(user.id, nextRoleIds)
      loadAll()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    }
  }

  return (
    <div className="roles-page">
      <header className="smart-page-header">
        <div>
          <div className="smart-breadcrumb">
            <span>TRACKING PLATFORM</span>
            <span>/</span>
            <strong>{isArabic ? 'الصلاحيات' : 'Roles & Permissions'}</strong>
          </div>
          <h1>{isArabic ? 'الأدوار والصلاحيات' : 'Roles & Permissions'}</h1>
          <p>
            {isArabic
              ? 'تحديد الأدوار وتخصيص الصلاحيات لكل دور، وتعيين المستخدمين للأدوار.'
              : 'Define roles, customize each role\u2019s permissions, and assign users to roles.'}
          </p>
        </div>
      </header>

      {error && <div className="settings-alert error roles-error">{error}</div>}

      <section className="roles-layout">
        {/* ---- Roles list ---- */}
        <div className="smart-panel roles-list-panel">
          <div className="smart-panel-header">
            <div><span>ROLES</span><h2>{isArabic ? 'الأدوار' : 'Roles'}</h2></div>
          </div>

          <div className="roles-list">
            {roles.map((r) => (
              <div
                key={r.id}
                className={`roles-list-item ${selectedRoleId === r.id ? 'active' : ''}`}
                onClick={() => setSelectedRoleId(r.id)}
              >
                <div>
                  <strong>{r.name}</strong>
                  <small>{r.permissionCount} {isArabic ? 'صلاحية' : 'permissions'} · {r.userCount} {isArabic ? 'مستخدم' : 'users'}</small>
                </div>
                {r.isSystemRole ? (
                  <span className="roles-badge system">{isArabic ? 'نظامي' : 'System'}</span>
                ) : (
                  <div className="roles-item-actions">
                    <button onClick={(e) => { e.stopPropagation(); handleRenameRole(r.id, r.name) }}>✎</button>
                    <button onClick={(e) => { e.stopPropagation(); handleDeleteRole(r.id) }}>🗑</button>
                  </div>
                )}
              </div>
            ))}
          </div>

          <div className="roles-create-row">
            <input
              value={newRoleName}
              onChange={(e) => setNewRoleName(e.target.value)}
              placeholder={isArabic ? 'اسم دور جديد' : 'New role name'}
            />
            <button className="smart-dark-button" onClick={handleCreateRole} disabled={saving}>
              {isArabic ? 'إضافة' : 'Add'}
            </button>
          </div>
        </div>

        {/* ---- Permission checklist ---- */}
        <div className="smart-panel roles-permissions-panel">
          <div className="smart-panel-header">
            <div>
              <span>PERMISSIONS</span>
              <h2>
                {roleDetail
                  ? `${isArabic ? 'صلاحيات' : 'Permissions for'} ${roleDetail.name}`
                  : isArabic ? 'اختر دورًا' : 'Select a role'}
              </h2>
            </div>
            {roleDetail && (
              <button className="smart-dark-button" onClick={handleSavePermissions} disabled={saving}>
                {saving ? (isArabic ? 'جاري الحفظ...' : 'Saving...') : (isArabic ? 'حفظ' : 'Save')}
              </button>
            )}
          </div>

          {roleDetail && (
            <div className="roles-permission-groups">
              {grouped.map(([category, items]) => (
                <div key={category} className="roles-permission-group">
                  <div className="roles-permission-group-title">{category}</div>
                  <div className="roles-permission-chips">
                    {items.map((p) => (
                      <label key={p.key} className="roles-permission-chip">
                        <input
                          type="checkbox"
                          checked={checkedKeys.has(p.key)}
                          onChange={() => togglePermission(p.key)}
                        />
                        {p.name}
                      </label>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}

          {!roleDetail && (
            <div className="driver-empty">{isArabic ? 'اختر دورًا من القائمة لعرض صلاحياته' : 'Select a role from the list to view its permissions'}</div>
          )}
        </div>
      </section>

      {/* ---- Users -> role assignment ---- */}
      <section className="smart-panel roles-users-panel">
        <div className="smart-panel-header">
          <div><span>USERS</span><h2>{isArabic ? 'تعيين المستخدمين للأدوار' : 'Assign Users to Roles'}</h2></div>
        </div>

        <div className="smart-table-wrapper">
          <table className="smart-table">
            <thead>
              <tr>
                <th>{isArabic ? 'المستخدم' : 'User'}</th>
                {roles.map((r) => <th key={r.id}>{r.name}</th>)}
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td><strong>{u.displayName}</strong><br /><span className="smart-mono">{u.username}</span></td>
                  {roles.map((r) => (
                    <td key={r.id} style={{ textAlign: 'center' }}>
                      <input
                        type="checkbox"
                        checked={u.roles.includes(r.name)}
                        onChange={() => toggleUserRole(u, r.name, r.id)}
                      />
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}

export default RolesPermissions
