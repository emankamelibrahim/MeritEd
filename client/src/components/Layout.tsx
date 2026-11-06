import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { homePathFor } from '../auth/roles'
import { useAuth } from '../auth/useAuth'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-md px-3 py-1.5 text-sm font-medium transition ${
    isActive ? 'bg-white/15 text-white' : 'text-white/70 hover:text-white'
  }`

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  if (!user) return null

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  const home = homePathFor(user.role)
  const showCourses = user.role !== 'Admin'

  return (
    <div className="min-h-screen">
      <header className="bg-navy">
        <nav className="mx-auto flex max-w-6xl items-center gap-2 px-4 py-3">
          <Link to={home} className="mr-4 flex items-center gap-2">
            <img src="/logo-white.png" alt="" className="h-9 w-9" />
            <span className="font-heading text-xl font-extrabold text-white">
              Merit<span className="text-gold">Ed</span>
            </span>
          </Link>
          <NavLink to={home} end className={linkClass}>Dashboard</NavLink>
          {showCourses && (
            <>
              <NavLink to="/courses" className={linkClass}>Courses</NavLink>
              <NavLink to="/my-courses" className={linkClass}>My courses</NavLink>
            </>
          )}
          <div className="ml-auto flex items-center gap-3">
            <span className="hidden text-sm text-white/80 sm:inline">
              {user.displayName} · {user.role}
            </span>
            <button
              onClick={handleLogout}
              className="rounded-lg bg-gold px-3 py-1.5 text-sm font-semibold text-navy hover:opacity-90"
            >
              Log out
            </button>
          </div>
        </nav>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}