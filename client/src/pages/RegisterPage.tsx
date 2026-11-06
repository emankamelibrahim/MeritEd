
import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../api/auth'
import AuthCard from '../components/AuthCard'
import FormField from '../components/FormField'
import { getErrorMessages } from '../utils/errors'

export default function RegisterPage() {
  const navigate = useNavigate()
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<'Student' | 'Instructor'>('Student')
  const [errors, setErrors] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setErrors([])
    setSubmitting(true)
    try {
      await authApi.register({ displayName, email, password, role })
      navigate('/login', { replace: true, state: { registered: role } })
    } catch (err) {
      setErrors(getErrorMessages(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthCard
      subtitle="Create your account"
      footer={
        <>
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-navy underline">
            Log in
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="Display name" required value={displayName}
          onChange={(e) => setDisplayName(e.target.value)} autoComplete="name" />
        <FormField label="Email" type="email" required value={email}
          onChange={(e) => setEmail(e.target.value)} autoComplete="email" />
        <FormField label="Password" type="password" required minLength={8} value={password}
          onChange={(e) => setPassword(e.target.value)} autoComplete="new-password" />
        <label className="block">
          <span className="mb-1 block text-sm font-medium text-ink">I am a</span>
          <select
            value={role}
            onChange={(e) => setRole(e.target.value as 'Student' | 'Instructor')}
            className="w-full rounded-lg border border-gray-300 bg-surface px-3 py-2 text-ink outline-none focus:border-gold focus:ring-2 focus:ring-gold/30"
          >
            <option value="Student">Student</option>
            <option value="Instructor">Instructor</option>
          </select>
        </label>
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
          {submitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>
    </AuthCard>
  )
}