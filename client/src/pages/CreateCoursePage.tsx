import { useNavigate } from 'react-router-dom'
import { useCourseMutations } from '../api/courseMutations'
import { useAuth } from '../auth/useAuth'
import CourseForm from '../components/CourseForm'

export default function CreateCoursePage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const { create } = useCourseMutations()

  if (!user?.isApproved) {
    return (
      <p className="max-w-xl rounded-lg bg-warning/15 p-4 text-sm text-ink">
        Your instructor account is pending admin approval. If an admin approved you recently,
        reload the page.
      </p>
    )
  }

  return (
    <>
      <h1 className="font-heading text-3xl font-extrabold text-navy">New course</h1>
      <div className="mt-6 max-w-2xl rounded-2xl bg-surface p-6 shadow-sm">
        <CourseForm
          submitLabel="Create course"
          onSubmit={async (d) => {
            const course = await create.mutateAsync({
              title: d.title,
              description: d.description,
              startDate: d.startDate,
              endDate: d.endDate,
            })
            navigate(`/instructor/courses/${course.id}/manage`)
          }}
        />
      </div>
    </>
  )
}