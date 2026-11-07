import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom'
import { useCourseMutations, useSectionMutations } from '../api/courseMutations'
import { useCourse, useSections } from '../api/courseQueries'
import { useAuth } from '../auth/useAuth'
import CourseForm from '../components/CourseForm'
import ErrorBox from '../components/ErrorBox'
import ManageSectionCard from '../components/ManageSectionCard'
import StatusChip from '../components/StatusChip'
import { inputClass, primaryButton, smallButton } from '../components/styles'
import { getErrorMessages } from '../utils/errors'
import { moved } from '../utils/reorder'

export default function ManageCoursePage() {
  const { id = '' } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const courseQuery = useCourse(id)
  const sectionsQuery = useSections(id)
  const { update, archive } = useCourseMutations()
  const sectionMutations = useSectionMutations(id)
  const [newTitle, setNewTitle] = useState('')

  if (courseQuery.isLoading) return <p className="text-muted">Loading course…</p>
  if (courseQuery.isError || !courseQuery.data || !user) {
    return <ErrorBox messages={getErrorMessages(courseQuery.error)} />
  }

  const course = courseQuery.data
  if (user.userId !== course.instructorId) return <Navigate to={`/courses/${id}`} replace />

  const actionError = sectionMutations.add.error ?? sectionMutations.reorder.error ?? archive.error

  function handleAddSection(e: FormEvent) {
    e.preventDefault()
    sectionMutations.add.mutate(newTitle.trim(), { onSuccess: () => setNewTitle('') })
  }

  function handleArchive() {
    if (window.confirm('Archive this course? It disappears from Browse. Enrolled students keep access.')) {
      archive.mutate(id, { onSuccess: () => navigate('/my-courses') })
    }
  }

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="font-heading text-3xl font-extrabold text-navy">{course.title}</h1>
          <StatusChip status={course.status} />
        </div>
        <div className="flex gap-2">
          <Link to={`/courses/${id}`} className={smallButton}>Open course page</Link>
          {course.status !== 'Archived' && (
            <button onClick={handleArchive} disabled={archive.isPending}
              className={`${smallButton} text-danger`}>
              Archive
            </button>
          )}
        </div>
      </div>

      <h2 className="mt-8 font-heading text-xl font-bold text-navy">Course details</h2>
      <div className="mt-3 rounded-2xl bg-surface p-6 shadow-sm">
        <CourseForm
          initial={course}
          submitLabel="Save changes"
          onSubmit={async (d) => {
            await update.mutateAsync({ id, payload: d })
          }}
        />
      </div>

      <h2 className="mt-8 font-heading text-xl font-bold text-navy">Sections</h2>
      <form onSubmit={handleAddSection} className="mt-3 flex gap-2">
        <input className={inputClass} placeholder="New section title" required minLength={3}
          maxLength={200} value={newTitle} onChange={(e) => setNewTitle(e.target.value)} />
        <button type="submit" disabled={sectionMutations.add.isPending}
          className={`${primaryButton} whitespace-nowrap`}>
          Add section
        </button>
      </form>

      {actionError && (
        <div className="mt-3"><ErrorBox messages={getErrorMessages(actionError)} /></div>
      )}

      <div className="mt-4 space-y-4">
        {sectionsQuery.isLoading && <p className="text-muted">Loading sections…</p>}
        {sectionsQuery.isError && <ErrorBox messages={getErrorMessages(sectionsQuery.error)} />}
        {sectionsQuery.data?.length === 0 && (
          <p className="text-muted">No sections yet. Add the first one above.</p>
        )}
        {sectionsQuery.data?.map((section, index, all) => (
          <ManageSectionCard
            key={section.id}
            section={section}
            courseId={id}
            isFirst={index === 0}
            isLast={index === all.length - 1}
            onMove={(direction) =>
              sectionMutations.reorder.mutate(moved(all, index, direction).map((s) => s.id))
            }
          />
        ))}
      </div>
    </>
  )
}