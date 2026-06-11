# User Stories

## Authentication

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-A1 | As a user I want to register with email and password so that I can access the platform | Registration fails if email already exists. Student accounts are active immediately. Instructor accounts are pending Admin approval. |
| US-A2 | As a user I want to log in so that I can access my dashboard | Login returns a JWT token. Invalid credentials return a clear error. Token expires after 60 minutes. |
| US-A3 | As an Admin I want to approve instructor accounts so that only verified instructors can create courses | Admin sees a list of pending instructor accounts. Approving activates course creation permission. Rejecting notifies the user. |

## Course Management

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-C1 | As an instructor I want to create a course so that students can enroll and access content | Course requires title, description, and start date. Course is created in draft status. Instructor is automatically set as owner. |
| US-C2 | As an instructor I want to organize course content into sections so that students can navigate clearly | Sections require a title. Sections have an order that the instructor can change. Content items must belong to a section. |
| US-C3 | As an instructor I want to publish and unpublish content items so that students only see what is ready | Draft items are invisible to students. Published items are visible to enrolled students. Instructor can toggle status at any time. |
| US-C4 | As a student I want to browse my enrolled courses so that I can access my content | Dashboard shows all active enrollments. Each course shows current XP, identity, and progress. |

## Enrollment

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-E1 | As a student I want to self-enroll in an open course so that I can start learning | Enrollment is instant for open courses. Closed and archived courses reject enrollment with a clear message. |
| US-E2 | As an instructor I want to manually enroll a student so that I can manage course access | Instructor searches by email. Student receives access immediately. Already enrolled students are rejected with a clear message. |
| US-E3 | As an instructor I want to remove a student from my course so that I can manage enrollment | Student loses content access immediately. XP history is retained. Action is irreversible from the student's perspective. |

## Progression & XP

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-P1 | As an instructor I want to set XP values for assignments and quizzes so that I can control point distribution | Each activity has a base XP field. XP value must be a positive integer. Default value is configurable. |
| US-P2 | As a student I want to earn XP automatically when I complete a quiz so that my effort is recognized immediately | XP is awarded on submission. XP transaction is recorded with category Assessment. Student total updates immediately. |
| US-P3 | As a student I want to earn XP when my assignment is approved so that quality is rewarded | XP is awarded only after instructor approval. Student is notified when approved. XP transaction is recorded with category Assessment. |
| US-P4 | As a student I want to attempt a recovery version of a missed activity so that one bad week does not define my standing | Recovery attempt is available only after original deadline. XP awarded at reduced multiplier. Recovery XP transaction is recorded with category Recovery. |
| US-P5 | As a student I want to see my XP breakdown by category so that I understand my learning behavior | Dashboard shows total XP and XP split by category. Breakdown is visible per course. |

## Badges

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-B1 | As a student I want to earn badges automatically when I reach milestones so that my achievements are recognized | Badge is awarded without manual action. XP bonus is applied immediately. Badge appears on student profile. |
| US-B2 | As a student I want to see all my earned badges on my profile so that I can track my achievements | Profile shows badge name, description, and date earned. Unearned badges are not visible in MVP. |

## Behavioral Identity

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-I1 | As a student I want to see my current identity so that I understand how the platform perceives my learning behavior | Identity label is shown on course dashboard. XP source breakdown explaining the identity is shown alongside it. Identity updates when XP is awarded. |
| US-I2 | As an instructor I want to see the identity of each student so that I can understand class behavior patterns | Instructor dashboard shows identity alongside each student's XP total. Identity distribution across the class is visible. |

## Discussion

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-D1 | As a student I want to post a discussion thread so that I can ask questions or start conversations | Thread requires title and body. Thread is immediately visible to enrolled users. Author is recorded. |
| US-D2 | As a student I want to reply to a thread so that I can contribute to discussions | Reply is attached to the correct thread. Reply author and timestamp are recorded. |
| US-D3 | As a thread author I want to mark a reply as the accepted answer so that others know the question is resolved | Only the thread author can mark accepted answer. Only one reply can be accepted per thread. Accepted answer is visually distinguished. |

## Dashboards

| ID | User Story | Acceptance Criteria |
|---|---|---|
| US-DS1 | As a student I want a dashboard that shows my progress so that I can understand where I stand | Shows total XP, XP by category, current identity, earned badges, and enrolled courses. Updates in real time when XP is awarded. |
| US-DS2 | As an instructor I want a dashboard that shows class engagement so that I can evaluate students holistically | Shows all enrolled students with XP totals, category breakdowns, and identities. Highlights outliers above and below class median. |