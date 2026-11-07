import { useSectionItems } from '../api/courseQueries'
import type { Section } from '../types/course'
import { getErrorMessages } from '../utils/errors'
import ContentItemRow from './ContentItemRow'
import ErrorBox from './ErrorBox'

interface Props {
  section: Section
  canViewContent: boolean
  lockedMessage?: string
}

export default function SectionCard({ section, canViewContent, lockedMessage }: Props) {
  const items = useSectionItems(section.id, canViewContent)

  return (
    <section className="rounded-2xl bg-surface p-5 shadow-sm">
      <div className="flex items-center justify-between gap-3">
        <h3 className="font-heading text-lg font-bold text-navy">{section.title}</h3>
        {!section.isPublished && (
          <span className="rounded-full bg-warning/15 px-2.5 py-0.5 text-xs font-medium text-amber-700">
            Draft
          </span>
        )}
      </div>

      {!canViewContent && (
        <p className="mt-2 text-sm text-muted">
          {section.contentItemCount} item(s)
          {lockedMessage && ` · ${lockedMessage}`}
        </p>
      )}

      {canViewContent && items.isLoading && <p className="mt-2 text-sm text-muted">Loading…</p>}
      {canViewContent && items.isError && (
        <div className="mt-2"><ErrorBox messages={getErrorMessages(items.error)} /></div>
      )}
      {canViewContent && items.data?.length === 0 && (
        <p className="mt-2 text-sm text-muted">No content in this section yet.</p>
      )}
      {canViewContent && items.data && items.data.length > 0 && (
        <ul className="mt-2 divide-y divide-gray-100">
          {items.data.map((item) => (
            <ContentItemRow key={item.id} item={item} />
          ))}
        </ul>
      )}
    </section>
  )
}