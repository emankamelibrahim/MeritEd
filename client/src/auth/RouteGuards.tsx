import { Navigate, Outlet } from 'react-router-dom'
import type { Role } from '../types/auth'
import { homePathFor } from './roles'
import { useAuth } from './useAuth'

function Loading() {
  return <div className="flex min-h-screen items-center justify-center text-muted">Loading…</div>
}

export function ProtectedRoute({ allow }: { allow?: Role[] }) {
  const { user, isLoading } = useAuth()
  if (isLoading) return <Loading />
  if (!user) return <Navigate to="/login" replace />
  if (allow && !allow.includes(user.role)) return <Navigate to={homePathFor(user.role)} replace />
  return <Outlet />
}

export function PublicOnlyRoute() {
  const { user, isLoading } = useAuth()
  if (isLoading) return <Loading />
  if (user) return <Navigate to={homePathFor(user.role)} replace />
  return <Outlet />
}

export function RootRedirect() {
  const { user, isLoading } = useAuth()
  if (isLoading) return <Loading />
  return <Navigate to={user ? homePathFor(user.role) : '/login'} replace />
}