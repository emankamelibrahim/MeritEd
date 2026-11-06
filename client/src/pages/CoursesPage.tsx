import { useBrowseCourses } from '../api/courseQueries'
import CourseCard from '../components/CourseCard'
import ErrorBox from '../components/ErrorBox'
import { getErrorMessages } from '../utils/errors'

export default function CoursesPage() {
  const { data, isLoading, isError, error } = useBrowseCourses()

  return (
    <>
      <h1 className="font-heading text-3xl font-extrabold text-navy">Browse courses</h1>
      <div className="mt-6">
        {isLoading && <p className="text-muted">Loading courses…</p>}
        {isError && <ErrorBox messages={getErrorMessages(error)} />}
        {data?.length === 0 && <p className="text-muted">No open courses yet.</p>}
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