# Functional Requirements

## Authentication (FR-A)

| ID | Requirement |
|---|---|
| FR-A1 | A user can register with email and password |
| FR-A2 | A user can log in and receive a JWT token |
| FR-A3 | Users self-register as Student or Instructor. Instructor accounts require Admin approval before course creation is permitted. Students are active immediately on registration. |
| FR-A4 | A user has a profile with display name and avatar |
| FR-A5 | JWT tokens expire and must be refreshed |

## Course & Content (FR-C)

| ID | Requirement |
|---|---|
| FR-C1 | An instructor can create a course with title, description, and start/end dates |
| FR-C2 | A course contains sections, each section contains content items. Section membership is mandatory. |
| FR-C3 | Content items can be: Lecture (rich text), File (upload), Link (external URL), Assignment, Quiz |
| FR-C4 | An instructor can set an item as published or draft |
| FR-C5 | Students can only see published content |
| FR-C6 | An instructor can reorder sections and content items |
| FR-C7 | A course has an enrollment status: open, closed, archived |

## Enrollment (FR-E)

| ID | Requirement |
|---|---|
| FR-E1 | An instructor can enroll students into their course manually |
| FR-E2 | A student can self-enroll if the course is open |
| FR-E3 | An instructor can remove a student from a course |
| FR-E4 | A student can see all courses they are enrolled in |
| FR-E5 | An instructor can see all students enrolled in their course |

## Progression & XP (FR-P)

| ID | Requirement |
|---|---|
| FR-P1 | Each activity (assignment, quiz) has an instructor-defined base XP value |
| FR-P2 | Submitting a quiz awards full base XP automatically on submission |
| FR-P3 | Submitting an assignment awards XP on instructor approval |
| FR-P4 | A recovery version of an activity becomes available after the deadline passes |
| FR-P5 | Recovery submission awards XP at a reduced multiplier (default 0.7x, configurable per course) |
| FR-P6 | XP is never deducted — only added |
| FR-P7 | A student's total XP is the sum of all earned XP across all activities in a course |
| FR-P8 | XP is scoped per course — a student has separate XP totals per course |
| FR-P9 | The instructor can see each student's XP total and XP breakdown by category |

## Badges (FR-B)

| ID | Requirement |
|---|---|
| FR-B1 | Badges are defined at platform level with a name, description, icon, and XP bonus |
| FR-B2 | Badges are awarded automatically when trigger conditions are met |
| FR-B3 | A badge can only be earned once per student per course |
| FR-B4 | Earning a badge grants a one-time XP bonus |
| FR-B5 | A student's earned badges are visible on their profile |
| FR-B6 | Badge triggers include: XP milestones, first submission, first discussion post, identity thresholds |

## Behavioral Identity (FR-I)

| ID | Requirement |
|---|---|
| FR-I1 | Each student has a computed identity per course based on XP source distribution |
| FR-I2 | Identity is recalculated every time XP is awarded |
| FR-I3 | Identity is displayed on the student's course profile |
| FR-I4 | The identity label and its source breakdown are visible to the student |
| FR-I5 | The instructor can see the identity of each enrolled student |

## Discussion (FR-D)

| ID | Requirement |
|---|---|
| FR-D1 | Each course has a discussion board |
| FR-D2 | A student or instructor can create a discussion thread with a title and body |
| FR-D3 | Any enrolled user can reply to a thread |
| FR-D4 | The thread author can mark a reply as the accepted answer |
| FR-D5 | Threads can be tagged by topic |
| FR-D6 | No XP is awarded for discussion actions in MVP |

## Dashboards (FR-DS)

| ID | Requirement |
|---|---|
| FR-DS1 | A student dashboard shows: total XP, XP by category, current identity, earned badges, enrolled courses |
| FR-DS2 | An instructor dashboard shows: enrolled students, XP distribution across class, per-student breakdown, identity distribution |
| FR-DS3 | The instructor can identify outliers — students significantly above or below class median |