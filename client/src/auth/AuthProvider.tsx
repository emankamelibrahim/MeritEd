import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { refreshSession, setSessionExpiredHandler } from '../api/client'
import { tokenStore } from '../api/tokenStore'
import type { AuthResponse, AuthUser } from '../types/auth'
import { AuthContext } from './AuthContext'

const toUser = (r: AuthResponse): AuthUser => ({
  userId: r.userId,
  email: r.email,
  displayName: r.displayName,
  role: r.role,
  isApproved: r.isApproved,
})

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let active = true
    refreshSession()
      .then((res) => {
        if (active) setUser(toUser(res))
      })
      .catch(() => {})
      .finally(() => {
        if (active) setIsLoading(false)
      })
    return () => {
      active = false
    }
  }, [])

  useEffect(() => {
    setSessionExpiredHandler(() => {
      queryClient.clear()
      setUser(null)
    })
    return () => setSessionExpiredHandler(null)
  }, [queryClient])

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await authApi.login(email, password)
      tokenStore.set(res.accessToken)
      queryClient.clear()
      const loggedIn = toUser(res)
      setUser(loggedIn)
      return loggedIn
    },
    [queryClient],
  )

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } catch {
      // clear local state even if the server call fails
    }
    tokenStore.set(null)
    queryClient.clear()
    setUser(null)
  }, [queryClient])

  const value = useMemo(
    () => ({ user, isLoading, login, logout }),
    [user, isLoading, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}