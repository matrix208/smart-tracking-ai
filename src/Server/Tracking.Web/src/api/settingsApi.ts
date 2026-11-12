import { getAccessToken } from './trackingApi'

export type LoginMediaType = 'image' | 'video'

export interface LoginMediaConfig {
  type: LoginMediaType | null
  url: string | null
}

/**
 * Public — used by the (unauthenticated) Login page.
 * Must not require a token since it's fetched before sign-in.
 */
export async function getPublicLoginMedia(): Promise<LoginMediaConfig> {
  try {
    const response = await fetch('/api/settings/login-media')
    if (!response.ok) {
      return { type: null, url: null }
    }
    return (await response.json()) as LoginMediaConfig
  } catch {
    return { type: null, url: null }
  }
}

/**
 * Admin — used by the Settings page (requires auth).
 */
export async function getLoginMedia(): Promise<LoginMediaConfig> {
  const token = getAccessToken()
  const response = await fetch('/api/settings/login-media', {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  })
  if (!response.ok) {
    throw new Error(`Failed to load login media: ${response.status}`)
  }
  return response.json() as Promise<LoginMediaConfig>
}

export async function uploadLoginMedia(file: File): Promise<LoginMediaConfig> {
  const token = getAccessToken()
  const formData = new FormData()
  formData.append('file', file)

  const response = await fetch('/api/settings/login-media/upload', {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    body: formData,
  })

  if (!response.ok) {
    let detail = `Upload failed: ${response.status}`
    try {
      const data = (await response.json()) as { message?: string }
      detail = data.message || detail
    } catch {
      // ignore
    }
    throw new Error(detail)
  }

  return response.json() as Promise<LoginMediaConfig>
}

export async function setLoginMediaUrl(
  type: LoginMediaType,
  url: string,
): Promise<LoginMediaConfig> {
  const token = getAccessToken()
  const response = await fetch('/api/settings/login-media/url', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({ type, url }),
  })

  if (!response.ok) {
    let detail = `Save failed: ${response.status}`
    try {
      const data = (await response.json()) as { message?: string }
      detail = data.message || detail
    } catch {
      // ignore
    }
    throw new Error(detail)
  }

  return response.json() as Promise<LoginMediaConfig>
}

export async function resetLoginMedia(): Promise<void> {
  const token = getAccessToken()
  const response = await fetch('/api/settings/login-media', {
    method: 'DELETE',
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  })
  if (!response.ok) {
    throw new Error(`Reset failed: ${response.status}`)
  }
}
