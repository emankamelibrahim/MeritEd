import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { coursesApi } from './courses'

export const courseKeys = {
  all: ['courses'] as const,
  browse: () => [...courseKeys.all, 'browse'] as const,
  mine: () => [...courseKeys.all, 'mine'] as const,
  detail: (id: string) => [...courseKeys.all, 'detail', id] as const,
  sections: (id: string) => [...courseKeys.all, 'sections', id] as const,
  items: (sectionId: string) => [...courseKeys.all, 'items', sectionId] as const,
}

export const useBrowseCourses = () =>
  useQuery({ queryKey: courseKeys.browse(), queryFn: coursesApi.browse })

export const useMyCourses = () =>
  useQuery({ queryKey: courseKeys.mine(), queryFn: coursesApi.mine })

export const useCourse = (id: string) =>
  useQuery({ queryKey: courseKeys.detail(id), queryFn: () => coursesApi.get(id) })

export const useSections = (courseId: string) =>
  useQuery({ queryKey: courseKeys.sections(courseId), queryFn: () => coursesApi.sections(courseId) })

export const useSectionItems = (sectionId: string, enabled: boolean) =>
  useQuery({
    queryKey: courseKeys.items(sectionId),
    queryFn: () => coursesApi.items(sectionId),
    enabled,
  })

export function useEnrollmentMutations(courseId: string) {
  const queryClient = useQueryClient()
  const refresh = () => queryClient.invalidateQueries({ queryKey: courseKeys.all })

  const enroll = useMutation({ mutationFn: () => coursesApi.enroll(courseId), onSuccess: refresh })
  const leave = useMutation({ mutationFn: () => coursesApi.leave(courseId), onSuccess: refresh })

  return { enroll, leave }
}