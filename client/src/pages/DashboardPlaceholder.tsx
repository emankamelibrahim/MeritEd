import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function DashboardPlaceholder() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  if (!user) return null

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-16">
      <h1 className="font-heading text-3xl font-extrabold text-navy">
        Welcome, {user.displayName}
      </h1>
      <p className="mt-2 text-muted">
        Signed in as {user.email} · <span className="font-medium text-ink">{user.role}</span>
      </p>
      {user.role === 'Instructor' && !user.isApproved && (
        <p className="mt-6 rounded-lg bg-warning/15 p-4 text-sm text-ink">
          Your instructor account is pending admin approval. You can't create courses yet.
        </p>
      )}
      <button
        onClick={handleLogout}
        className="mt-8 rounded-lg bg-gold px-4 py-2 font-semibold text-navy hover:opacity-90"
      >
        Log out
      </button>
    </div>
  )
}