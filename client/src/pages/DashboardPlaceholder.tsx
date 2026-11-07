import { Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function DashboardPlaceholder() {
  const { user } = useAuth()
  if (!user) return null

  return (
    <div className="max-w-2xl">
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
      {user.role === 'Instructor' && user.isApproved && (
        <div className="mt-6 flex gap-3">
          <Link to="/instructor/courses/new"
            className="rounded-lg bg-navy px-4 py-2 font-semibold text-white hover:opacity-90">
            Create a course
          </Link>
          <Link to="/my-courses"
            className="rounded-lg border border-navy px-4 py-2 font-semibold text-navy hover:bg-navy/5">
            My courses
          </Link>
        </div>
      )}
    </div>
  )
}