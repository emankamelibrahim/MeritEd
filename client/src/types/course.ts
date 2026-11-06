export type CourseStatus = 'Open' | 'Closed' | 'Archived'
export type ContentItemType = 'Lecture' | 'File' | 'Link' | 'Assignment' | 'Quiz'

export interface Course {
  id: string
  title: string
  description: string | null
  startDate: string
  endDate: string | null
  status: CourseStatus
  recoveryMultiplier: number
  instructorId: string
  instructorName: string
  isEnrolled: boolean
  createdAt: string
}

export interface Section {
  id: string
  courseId: string
  title: string
  orderIndex: number
  isPublished: boolean
  createdAt: string
  contentItemCount: number
}

export interface ContentItem {
  id: string
  sectionId: string
  type: ContentItemType
  title: string
  orderIndex: number
  isPublished: boolean
  createdAt: string
  body: string | null
  fileUrl: string | null
  fileSize: number | null
  mimeType: string | null
  externalUrl: string | null
  description: string | null
  dueDate: string | null
  baseXP: number | null
  allowsRecovery: boolean | null
  questions: string | null
}