import { Link } from 'react-router-dom'
import { useMyCourses } from '../api/courseQueries'
import { useAuth } from '../auth/useAuth'
import CourseCard from '../components/CourseCard'
import ErrorBox from '../components/ErrorBox'
import { getErrorMessages } from '../utils/errors'

export default function MyCoursesPage() {
  const { user } = useAuth()
  const { data, isLoading, isError, error } = useMyCourses()
  const isInstructor = user?.role === 'Instructor'

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="font-heading text-3xl font-extrabold text-navy">My courses</h1>
        {isInstructor && user?.isApproved && (
          <Link to="/instructor/courses/new"
            className="rounded-lg bg-navy px-4 py-2 font-semibold text-white hover:opacity-90">
            New course
          </Link>
        )}
      </div>
      <div className="mt-6">
        {isLoading && <p className="text-muted">Loading…</p>}
        {isError && <ErrorBox messages={getErrorMessages(error)} />}
        {data?.length === 0 && (
          <p className="text-muted">
            {isInstructor ? (
              "You haven't created any courses yet."
            ) : (
              <>
                You haven't enrolled in any courses yet.{' '}
                <Link to="/courses" className="font-medium text-navy underline">
                  Browse courses
                </Link>
              </>
            )}
          </p>
        )}
        {data && data.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.map((course) => (
              <CourseCard
                key={course.id}
                course={course}
                to={isInstructor ? `/instructor/courses/${course.id}/manage` : undefined}
              />
            ))}
          </div>
        )}
      </div>
    </>
  )
}