import { Link, useParams } from 'react-router-dom'
import { useCourse, useEnrollmentMutations, useSections } from '../api/courseQueries'
import { useAuth } from '../auth/useAuth'
import ErrorBox from '../components/ErrorBox'
import SectionCard from '../components/SectionCard'
import StatusChip from '../components/StatusChip'
import { getErrorMessages } from '../utils/errors'
import { formatDate } from '../utils/format'

export default function CourseDetailPage() {
  const { id = '' } = useParams()
  const { user } = useAuth()
  const courseQuery = useCourse(id)
  const sectionsQuery = useSections(id)
  const { enroll, leave } = useEnrollmentMutations(id)

  if (courseQuery.isLoading) return <p className="text-muted">Loading course…</p>
  if (courseQuery.isError || !courseQuery.data || !user) {
    return <ErrorBox messages={getErrorMessages(courseQuery.error)} />
  }

  const course = courseQuery.data
  const isOwner = user.userId === course.instructorId
  const isStudent = user.role === 'Student'
  const canViewContent = course.isEnrolled || isOwner
  const actionError = enroll.error ?? leave.error

  const lockedMessage = !isStudent
    ? 'Content is visible to enrolled students only'
    : course.status === 'Open'
      ? 'Enroll to view the content'
      : 'Enrollment is closed'

  function handleLeave() {
    if (window.confirm('Leave this course? Your progress is kept if you rejoin later.')) {
      leave.mutate()
    }
  }

  return (
    <>
      <div className="rounded-2xl bg-surface p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <div className="flex flex-wrap items-center gap-3">
              <h1 className="font-heading text-3xl font-extrabold text-navy">{course.title}</h1>
              <StatusChip status={course.status} />
            </div>
            <p className="mt-1 text-sm text-muted">
              by {course.instructorName} · {formatDate(course.startDate)}
              {course.endDate && ` – ${formatDate(course.endDate)}`}
            </p>
          </div>

          {isOwner && (
            <Link
              to={`/instructor/courses/${course.id}/manage`}
              className="rounded-lg bg-navy px-4 py-2 font-semibold text-white hover:opacity-90"
            >
              Manage course
            </Link>
          )}

          {isStudent &&
            (course.isEnrolled ? (
              <button
                onClick={handleLeave}
                disabled={leave.isPending}
                className="rounded-lg border border-danger px-4 py-2 font-semibold text-danger hover:bg-danger/5 disabled:opacity-60"
              >
                {leave.isPending ? 'Leaving…' : 'Leave course'}
              </button>
            ) : course.status === 'Open' ? (
              <button
                onClick={() => enroll.mutate()}
                disabled={enroll.isPending}
                className="rounded-lg bg-navy px-4 py-2 font-semibold text-white hover:opacity-90 disabled:opacity-60"
              >
                {enroll.isPending ? 'Enrolling…' : 'Enroll'}
              </button>
            ) : (
              <span className="text-sm text-muted">Enrollment is closed</span>
            ))}
        </div>

        {course.description && (
          <p className="mt-4 whitespace-pre-wrap text-ink">{course.description}</p>
        )}
        {actionError && (
          <div className="mt-4">
            <ErrorBox messages={getErrorMessages(actionError)} />
          </div>
        )}
      </div>

      <h2 className="mt-8 font-heading text-xl font-bold text-navy">Syllabus</h2>
      <div className="mt-4 space-y-4">
        {sectionsQuery.isLoading && <p className="text-muted">Loading sections…</p>}
        {sectionsQuery.isError && <ErrorBox messages={getErrorMessages(sectionsQuery.error)} />}
        {sectionsQuery.data?.length === 0 && (
          <p className="text-muted">No content has been published yet.</p>
        )}
        {sectionsQuery.data?.map((section) => (
          <SectionCard
            key={section.id}
            section={section}
            canViewContent={canViewContent}
            lockedMessage={lockedMessage}
          />
        ))}
      </div>
    </>
  )
}