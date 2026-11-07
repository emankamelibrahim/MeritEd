import { api } from './client'

export interface PendingInstructor {
  id: string
  email: string
  displayName: string
  createdAt: string
}

export const usersApi = {
  pending: () => api.get<PendingInstructor[]>('/api/users/pending').then((r) => r.data),
  approve: (id: string) => api.put(`/api/users/${id}/approve`),
}