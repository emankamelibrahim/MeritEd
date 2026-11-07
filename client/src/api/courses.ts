import { api } from './client'
import type {
  ContentItem,
  Course,
  CreateCoursePayload,
  CreateItemPayload,
  Section,
  UpdateCoursePayload,
} from '../types/course'

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

  create: (payload: CreateCoursePayload) =>
    api.post<Course>('/api/courses', payload).then((r) => r.data),
  update: (id: string, payload: UpdateCoursePayload) =>
    api.put<Course>(`/api/courses/${id}`, payload).then((r) => r.data),
  archive: (id: string) => api.delete(`/api/courses/${id}`),

  createSection: (courseId: string, title: string) =>
    api.post<Section>(`/api/courses/${courseId}/sections`, { title }).then((r) => r.data),
  updateSection: (courseId: string, sectionId: string, payload: { title: string; isPublished: boolean }) =>
    api.put(`/api/courses/${courseId}/sections/${sectionId}`, payload),
  deleteSection: (courseId: string, sectionId: string) =>
    api.delete(`/api/courses/${courseId}/sections/${sectionId}`),
  reorderSections: (courseId: string, orderedIds: string[]) =>
    api.put(`/api/courses/${courseId}/sections/reorder`, { orderedIds }),

  createItem: (sectionId: string, payload: CreateItemPayload) =>
    api.post<ContentItem>(`/api/sections/${sectionId}/items`, payload).then((r) => r.data),
  setItemPublished: (sectionId: string, itemId: string, isPublished: boolean) =>
    api.put(`/api/sections/${sectionId}/items/${itemId}/publish`, { isPublished }),
  deleteItem: (sectionId: string, itemId: string) =>
    api.delete(`/api/sections/${sectionId}/items/${itemId}`),
  reorderItems: (sectionId: string, orderedIds: string[]) =>
    api.put(`/api/sections/${sectionId}/items/reorder`, { orderedIds }),
}