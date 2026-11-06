import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute, PublicOnlyRoute, RootRedirect } from './auth/RouteGuards'
import DashboardPlaceholder from './pages/DashboardPlaceholder'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

export default function App() {
  return (
    <Routes>
      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
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

      <Route path="*" element={<RootRedirect />} />
    </Routes>
  )
} 