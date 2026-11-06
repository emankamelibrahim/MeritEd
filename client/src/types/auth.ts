export type Role = 'Student' | 'Instructor' | 'Admin'

export interface AuthUser {
  userId: string
  email: string
  displayName: string
  role: Role
  isApproved: boolean
}

export interface AuthResponse extends AuthUser {
  accessToken: string
  expiresAt: string
}

export interface RegisterPayload {
  email: string
  password: string
  displayName: string
  role: 'Student' | 'Instructor'
}