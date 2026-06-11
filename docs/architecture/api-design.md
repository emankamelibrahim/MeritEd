# API Design

## Conventions

- RESTful resource-based routing
- Base URL: `/api`
- All requests and responses use JSON
- Authentication: `Authorization: Bearer <token>` header on all protected endpoints
- Consistent error response format across all endpoints

## Error Response Format

```json
{
  "status": 400,
  "error": "Validation failed",
  "details": [
    "Title is required",
    "Base XP must be a positive integer"
  ]
}
```

## Endpoints

---

### Auth

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | Public |
| POST | `/api/auth/login` | Login and receive tokens | Public |
| POST | `/api/auth/refresh` | Refresh access token | Public |
| POST | `/api/auth/logout` | Invalidate refresh token | Required |

---

### Users

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/users/me` | Get current user profile | Required |
| PUT | `/api/users/me` | Update current user profile | Required |
| GET | `/api/users/pending` | Get pending instructor approvals | Admin |
| PUT | `/api/users/{id}/approve` | Approve instructor account | Admin |

---

### Courses

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/courses` | List all open courses | Required |
| POST | `/api/courses` | Create a new course | Instructor |
| GET | `/api/courses/{id}` | Get course details | Required |
| PUT | `/api/courses/{id}` | Update course details | Instructor (owner) |
| DELETE | `/api/courses/{id}` | Archive a course | Instructor (owner) |

---

### Sections

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/courses/{courseId}/sections` | List sections in a course | Enrolled/Instructor |
| POST | `/api/courses/{courseId}/sections` | Create a section | Instructor (owner) |
| PUT | `/api/courses/{courseId}/sections/{id}` | Update a section | Instructor (owner) |
| DELETE | `/api/courses/{courseId}/sections/{id}` | Delete a section | Instructor (owner) |
| PUT | `/api/courses/{courseId}/sections/reorder` | Reorder sections | Instructor (owner) |

---

### Content Items

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/sections/{sectionId}/items` | List content items in a section | Enrolled/Instructor |
| POST | `/api/sections/{sectionId}/items` | Create a content item | Instructor (owner) |
| GET | `/api/sections/{sectionId}/items/{id}` | Get content item details | Enrolled/Instructor |
| PUT | `/api/sections/{sectionId}/items/{id}` | Update a content item | Instructor (owner) |
| DELETE | `/api/sections/{sectionId}/items/{id}` | Delete a content item | Instructor (owner) |
| PUT | `/api/sections/{sectionId}/items/reorder` | Reorder content items | Instructor (owner) |

---

### Enrollments

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/courses/{courseId}/enroll` | Self-enroll in a course | Student |
| DELETE | `/api/courses/{courseId}/enroll` | Leave a course | Student |
| GET | `/api/courses/{courseId}/enrollments` | List enrolled students | Instructor (owner) |
| POST | `/api/courses/{courseId}/enrollments` | Manually enroll a student | Instructor (owner) |
| DELETE | `/api/courses/{courseId}/enrollments/{userId}` | Remove a student | Instructor (owner) |

---

### Activities (Submissions)

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/items/{itemId}/submit` | Submit an assignment or quiz | Student |
| GET | `/api/courses/{courseId}/activities` | List all submissions in a course | Instructor (owner) |
| GET | `/api/items/{itemId}/activities` | List submissions for a content item | Instructor (owner) |
| PUT | `/api/activities/{id}/approve` | Approve a submission and award XP | Instructor (owner) |
| PUT | `/api/activities/{id}/reject` | Reject a submission | Instructor (owner) |

---

### XP & Progression

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/courses/{courseId}/my-progress` | Get current student XP and identity | Student |
| GET | `/api/courses/{courseId}/enrollments/{userId}/progress` | Get a specific student's progress | Instructor (owner) |
| GET | `/api/courses/{courseId}/my-progress/transactions` | Get full XP transaction ledger | Student |

---

### Badges

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/badges` | List all platform badges | Required |
| POST | `/api/badges` | Create a badge | Admin |
| PUT | `/api/badges/{id}` | Update a badge | Admin |
| DELETE | `/api/badges/{id}` | Delete a badge | Admin |
| GET | `/api/courses/{courseId}/my-badges` | Get student earned badges in course | Student |

---

### Discussions

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/courses/{courseId}/discussions` | List discussion threads | Enrolled/Instructor |
| POST | `/api/courses/{courseId}/discussions` | Create a thread | Enrolled/Instructor |
| GET | `/api/courses/{courseId}/discussions/{id}` | Get thread with replies | Enrolled/Instructor |
| PUT | `/api/courses/{courseId}/discussions/{id}` | Update a thread | Thread author |
| DELETE | `/api/courses/{courseId}/discussions/{id}` | Delete a thread | Thread author/Instructor |
| POST | `/api/courses/{courseId}/discussions/{id}/replies` | Post a reply | Enrolled/Instructor |
| PUT | `/api/discussions/{threadId}/replies/{id}/accept` | Mark reply as accepted answer | Thread author |

---

### Dashboards

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/dashboard/student` | Get student dashboard data | Student |
| GET | `/api/dashboard/instructor` | Get instructor dashboard data | Instructor |
| GET | `/api/courses/{courseId}/dashboard` | Get course-level dashboard | Instructor (owner) |