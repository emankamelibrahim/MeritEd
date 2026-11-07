import { useState, type FormEvent } from 'react'
import type { Course, CourseStatus, UpdateCoursePayload } from '../types/course'
import { getErrorMessages } from '../utils/errors'
import ErrorBox from './ErrorBox'
import FormField from './FormField'
import { inputClass, primaryButton } from './styles'

interface Props {
  initial?: Course
  submitLabel: string
  onSubmit: (data: UpdateCoursePayload) => Promise<unknown>
}

export default function CourseForm({ initial, submitLabel, onSubmit }: Props) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [startDate, setStartDate] = useState(initial?.startDate ?? '')
  const [endDate, setEndDate] = useState(initial?.endDate ?? '')
  const [status, setStatus] = useState<CourseStatus>(initial?.status ?? 'Open')
  const [multiplier, setMultiplier] = useState(String(initial?.recoveryMultiplier ?? 0.7))
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSaved(false)

    const m = Number(multiplier)
    const problems: string[] = []
    if (endDate && endDate < startDate) problems.push('End date must be on or after the start date.')
    if (initial && (Number.isNaN(m) || m < 0.1 || m > 1)) {
      problems.push('Recovery multiplier must be between 0.1 and 1.')
    }
    setErrors(problems)
    if (problems.length > 0) return

    setSaving(true)
    try {
      await onSubmit({
        title: title.trim(),
        description: description.trim() || null,
        startDate,
        endDate: endDate || null,
        status,
        recoveryMultiplier: m,
      })
      setSaved(true)
    } catch (err) {
      setErrors(getErrorMessages(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <FormField label="Title" required minLength={3} maxLength={200} value={title}
        onChange={(e) => setTitle(e.target.value)} />
      <label className="block">
        <span className="mb-1 block text-sm font-medium text-ink">Description</span>
        <textarea rows={4} className={inputClass} value={description}
          onChange={(e) => setDescription(e.target.value)} />
      </label>
      <div className="grid gap-4 sm:grid-cols-2">
        <FormField label="Start date" type="date" required value={startDate}
          onChange={(e) => setStartDate(e.target.value)} />
        <FormField label="End date (optional)" type="date" value={endDate}
          onChange={(e) => setEndDate(e.target.value)} />
      </div>

      {initial && (
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-ink">Status</span>
            <select className={inputClass} value={status}
              onChange={(e) => setStatus(e.target.value as CourseStatus)}>
              <option value="Open">Open (students can enroll)</option>
              <option value="Closed">Closed</option>
              <option value="Archived">Archived</option>
            </select>
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-ink">Recovery multiplier</span>
            <input className={inputClass} type="number" step="any" min={0.1} max={1}
              value={multiplier} onChange={(e) => setMultiplier(e.target.value)} />
            <span className="mt-1 block text-xs text-muted">
              Share of base XP awarded for late (recovery) work, from 0.1 to 1.
            </span>
          </label>
        </div>
      )}

      {errors.length > 0 && <ErrorBox messages={errors} />}
      {saved && <p className="text-sm text-emerald-700">Changes saved.</p>}
      <button type="submit" disabled={saving} className={primaryButton}>
        {saving ? 'Saving…' : submitLabel}
      </button>
    </form>
  )
}