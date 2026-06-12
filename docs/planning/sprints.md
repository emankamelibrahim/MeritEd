# Sprint Plan

## Approach

- Sprints have no fixed time estimate
- A sprint ends when its done criteria are fully met
- Progress depends on available time alongside ITI program workload
- Backend sprints are completed and verified via Postman before frontend begins
- Each milestone is a LinkedIn post checkpoint and a meaningful GitHub commit

---

## Milestones

| Milestone | Description |
|---|---|
| M1 — Foundation | Auth working end to end |
| M2 — Course Structure | Instructor can build a course, student can enroll |
| M3 — Progression Core | Student can submit work and earn XP |
| M4 — Recovery and Identity | Recovery mechanic and identity system live |
| M5 — Badges and Dashboards | Complete experience for both actors |
| M6 — Discussion System | Full discussion structure without XP |
| M7 — Deployment | Live URL, portfolio ready |

---

## Sprint 1 — Project Setup and Auth Backend
**Milestone: M1**

### Goals
- Create solution with MeritEd.API and MeritEd.Core projects
- Configure PostgreSQL connection
- Configure EF Core with AppDbContext
- Implement User entity and ASP.NET Identity setup
- Register and login endpoints
- JWT generation and validation
- Refresh token endpoint
- Role-based authorization setup
- Instructor approval flag

### Done When
Postman can register a user, log in, and receive a valid JWT. Role is included in the token. Instructor account is flagged as pending until approved by Admin.

---

## Sprint 2 — Course and Content Backend
**Milestone: M2**

### Goals
- Course CRUD endpoints
- Section CRUD and reordering
- ContentItem TPH implementation
- Lecture, File, Link, Assignment, Quiz types
- Published and draft status
- Authorization checks (instructor owns course)

### Done When
Instructor can create a course with sections and content items via Postman. Draft items are not returned for student requests.

---

## Sprint 3 — Enrollment Backend
**Milestone: M2**

### Goals
- Enrollment entity and repository
- Self-enroll endpoint
- Manual enroll by instructor
- Remove student endpoint
- Enrollment status checks

### Done When
A student can enroll in an open course. An instructor can manually enroll and remove students. Closed and archived courses reject self-enrollment.

---

## Sprint 4 — Progression Core Backend
**Milestone: M3 and M4**

### Goals
- Activity submission endpoint
- Quiz auto-XP on submission
- Assignment XP on instructor approval
- XPTransaction ledger implementation
- Recovery attempt logic and multiplier
- Enrollment TotalXP update on every award
- XPService and IdentityService in MeritEd.Core
- Identity calculation from ledger ratios
- Identity update on every XP award

### Done When
A student submits a quiz and XP is awarded automatically. A student submits an assignment and XP is awarded after instructor approval. Recovery attempt awards reduced XP. Identity label updates correctly after each award. Full XP ledger is queryable.

---

## Sprint 5 — Badge System Backend
**Milestone: M5**

### Goals
- Badge entity and Admin CRUD
- StudentBadge entity
- BadgeService trigger evaluation
- Badge award on XP milestone
- Badge XP bonus transaction
- Badge uniqueness enforcement

### Done When
A student earns a badge automatically when a trigger condition is met. XP bonus is applied and recorded in the ledger. Same badge cannot be earned twice in the same course.

---

## Sprint 6 — Discussion Backend
**Milestone: M6**

### Goals
- DiscussionThread CRUD
- DiscussionReply CRUD
- Accepted answer mechanic
- Authorization checks (enrolled users only)

### Done When
A student can create a thread. Another student can reply. Thread author can mark a reply as accepted. Non-enrolled users cannot access the discussion board.

---

## Sprint 7 — Dashboard Endpoints
**Milestone: M5**

### Goals
- Student dashboard endpoint
- Instructor dashboard endpoint
- Course level dashboard
- Outlier detection logic (above and below median)

### Done When
Student dashboard endpoint returns total XP, XP by category, identity, badges, and enrolled courses. Instructor dashboard returns all students with XP totals, breakdowns, identities, and outlier flags.

---

## Sprint 8 — React Frontend Foundation
**Milestone: internal**

### Goals
- React and TypeScript project setup
- Routing structure
- Auth context and JWT storage
- Login and register pages
- Protected route logic
- API service layer setup

### Done When
A user can register, log in, and be redirected based on role in the browser. Token is stored and attached to API requests automatically.

---

## Sprint 9 — Frontend Course and Enrollment
**Milestone: M2 frontend**

### Goals
- Course listing page
- Course detail page with sections and content
- Enrollment flow
- Instructor course management UI
- Section and content item management

### Done When
A student can browse and enroll in a course in the browser. An instructor can create and manage a course with sections and content items.

---

## Sprint 10 — Frontend Progression and Identity
**Milestone: M3 and M4 frontend**

### Goals
- Assignment and quiz submission UI
- XP display and breakdown
- Identity display with source breakdown
- Recovery attempt UI
- Instructor approval UI

### Done When
The full XP and identity flow is visible and interactive in the browser. Identity label and source breakdown are displayed on the student course dashboard.

---

## Sprint 11 — Frontend Badges and Dashboards
**Milestone: M5 frontend**

### Goals
- Badge display on student profile
- Student dashboard page
- Instructor dashboard page with class metrics
- Outlier highlighting

### Done When
Both dashboards are fully functional in the browser. Badges display correctly on student profile.

---

## Sprint 12 — Frontend Discussion
**Milestone: M6 frontend**

### Goals
- Discussion board page
- Thread creation and display
- Reply flow
- Accepted answer UI

### Done When
Full discussion system works in the browser. Accepted answer is visually distinguished from other replies.

---

## Sprint 13 — Testing
**Milestone: internal**

### Goals
- Unit tests for XPService
- Unit tests for IdentityService
- Unit tests for BadgeService
- Integration tests for auth endpoints
- Integration tests for submission and XP award flow

### Done When
Critical business logic has test coverage. All tests pass. CI runs on GitHub Actions on every push.

---

## Sprint 14 — Deployment and Portfolio Preparation
**Milestone: M7**

### Goals
- Environment configuration
- Deploy backend to Railway or Render
- Deploy frontend to Vercel or Netlify
- Setup guide written
- README finalized with live URL and demo instructions
- Portfolio presentation prepared

### Done When
Live URL exists. Any person can clone the repo and run it locally following the setup guide. Project is ready to present in an interview.

---

## Feature Prioritization

### MVP
Course structure, enrollment, progression, XP ledger, recovery mechanic,
identity system, badge system, discussion structure, student dashboard,
instructor dashboard.

### V2
Discussion XP and credibility model, instructor custom badges, quiz analytics,
team projects, course map visualization, end of course recap, notification system.

### Future
Real time features, mobile app, institution level multitenancy, AI feedback.