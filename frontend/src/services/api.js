const API_ROOT = '/api/v1'

export async function login(username, password) {
  const response = await fetch(`${API_ROOT}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })

  if (!response.ok) {
    const payload = await response.json().catch(() => ({}))
    throw new Error(payload.message ?? 'No fue posible iniciar sesión.')
  }

  return response.json()
}

