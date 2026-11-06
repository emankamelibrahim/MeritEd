import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { tokenStore } from './tokenStore'
import type { AuthResponse } from '../types/auth'

export const API_URL = import.meta.env.VITE_API_URL as string

export const api = axios.create({ baseURL: API_URL, withCredentials: true })

let onSessionExpired: (() => void) | null = null
export const setSessionExpiredHandler = (fn: (() => void) | null) => {
  onSessionExpired = fn
}

// One shared in-flight refresh. Refresh tokens rotate, so two parallel
// refreshes would revoke each other and log the user out.
let refreshPromise: Promise<AuthResponse> | null = null

export function refreshSession(): Promise<AuthResponse> {
  if (!refreshPromise) {
    refreshPromise = axios
      .post<AuthResponse>(`${API_URL}/api/auth/refresh`, null, { withCredentials: true })
      .then((res) => {
        tokenStore.set(res.data.accessToken)
        return res.data
      })
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}

api.interceptors.request.use((config) => {
  const token = tokenStore.get()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean }

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as RetriableConfig | undefined
    const isAuthCall = original?.url?.startsWith('/api/auth/')

    if (error.response?.status === 401 && original && !original._retry && !isAuthCall) {
      original._retry = true
      try {
        await refreshSession()
        return api(original)
      } catch {
        tokenStore.set(null)
        onSessionExpired?.()
      }
    }
    return Promise.reject(error)
  },
)   