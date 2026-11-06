import type { Role } from '../types/auth'

export function homePathFor(role: Role): string {
  switch (role) {
    case 'Student':
      return '/student'
    case 'Instructor':
      return '/instructor'
    case 'Admin':
      return '/admin'
  }
}