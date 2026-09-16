const API_ROOT = '/api/v1'

async function parseResponse(response) {
  if (response.status === 204) return null
  const payload = await response.json().catch(() => ({}))
  if (!response.ok) {
    const validationMessage = payload.errors ? Object.values(payload.errors).flat()[0] : null
    throw new Error(payload.message ?? validationMessage ?? payload.detail ?? payload.title ?? 'No fue posible completar la operación.')
  }
  return payload
}

export async function login(username, password) {
  const response = await fetch(`${API_ROOT}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })

  return parseResponse(response)
}

export async function apiRequest(path, token, options = {}) {
  const response = await fetch(`${API_ROOT}${path}`, {
    ...options,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  })
  return parseResponse(response)
}
