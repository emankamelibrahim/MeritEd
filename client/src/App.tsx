import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import { ProtectedRoute, PublicOnlyRoute, RootRedirect } from './auth/RouteGuards'
import AdminPage from './pages/AdminPage'
import CourseDetailPage from './pages/CourseDetailPage'
import CoursesPage from './pages/CoursesPage'
import CreateCoursePage from './pages/CreateCoursePage'
import DashboardPlaceholder from './pages/DashboardPlaceholder'
import LoginPage from './pages/LoginPage'
import ManageCoursePage from './pages/ManageCoursePage'
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
            <Route path="/instructor/courses/new" element={<CreateCoursePage />} />
            <Route path="/instructor/courses/:id/manage" element={<ManageCoursePage />} />
          </Route>
          <Route element={<ProtectedRoute allow={['Admin']} />}>
            <Route path="/admin" element={<AdminPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<RootRedirect />} />
    </Routes>
  )
}