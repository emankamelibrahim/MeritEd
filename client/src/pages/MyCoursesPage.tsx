import { Link } from 'react-router-dom'
import { useMyCourses } from '../api/courseQueries'
import { useAuth } from '../auth/useAuth'
import CourseCard from '../components/CourseCard'
import ErrorBox from '../components/ErrorBox'
import { getErrorMessages } from '../utils/errors'

export default function MyCoursesPage() {
  const { user } = useAuth()
  const { data, isLoading, isError, error } = useMyCourses()

  return (
    <>
      <h1 className="font-heading text-3xl font-extrabold text-navy">My courses</h1>
      <div className="mt-6">
        {isLoading && <p className="text-muted">Loading…</p>}
        {isError && <ErrorBox messages={getErrorMessages(error)} />}
        {data?.length === 0 && (
          <p className="text-muted">
            {user?.role === 'Instructor' ? (
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
              <CourseCard key={course.id} course={course} />
            ))}
          </div>
        )}
      </div>
    </>
  )
}