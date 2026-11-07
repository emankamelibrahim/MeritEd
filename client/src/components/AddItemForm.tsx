import { useState, type FormEvent } from 'react'
import { useItemMutations } from '../api/courseMutations'
import type { ContentItemType, CreateItemPayload } from '../types/course'
import { getErrorMessages } from '../utils/errors'
import { safeUrl } from '../utils/format'
import ErrorBox from './ErrorBox'
import FormField from './FormField'
import { inputClass, primaryButton, smallButton } from './styles'

const TYPES: ContentItemType[] = ['Lecture', 'File', 'Link', 'Assignment', 'Quiz']

interface Props {
  sectionId: string
  onClose: () => void
}

export default function AddItemForm({ sectionId, onClose }: Props) {
  const { add } = useItemMutations(sectionId)
  const [type, setType] = useState<ContentItemType>('Lecture')
  const [title, setTitle] = useState('')
  const [body, setBody] = useState('')
  const [url, setUrl] = useState('')
  const [description, setDescription] = useState('')
  const [dueDate, setDueDate] = useState('')
  const [baseXP, setBaseXP] = useState('100')
  const [allowsRecovery, setAllowsRecovery] = useState(true)
  const [questions, setQuestions] = useState('')
  const [localErrors, setLocalErrors] = useState<string[]>([])

  const graded = type === 'Assignment' || type === 'Quiz'

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const errors: string[] = []
    const payload: CreateItemPayload = { type, title: title.trim() }

    if (type === 'Lecture') payload.body = body

    if (type === 'File' || type === 'Link') {
      if (!safeUrl(url.trim())) errors.push('Enter a valid http(s) URL.')
      if (type === 'File') payload.fileUrl = url.trim()
      else payload.externalUrl = url.trim()
    }

    if ((type === 'Link' || type === 'Assignment') && description.trim()) {
      payload.description = description.trim()
    }

    if (graded) {
      const xp = Number(baseXP)
      if (!Number.isInteger(xp) || xp < 0) errors.push('Base XP must be a whole number, 0 or more.')
      payload.baseXP = xp
      payload.allowsRecovery = allowsRecovery
      if (dueDate) payload.dueDate = new Date(dueDate).toISOString()
    }

    if (type === 'Quiz' && questions.trim()) {
      try {
        JSON.parse(questions)
        payload.questions = questions
      } catch {
        errors.push('Questions must be valid JSON.')
      }
    }

    setLocalErrors(errors)
    if (errors.length > 0) return
    add.mutate(payload, { onSuccess: onClose })
  }

  const allErrors = [...localErrors, ...(add.error ? getErrorMessages(add.error) : [])]

  return (
    <form onSubmit={handleSubmit}
      className="mt-3 space-y-3 rounded-xl border border-gray-200 bg-background p-4">
      <label className="block">
        <span className="mb-1 block text-sm font-medium text-ink">Type</span>
        <select className={inputClass} value={type}
          onChange={(e) => setType(e.target.value as ContentItemType)}>
          {TYPES.map((t) => (
            <option key={t}>{t}</option>
          ))}
        </select>
      </label>

      <FormField label="Title" required minLength={3} maxLength={200} value={title}
        onChange={(e) => setTitle(e.target.value)} />

      {type === 'Lecture' && (
        <label className="block">
          <span className="mb-1 block text-sm font-medium text-ink">Lecture text</span>
          <textarea rows={5} className={inputClass} value={body}
            onChange={(e) => setBody(e.target.value)} />
        </label>
      )}

      {(type === 'File' || type === 'Link') && (
        <FormField label={type === 'File' ? 'File URL' : 'Link URL'} type="url" required
          placeholder="https://" value={url} onChange={(e) => setUrl(e.target.value)} />
      )}

      {(type === 'Link' || type === 'Assignment') && (
        <label className="block">
          <span className="mb-1 block text-sm font-medium text-ink">Description (optional)</span>
          <textarea rows={3} className={inputClass} value={description}
            onChange={(e) => setDescription(e.target.value)} />
        </label>
      )}

      {graded && (
        <div className="grid gap-3 sm:grid-cols-2">
          <FormField label="Base XP" type="number" min={0} step={1} required value={baseXP}
            onChange={(e) => setBaseXP(e.target.value)} />
          <FormField label="Due date (optional)" type="datetime-local" value={dueDate}
            onChange={(e) => setDueDate(e.target.value)} />
          <label className="flex items-center gap-2 text-sm text-ink sm:col-span-2">
            <input type="checkbox" checked={allowsRecovery}
              onChange={(e) => setAllowsRecovery(e.target.checked)} />
            Allow recovery after the due date
          </label>
        </div>
      )}

      {type === 'Quiz' && (
        <label className="block">
          <span className="mb-1 block text-sm font-medium text-ink">Questions (JSON, optional)</span>
          <textarea rows={4} className={`${inputClass} font-mono text-xs`} value={questions}
            placeholder='[{"question":"What is a variable?","options":["A","B"],"answer":0}]'
            onChange={(e) => setQuestions(e.target.value)} />
        </label>
      )}

      {allErrors.length > 0 && <ErrorBox messages={allErrors} />}

      <div className="flex gap-2">
        <button type="submit" disabled={add.isPending} className={primaryButton}>
          {add.isPending ? 'Adding…' : 'Add item'}
        </button>
        <button type="button" onClick={onClose} className={smallButton}>
          Cancel
        </button>
      </div>
    </form>
  )
}