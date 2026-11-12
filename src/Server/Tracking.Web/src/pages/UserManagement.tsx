import { useEffect, useState } from 'react'
import {
  getUsers, createUser, setUserRoles, setUserActive, getRoles,
  type UserSummary, type RoleSummary,
} from '../api/rolesApi'
import type { Language } from '../App'

interface Props { language: Language }

function UserManagement({ language }: Props) {
  const isArabic = language === 'ar'

  const [users, setUsers] = useState<UserSummary[]>([])
  const [roles, setRoles] = useState<RoleSummary[]>([])
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const [showCreate, setShowCreate] = useState(false)
  const [newUsername, setNewUsername] = useState('')
  const [newDisplayName, setNewDisplayName] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [newRoleIds, setNewRoleIds] = useState<Set<number>>(new Set())
  const [creating, setCreating] = useState(false)

  const load = () => {
    setLoading(true)
    Promise.all([getUsers(), getRoles()])
      .then(([u, r]) => { setUsers(u); setRoles(r) })
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const toggleNewRole = (id: number) => {
    setNewRoleIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  const handleCreate = async () => {
    if (!newUsername.trim() || !newPassword.trim()) {
      setError(isArabic ? 'اسم المستخدم وكلمة المرور مطلوبة' : 'Username and password are required')
      return
    }
    setCreating(true)
    setError(null)
    try {
      await createUser(
        newUsername.trim(),
        newPassword.trim(),
        newDisplayName.trim() || newUsername.trim(),
        Array.from(newRoleIds),
      )
      setNewUsername('')
      setNewDisplayName('')
      setNewPassword('')
      setNewRoleIds(new Set())
      setShowCreate(false)
      load()
    } catch (e) {
      setError(e instanceof Error ? e.message : isArabic ? 'فشل إنشاء المستخدم' : 'Failed to create user')
    } finally {
      setCreating(false)
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
      load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    }
  }

  const toggleActive = async (user: UserSummary) => {
    try {
      await setUserActive(user.id, !user.isActive)
      load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed')
    }
  }

  return (
    <div className="users-page">
      <header className="smart-page-header">
        <div>
          <div className="smart-breadcrumb">
            <span>TRACKING PLATFORM</span>
            <span>/</span>
            <strong>{isArabic ? 'إدارة المستخدمين' : 'User Management'}</strong>
          </div>
          <h1>{isArabic ? 'إدارة المستخدمين' : 'User Management'}</h1>
          <p>
            {isArabic
              ? 'إنشاء حسابات المستخدمين، تفعيلها أو تعطيلها، وتعيين الأدوار لكل مستخدم.'
              : 'Create user accounts, activate or deactivate them, and assign roles to each user.'}
          </p>
        </div>

        <button className="smart-dark-button" onClick={() => setShowCreate((v) => !v)}>
          {showCreate ? (isArabic ? 'إلغاء' : 'Cancel') : (isArabic ? '+ مستخدم جديد' : '+ New User')}
        </button>
      </header>

      {error && <div className="settings-alert error roles-error">{error}</div>}

      {showCreate && (
        <section className="smart-panel users-create-panel">
          <div className="smart-panel-header">
            <div><span>NEW USER</span><h2>{isArabic ? 'مستخدم جديد' : 'New User'}</h2></div>
          </div>

          <div className="users-create-form">
            <div className="users-create-row">
              <div>
                <label>{isArabic ? 'اسم المستخدم' : 'Username'}</label>
                <input value={newUsername} onChange={(e) => setNewUsername(e.target.value)} />
              </div>
              <div>
                <label>{isArabic ? 'الاسم المعروض' : 'Display Name'}</label>
                <input value={newDisplayName} onChange={(e) => setNewDisplayName(e.target.value)} />
              </div>
              <div>
                <label>{isArabic ? 'كلمة المرور' : 'Password'}</label>
                <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
              </div>
            </div>

            <div>
              <label>{isArabic ? 'الأدوار' : 'Roles'}</label>
              <div className="roles-permission-chips">
                {roles.map((r) => (
                  <label key={r.id} className="roles-permission-chip">
                    <input
                      type="checkbox"
                      checked={newRoleIds.has(r.id)}
                      onChange={() => toggleNewRole(r.id)}
                    />
                    {r.name}
                  </label>
                ))}
              </div>
            </div>

            <button className="smart-dark-button" onClick={handleCreate} disabled={creating}>
              {creating ? (isArabic ? 'جاري الإنشاء...' : 'Creating...') : (isArabic ? 'إنشاء المستخدم' : 'Create User')}
            </button>
          </div>
        </section>
      )}

      <section className="smart-panel users-list-panel">
        <div className="smart-panel-header">
          <div><span>USERS</span><h2>{isArabic ? 'كل المستخدمين' : 'All Users'}</h2></div>
          <span className="smart-count">{users.length}</span>
        </div>

        {loading && <div className="driver-loading">{isArabic ? 'جاري التحميل...' : 'Loading...'}</div>}

        {!loading && (
          <div className="smart-table-wrapper">
            <table className="smart-table">
              <thead>
                <tr>
                  <th>{isArabic ? 'المستخدم' : 'User'}</th>
                  <th>{isArabic ? 'الحالة' : 'Status'}</th>
                  {roles.map((r) => <th key={r.id}>{r.name}</th>)}
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td>
                      <div className="smart-vehicle-name">
                        <div className="avatar" style={{ width: 30, height: 30, fontSize: 12 }}>
                          {u.displayName.slice(0, 1).toUpperCase()}
                        </div>
                        <div>
                          <strong>{u.displayName}</strong>
                          <small className="smart-mono">{u.username}</small>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span
                        className={u.isActive ? 'smart-status-badge online' : 'smart-status-badge offline'}
                        style={{ cursor: 'pointer' }}
                        onClick={() => toggleActive(u)}
                      >
                        {u.isActive ? (isArabic ? 'نشط' : 'Active') : (isArabic ? 'معطل' : 'Disabled')}
                      </span>
                    </td>
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

                {users.length === 0 && (
                  <tr>
                    <td colSpan={2 + roles.length}>
                      <div className="smart-empty">{isArabic ? 'لا يوجد مستخدمون' : 'No users found'}</div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

export default UserManagement
