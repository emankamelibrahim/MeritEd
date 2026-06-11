# Domain Model

This document describes the core entities in the MeritEd domain, what each one represents, what it owns, and how it relates to others. This is a conceptual model — it precedes and informs the database design but is not identical to it.

---

## Entities

### User
Represents any authenticated person on the platform.

**Owns:** email, password hash, display name, avatar, role, approval status (for instructors), registration date.

**Relationships:**
- A User can be an Instructor who owns many Courses
- A User can be a Student who has many Enrollments
- A User can author DiscussionThreads and DiscussionReplies

**Notes:** Role determines what a user can do, not what they are in the domain. A single User entity with a role discriminator is cleaner than separate Student and Instructor entities at this scale.

---

### Course
The central organizing unit of the platform. Everything else belongs to or happens within a Course.

**Owns:** title, description, start date, end date, enrollment status (open, closed, archived), recovery multiplier, created date.

**Relationships:**
- A Course belongs to one Instructor (User)
- A Course has many Sections
- A Course has many Enrollments (students)
- A Course has many DiscussionThreads

**Notes:** Enrollment status controls whether new students can join, not whether existing students can access content.

---

### Section
An organizational grouping within a Course. Used by instructors to structure content logically.

**Owns:** title, order index, published status.

**Relationships:**
- A Section belongs to one Course
- A Section has many ContentItems

**Notes:** Section membership is mandatory for all content items. Content cannot exist outside a section.

---

### ContentItem
The base concept for any piece of content inside a Section. A polymorphic entity with a type discriminator.

**Owns:** title, order index, published status, type (Lecture, File, Link, Assignment, Quiz).

**Type-specific properties:**
- **Lecture** — rich text body
- **File** — stored file reference, file size, mime type
- **Link** — external URL, description
- **Assignment** — description, due date, base XP value, allows recovery flag
- **Quiz** — questions (structured), due date, base XP value, allows recovery flag

**Relationships:**
- A ContentItem belongs to one Section
- Assignments and Quizzes can have many Activities (student submissions)

**Notes:** Implemented using EF Core Table Per Hierarchy (TPH). Single table with discriminator column. See AD-03.

---

### Enrollment
Represents the relationship between a Student and a Course. Not a simple join — it carries meaningful state.

**Owns:** enrollment date, status (active, removed), total XP, current identity label.

**Relationships:**
- An Enrollment belongs to one Student (User)
- An Enrollment belongs to one Course
- An Enrollment has many Activities
- An Enrollment has many XPTransactions
- An Enrollment has many StudentBadges

**Notes:** Total XP and identity are stored here as a performance optimization. They are always derived from and consistent with the XPTransaction ledger. See AD-04.

---

### Activity
Represents a student's submission against an Assignment or Quiz. This is the XP-earning event record.

**Owns:** submission date, is recovery flag, XP awarded, status (submitted, approved, rejected), graded date.

**Relationships:**
- An Activity belongs to one Enrollment
- An Activity belongs to one ContentItem (Assignment or Quiz)

**Notes:** ContentItem is the definition of what the activity is. Activity is the record of what the student did. One ContentItem can have many Activities across different students.

---

### XPTransaction
An immutable ledger entry recording every XP award. Never updated, never deleted.

**Owns:** amount, source category (Assessment, Recovery, Badge), source reference ID, timestamp.

**Relationships:**
- An XPTransaction belongs to one Enrollment

**Notes:** The identity system reads XP source ratios directly from this ledger. The Enrollment total XP is always the sum of all XPTransactions for that enrollment. See AD-02.

**Source categories:**
- **Assessment** — earned from on-time quiz or approved assignment
- **Recovery** — earned from recovery attempt
- **Badge** — earned from badge milestone reward

---

### Badge
A platform-defined achievement that rewards milestone behavior.

**Owns:** name, description, icon reference, XP bonus, trigger type, trigger threshold.

**Relationships:**
- A Badge can be earned by many Students (via StudentBadge)

**Trigger types (initial set):**
- XP milestone (e.g. first 100 XP)
- First submission
- First discussion post
- Identity threshold reached

**Notes:** Badges are platform-wide and defined by Admin only in MVP. See AD-08.

---

### StudentBadge
The record of a student earning a badge within a course.

**Owns:** earned date.

**Relationships:**
- A StudentBadge belongs to one Enrollment
- A StudentBadge belongs to one Badge

**Notes:** A student can earn the same badge in different courses. The uniqueness constraint is per enrollment, not per user globally.

---

### DiscussionThread
A question or topic posted within a Course discussion board.

**Owns:** title, body, tags, created date, is closed flag.

**Relationships:**
- A DiscussionThread belongs to one Course
- A DiscussionThread belongs to one author (User)
- A DiscussionThread has many DiscussionReplies

---

### DiscussionReply
A response to a DiscussionThread.

**Owns:** body, created date, is accepted answer flag.

**Relationships:**
- A DiscussionReply belongs to one DiscussionThread
- A DiscussionReply belongs to one author (User)

**Notes:** Only one reply per thread can be marked as accepted answer. Only the thread author can mark it.

---

## Entity Relationship Overview

The full ERD is available in [`/docs/design/erd.png`](/docs/design/erd.png).

The live diagram can be viewed and edited at [dbdiagram.io](https://dbdiagram.io) — link saved separately in the design folder.
---

## Identity Classification Rules

Identity is derived from the XP source distribution in the XPTransaction ledger for a given Enrollment.

| Identity | Rule | XP Category |
|---|---|---|
| Scholar | Assessment XP > 60% of total | Assessment |
| Challenger | Recovery XP > 30% of total | Recovery |
| Contributor | No single category exceeds 50% | Balanced |
| Mentor | Discussion-based — deferred to V2 | Discussion |

Rules are evaluated in priority order. The first matching rule wins.

---

## What This Model Does Not Include (By Design)

- Global XP or cross-course progression — XP is scoped per course (AD-05)
- Team or group entities — deferred to V2
- Notification entity — deferred to V2
- Course map or visual progression — deferred to V2