import type { CourseStatus } from '../types/course'

const styles: Record<CourseStatus, string> = {
  Open: 'bg-success/15 text-emerald-700',
  Closed: 'bg-warning/15 text-amber-700',
  Archived: 'bg-gray-200 text-muted',
}

export default function StatusChip({ status }: { status: CourseStatus }) {
  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>
      {status}
    </span>
  )
}