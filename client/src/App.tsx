import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import { ProtectedRoute, PublicOnlyRoute, RootRedirect } from './auth/RouteGuards'
import CourseDetailPage from './pages/CourseDetailPage'
import CoursesPage from './pages/CoursesPage'
import DashboardPlaceholder from './pages/DashboardPlaceholder'
import LoginPage from './pages/LoginPage'
import MyCoursesPage from './pages/MyCoursesPage'
import RegisterPage from './pages/RegisterPage'

export default function App() {
  return (
    <Routes>
      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route element={<ProtectedRoute allow={['Student', 'Instructor']} />}>
            <Route path="/courses" element={<CoursesPage />} />
            <Route path="/courses/:id" element={<CourseDetailPage />} />
            <Route path="/my-courses" element={<MyCoursesPage />} />
          </Route>

          <Route element={<ProtectedRoute allow={['Student']} />}>
            <Route path="/student" element={<DashboardPlaceholder />} />
          </Route>
          <Route element={<ProtectedRoute allow={['Instructor']} />}>
            <Route path="/instructor" element={<DashboardPlaceholder />} />
          </Route>
          <Route element={<ProtectedRoute allow={['Admin']} />}>
            <Route path="/admin" element={<DashboardPlaceholder />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<RootRedirect />} />
    </Routes>
  )
}