import { Link } from 'react-router-dom'
import type { Course } from '../types/course'
import { formatDate } from '../utils/format'
import StatusChip from './StatusChip'

export default function CourseCard({ course }: { course: Course }) {
  return (
    <Link
      to={`/courses/${course.id}`}
      className="flex flex-col rounded-2xl bg-surface p-5 shadow-sm transition hover:shadow-md"
    >
      <div className="flex items-start justify-between gap-3">
        <h2 className="font-heading text-lg font-bold text-navy">{course.title}</h2>
        <StatusChip status={course.status} />
      </div>
      <p className="mt-1 text-sm text-muted">by {course.instructorName}</p>
      <p className="mt-3 line-clamp-3 text-sm text-ink">
        {course.description || 'No description yet.'}
      </p>
      <div className="mt-auto flex items-center justify-between pt-4 text-xs text-muted">
        <span>Starts {formatDate(course.startDate)}</span>
        {course.isEnrolled && (
          <span className="rounded-full bg-gold/20 px-2 py-0.5 font-medium text-navy">
            Enrolled
          </span>
        )}
      </div>
    </Link>
  )
}