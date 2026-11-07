import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { CreateCoursePayload, CreateItemPayload, UpdateCoursePayload } from '../types/course'
import { coursesApi } from './courses'
import { courseKeys } from './courseQueries'

function useInvalidateCourses() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: courseKeys.all })
}

export function useCourseMutations() {
  const invalidate = useInvalidateCourses()

  const create = useMutation({
    mutationFn: (payload: CreateCoursePayload) => coursesApi.create(payload),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (v: { id: string; payload: UpdateCoursePayload }) =>
      coursesApi.update(v.id, v.payload),
    onSuccess: invalidate,
  })
  const archive = useMutation({
    mutationFn: (id: string) => coursesApi.archive(id),
    onSuccess: invalidate,
  })

  return { create, update, archive }
}

export function useSectionMutations(courseId: string) {
  const invalidate = useInvalidateCourses()

  const add = useMutation({
    mutationFn: (title: string) => coursesApi.createSection(courseId, title),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (v: { sectionId: string; title: string; isPublished: boolean }) =>
      coursesApi.updateSection(courseId, v.sectionId, { title: v.title, isPublished: v.isPublished }),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (sectionId: string) => coursesApi.deleteSection(courseId, sectionId),
    onSuccess: invalidate,
  })
  const reorder = useMutation({
    mutationFn: (ids: string[]) => coursesApi.reorderSections(courseId, ids),
    onSuccess: invalidate,
  })

  return { add, update, remove, reorder }
}

export function useItemMutations(sectionId: string) {
  const invalidate = useInvalidateCourses()

  const add = useMutation({
    mutationFn: (payload: CreateItemPayload) => coursesApi.createItem(sectionId, payload),
    onSuccess: invalidate,
  })
  const setPublished = useMutation({
    mutationFn: (v: { itemId: string; isPublished: boolean }) =>
      coursesApi.setItemPublished(sectionId, v.itemId, v.isPublished),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (itemId: string) => coursesApi.deleteItem(sectionId, itemId),
    onSuccess: invalidate,
  })
  const reorder = useMutation({
    mutationFn: (ids: string[]) => coursesApi.reorderItems(sectionId, ids),
    onSuccess: invalidate,
  })

  return { add, setPublished, remove, reorder }
}