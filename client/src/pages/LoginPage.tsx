import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import AuthCard from '../components/AuthCard'
import FormField from '../components/FormField'
import { homePathFor } from '../auth/roles'
import { useAuth } from '../auth/useAuth'
import { getErrorMessages } from '../utils/errors'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const registered = (location.state as { registered?: 'Student' | 'Instructor' } | null)?.registered

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setErrors([])
    setSubmitting(true)
    try {
      const user = await login(email, password)
      navigate(homePathFor(user.role), { replace: true })
    } catch (err) {
      setErrors(getErrorMessages(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthCard
      subtitle="Log in to continue"
      footer={
        <>
          New here?{' '}
          <Link to="/register" className="font-medium text-navy underline">
            Create an account
          </Link>
        </>
      }
    >
      {registered && (
        <p className="mb-4 rounded-lg bg-success/10 p-3 text-sm text-ink">
          {registered === 'Instructor'
            ? 'Account created. An admin must approve your instructor account before you can create courses.'
            : 'Account created. Please log in.'}
        </p>
      )}
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="Email" type="email" required value={email}
          onChange={(e) => setEmail(e.target.value)} autoComplete="email" />
        <FormField label="Password" type="password" required value={password}
          onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" />
        {errors.length > 0 && (
          <ul role="alert" className="space-y-1 rounded-lg bg-danger/10 p-3 text-sm text-danger">
            {errors.map((m) => <li key={m}>{m}</li>)}
          </ul>
        )}
        <button
          type="submit"
          disabled={submitting}
          className="w-full rounded-lg bg-navy py-2 font-semibold text-white hover:opacity-90 disabled:opacity-60"
        >
          {submitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>
    </AuthCard>
  )
}