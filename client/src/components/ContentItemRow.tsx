import type { ContentItem } from '../types/course'
import { formatDate, safeUrl } from '../utils/format'

export default function ContentItemRow({ item }: { item: ContentItem }) {
  const graded = item.type === 'Assignment' || item.type === 'Quiz'
  const url =
    item.type === 'Link' ? safeUrl(item.externalUrl)
    : item.type === 'File' ? safeUrl(item.fileUrl)
    : null

  return (
    <li className="py-3">
      <div className="flex flex-wrap items-center gap-2">
        <span
          className={`rounded-md px-2 py-0.5 text-xs font-semibold ${
            graded ? 'bg-gold/20 text-navy' : 'bg-navy/10 text-navy'
          }`}
        >
          {item.type}
        </span>
        {url ? (
          <a href={url} target="_blank" rel="noopener noreferrer"
            className="font-medium text-navy underline">
            {item.title}
          </a>
        ) : (
          <span className="font-medium text-ink">{item.title}</span>
        )}
        {!item.isPublished && (
          <span className="rounded-full bg-warning/15 px-2 py-0.5 text-xs font-medium text-amber-700">
            Draft
          </span>
        )}
      </div>
      {graded && (
        <p className="mt-1 text-sm text-muted">
          {item.baseXP} XP{item.dueDate && ` · due ${formatDate(item.dueDate)}`}
        </p>
      )}
      {item.description && <p className="mt-1 text-sm text-ink">{item.description}</p>}
      {item.type === 'Lecture' && item.body && (
        <details className="mt-2">
          <summary className="cursor-pointer text-sm font-medium text-navy">Read lecture</summary>
          <p className="mt-2 whitespace-pre-wrap text-sm text-ink">{item.body}</p>
        </details>
      )}
    </li>
  )
}