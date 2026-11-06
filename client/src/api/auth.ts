import { api } from './client'
import type { AuthResponse, RegisterPayload } from '../types/auth'

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { email, password }).then((r) => r.data),

  register: (payload: RegisterPayload) =>
    api.post('/api/auth/register', payload).then((r) => r.data),

  logout: () => api.post('/api/auth/logout'),
}