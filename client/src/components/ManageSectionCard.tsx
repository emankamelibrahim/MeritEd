import { useState, type FormEvent } from 'react'
import { useItemMutations, useSectionMutations } from '../api/courseMutations'
import { useSectionItems } from '../api/courseQueries'
import type { Section } from '../types/course'
import { getErrorMessages } from '../utils/errors'
import { moved } from '../utils/reorder'
import AddItemForm from './AddItemForm'
import ContentItemRow from './ContentItemRow'
import ErrorBox from './ErrorBox'
import { inputClass, smallButton } from './styles'

interface Props {
  section: Section
  courseId: string
  isFirst: boolean
  isLast: boolean
  onMove: (direction: -1 | 1) => void
}

export default function ManageSectionCard({ section, courseId, isFirst, isLast, onMove }: Props) {
  const sections = useSectionMutations(courseId)
  const itemMutations = useItemMutations(section.id)
  const items = useSectionItems(section.id, true)
  const [editing, setEditing] = useState(false)
  const [title, setTitle] = useState(section.title)
  const [adding, setAdding] = useState(false)

  const list = items.data ?? []
  const actionError =
    sections.update.error ??
    sections.remove.error ??
    itemMutations.setPublished.error ??
    itemMutations.remove.error ??
    itemMutations.reorder.error

  function saveTitle(e: FormEvent) {
    e.preventDefault()
    sections.update.mutate(
      { sectionId: section.id, title: title.trim(), isPublished: section.isPublished },
      { onSuccess: () => setEditing(false) },
    )
  }

  function togglePublish() {
    sections.update.mutate({
      sectionId: section.id,
      title: section.title,
      isPublished: !section.isPublished,
    })
  }

  function removeSection() {
    if (window.confirm('Delete this section, all its items and any student submissions for them?')) {
      sections.remove.mutate(section.id)
    }
  }

  function removeItem(itemId: string) {
    if (window.confirm('Delete this item and any student submissions for it?')) {
      itemMutations.remove.mutate(itemId)
    }
  }

  return (
    <section className="rounded-2xl bg-surface p-5 shadow-sm">
      <div className="flex flex-wrap items-center gap-2">
        {editing ? (
          <form onSubmit={saveTitle} className="flex flex-1 items-center gap-2">
            <input className={inputClass} required minLength={3} maxLength={200}
              value={title} onChange={(e) => setTitle(e.target.value)} />
            <button type="submit" className={smallButton} disabled={sections.update.isPending}>
              Save
            </button>
            <button type="button" className={smallButton}
              onClick={() => { setEditing(false); setTitle(section.title) }}>
              Cancel
            </button>
          </form>
        ) : (
          <>
            <h3 className="font-heading text-lg font-bold text-navy">{section.title}</h3>
            <span
              className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                section.isPublished ? 'bg-success/15 text-emerald-700' : 'bg-warning/15 text-amber-700'
              }`}
            >
              {section.isPublished ? 'Published' : 'Draft'}
            </span>
            <div className="ml-auto flex flex-wrap items-center gap-1">
              <button onClick={togglePublish} disabled={sections.update.isPending}
                className={
                  section.isPublished
                    ? smallButton
                    : 'rounded-md bg-gold px-3 py-1 text-xs font-semibold text-navy hover:opacity-90 disabled:opacity-60'
                }>
                {section.isPublished ? 'Unpublish' : 'Publish section'}
              </button>
              <button className={smallButton} aria-label="Move section up"
                disabled={isFirst} onClick={() => onMove(-1)}>▲</button>
              <button className={smallButton} aria-label="Move section down"
                disabled={isLast} onClick={() => onMove(1)}>▼</button>
              <button className={smallButton} onClick={() => setEditing(true)}>Rename</button>
              <button className={`${smallButton} text-danger`} onClick={removeSection}>Delete</button>
            </div>
          </>
        )}
      </div>

      {!section.isPublished && (
        <p className="mt-2 text-sm text-amber-700">
          Students can't see this section or its items until it's published.
        </p>
      )}

      {actionError && (
        <div className="mt-3"><ErrorBox messages={getErrorMessages(actionError)} /></div>
      )}

      {items.isLoading && <p className="mt-3 text-sm text-muted">Loading…</p>}
      {items.isError && (
        <div className="mt-3"><ErrorBox messages={getErrorMessages(items.error)} /></div>
      )}
      {items.isSuccess && list.length === 0 && (
        <p className="mt-3 text-sm text-muted">No items yet.</p>
      )}
      {list.length > 0 && (
        <ul className="mt-3 divide-y divide-gray-100">
          {list.map((item, index) => (
            <ContentItemRow
              key={item.id}
              item={item}
              actions={
                <>
                  <button className={smallButton} aria-label="Move item up"
                    disabled={index === 0 || itemMutations.reorder.isPending}
                    onClick={() =>
                      itemMutations.reorder.mutate(moved(list, index, -1).map((i) => i.id))
                    }>▲</button>
                  <button className={smallButton} aria-label="Move item down"
                    disabled={index === list.length - 1 || itemMutations.reorder.isPending}
                    onClick={() =>
                      itemMutations.reorder.mutate(moved(list, index, 1).map((i) => i.id))
                    }>▼</button>
                  <button className={smallButton}
                    disabled={itemMutations.setPublished.isPending}
                    onClick={() =>
                      itemMutations.setPublished.mutate({
                        itemId: item.id,
                        isPublished: !item.isPublished,
                      })
                    }>
                    {item.isPublished ? 'Unpublish' : 'Publish'}
                  </button>
                  <button className={`${smallButton} text-danger`}
                    onClick={() => removeItem(item.id)}>Delete</button>
                </>
              }
            />
          ))}
        </ul>
      )}

      {adding ? (
        <AddItemForm sectionId={section.id} onClose={() => setAdding(false)} />
      ) : (
        <button className={`${smallButton} mt-3`} onClick={() => setAdding(true)}>
          + Add content
        </button>
      )}
    </section>
  )
}