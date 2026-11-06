import { api } from './client'
import type { ContentItem, Course, Section } from '../types/course'

export const coursesApi = {
  browse: () => api.get<Course[]>('/api/courses').then((r) => r.data),
  mine: () => api.get<Course[]>('/api/courses/mine').then((r) => r.data),
  get: (id: string) => api.get<Course>(`/api/courses/${id}`).then((r) => r.data),
  sections: (courseId: string) =>
    api.get<Section[]>(`/api/courses/${courseId}/sections`).then((r) => r.data),
  items: (sectionId: string) =>
    api.get<ContentItem[]>(`/api/sections/${sectionId}/items`).then((r) => r.data),
  enroll: (courseId: string) => api.post(`/api/courses/${courseId}/enroll`),
  leave: (courseId: string) => api.delete(`/api/courses/${courseId}/enroll`),
}