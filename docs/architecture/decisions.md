# Architecture Decisions and Tradeoffs

This document records every significant architectural decision made during the design of MeritEd, including the reasoning and tradeoffs considered. This is a living document — updated as new decisions are made during implementation.

---

## AD-01: Accumulative XP with No Deduction

**Decision:** XP only increases. It is never deducted under any circumstance.

**Reasoning:** Deducting XP punishes students for setbacks, which contradicts the platform's recovery-friendly philosophy. A student who fails an assignment and completes a recovery challenge should feel rewarded for persistence, not penalized for the original failure.

**Tradeoff:** Students cannot be penalized for poor behavior (e.g. late submissions). This is intentional — the recovery multiplier handles reduced reward without punishment.

---

## AD-02: XP Ledger Instead of Running Total

**Decision:** Every XP award is recorded as an immutable XPTransaction entry. The running total on the Enrollment record is derived from and always consistent with the ledger.

**Reasoning:**
- Auditable — every XP value can be fully explained by replaying the ledger
- Recalculable — if business rules change, the ledger can be replayed with new rules
- Identity system reads XP source ratios directly from the ledger without additional storage
- Demonstrates event sourcing principles — a strong portfolio and interview signal

**Tradeoff:** Slightly more complex than storing a single integer. The performance cost is negligible at this scale.

---

## AD-03: ContentItem Polymorphism via Table Per Hierarchy (TPH)

**Decision:** All content item types (Lecture, File, Link, Assignment, Quiz) are stored in a single ContentItems table with a Type discriminator column. Unused columns are null for irrelevant types.

**Reasoning:** EF Core supports TPH natively and cleanly. At MVP scale the null columns are not a problem. Querying is simpler — one table, no joins for type resolution.

**Tradeoff:** As types grow and diverge significantly in their columns, the table becomes wide and sparse. TPT (Table Per Type) or TPC (Table Per Concrete Type) can be migrated to later if needed.

**Alternative considered:** Separate tables per type (TPT). Rejected for MVP due to increased join complexity and migration overhead.

---

## AD-04: Identity Stored on Enrollment, Calculated from Ledger

**Decision:** The student's current identity label is stored as a field on the Enrollment record. It is recalculated and updated every time XP is awarded.

**Reasoning:** Recalculating identity from the full XP ledger on every dashboard load is wasteful. Storing it denormalized on Enrollment gives O(1) reads while keeping the ledger as the source of truth.

**Tradeoff:** If identity rules change, existing stored identities may be stale until next XP award. An admin re-calculation job can address this in V2.

---

## AD-05: XP Scoped Per Course

**Decision:** A student's XP total and identity are calculated and stored per course, not globally across the platform.

**Reasoning:** A student's behavior in a programming course is independent of their behavior in a design course. Global XP would dilute the signal quality of the identity system.

**Tradeoff:** No global student ranking or cross-course progression. This is intentional for MVP.

---

## AD-06: Recovery Multiplier Configurable Per Course

**Decision:** The recovery XP multiplier defaults to 0.7 but can be configured by the instructor per course.

**Reasoning:** Different courses have different policies. A strict instructor may want 0.5x. A lenient one may want 0.9x. Hardcoding a platform value removes instructor agency.

**Tradeoff:** Adds a configuration field to the Course entity. Minimal complexity cost for meaningful flexibility.

---

## AD-07: Discussion XP Deferred

**Decision:** The discussion system is built structurally in MVP but no XP is awarded for discussion actions.

**Reasoning:** A flawed credibility model is worse than no credibility model. Awarding XP for discussion posts without a quality signal creates a farming exploit. The right model requires more design thinking before implementation.

**Tradeoff:** Discussion participation is not rewarded in MVP. Students who contribute heavily to discussions will not see this reflected in their identity until V2.

---

## AD-08: Platform-Wide Badges Only in MVP

**Decision:** Badges are defined at platform level by Admin only. Instructors cannot create custom badges in MVP.

**Reasoning:** Keeps the badge system simple and consistent across courses. Instructor-custom badges introduce complexity around badge ownership, scope, and display.

**Tradeoff:** Instructors cannot tailor badge incentives to their specific course. Planned for V2.

---

## AD-09: Authorization Enforced at API Layer

**Decision:** All role-based authorization is enforced in the ASP.NET Core API using policy-based authorization. Frontend restrictions are supplementary only and never trusted as security boundaries.

**Reasoning:** The frontend can be bypassed. Security must live in the backend. Any authorization check that exists only in React is not a real authorization check.

**Tradeoff:** Requires consistent application of authorization attributes across all controllers and endpoints.

---

## AD-10: Instructor Role Requires Admin Approval

**Decision:** Users can self-register as Instructor, but course creation is locked until an Admin approves the account.

**Reasoning:** Open instructor registration with no oversight would allow any user to create courses, undermining platform governance.

**Tradeoff:** Requires an Admin to be active and responsive. For MVP where the developer is the Admin, this is trivial. For a real deployment, an approval workflow UI is needed.

---

## AD-11: UUID Primary Keys

**Decision:** All tables use UUID primary keys instead of integer sequences.

**Reasoning:**
- Avoids exposing record counts in URLs (no sequential /api/courses/1, /api/courses/2)
- Better for distributed systems and future scaling
- Modern standard for new PostgreSQL applications
- Prevents enumeration attacks where an attacker guesses sequential IDs

**Tradeoff:** Slightly larger storage footprint than integers. Negligible at this scale.

---

## AD-12: Quiz Questions Stored as JSONB

**Decision:** Quiz question structure is stored as a JSONB column on the ContentItems table rather than a separate relational QuizQuestions table.

**Reasoning:**
- PostgreSQL's native JSONB support makes this performant for read-heavy quiz display
- Quiz questions are always read and written together — no need to query individual questions in isolation
- Significantly simpler data model for MVP
- Flexible schema allows different question types without schema migrations

**Tradeoff:** Cannot efficiently query or aggregate individual question-level data (e.g. which question did most students get wrong). A relational QuizQuestions table is planned for V2 when quiz analytics become a requirement.

---

## AD-13: User Entity Inherits from IdentityUser

**Decision:** The User entity in MeritEd.Core inherits from `IdentityUser<Guid>` rather than being built from scratch.

**Reasoning:**
- ASP.NET Identity provides battle-tested password hashing, lockout, and security stamp behavior
- Reimplementing secure authentication primitives manually is high risk and low value
- Guid is used as the key type to match the UUID primary key decision (AD-11)

**Tradeoff:** MeritEd.Core takes on a dependency on `Microsoft.Extensions.Identity.Stores`, technically breaking the "zero infrastructure dependency" rule for Core. This is a deliberate, accepted exception — the alternative (reimplementing Identity) is strictly worse. The lightweight `Identity.Stores` package was chosen specifically over the full `Identity.EntityFrameworkCore` package to minimize what Core depends on.

---

## AD-14: Refresh Token Rotation

**Decision:** Every time a refresh token is used to obtain a new access token, the old refresh token is revoked and a new one is issued.

**Reasoning:**
- Limits the damage if a refresh token is ever stolen — it can only be used once before becoming invalid
- Standard security practice for JWT-based authentication systems
- Forces an attacker to act immediately on a stolen token rather than holding it indefinitely

**Tradeoff:** Requires a database write on every refresh, slightly more than a stateless check. Negligible cost for the security benefit gained.

---

## AD-15: Repository Pattern Deferred for Auth

**Decision:** AuthService accesses AppDbContext directly for refresh token operations rather than going through a repository interface.

**Reasoning:** For Sprint 1, refresh token queries are simple single-entity lookups. Introducing a repository abstraction adds a layer of indirection without meaningful benefit at this scale.

**Tradeoff:** Inconsistent with the repository pattern planned for other entities starting Sprint 2. This is a deliberate, temporary simplification — AuthService may be refactored to use a repository once the pattern is established elsewhere, for consistency.

---

## AD-16: Two-Flag Discussion Answers and One-Level Nesting

**Decision:** Replies carry two independent flags: `IsAcceptedAnswer` (set by the thread author, "this solved my problem") and `IsInstructorVerified` (set by the course instructor, "this is technically correct"). Replies can be nested one level deep via `ParentReplyId`.

**Reasoning:**
- A thread author can accept a wrong answer; the instructor flag gives students an authoritative signal without removing student agency
- Clarification questions belong under the specific reply, not the whole thread
- One level keeps the UI and queries simple

**Tradeoff:** Two flags means more UI states to design. Nested replies are loaded flat by EF Core, so the controller filters top-level replies and maps children via `ChildReplies`.

---

## AD-17: Outlier Detection via Median ±50%

**Decision:** A student is flagged High or Low when their XP is more than 50% above or below the class median. If the median is 0, nobody is flagged.

**Reasoning:** The median resists skew from one extreme student, and a fixed percentage is easy for instructors to understand. Standard deviation is unstable in small classes.

**Tradeoff:** Fixed thresholds can over-flag in small classes. A configurable threshold is a V2 candidate. Removed students are excluded from the median (their XP history is retained per BR-E3).

---

## AD-18: Badges Evaluated Once Per XP Event

**Decision:** Badge triggers are evaluated once after each XP award. Identity is recalculated after badge bonuses are applied.

**Reasoning:** Keeps the award flow predictable and avoids loops.

**Tradeoff:** A bonus that crosses another badge's threshold, or flips identity into an identity-badge trigger, is picked up on the next XP event, not in the same request.

---

## AD-19: Refresh Token in httpOnly Cookie

**Decision:** The refresh token is sent only as an httpOnly, Secure, SameSite=None cookie scoped to `/api/auth`. It is never returned in a JSON body. The access token is returned in the body and held in memory by the frontend.

**Reasoning:**
- JavaScript can't read the refresh token, so an XSS bug can't steal it
- The cookie path means the browser sends it only to auth endpoints
- Matches the strategy already documented in `security.md`

**Tradeoff:** Needs CORS with credentials and a fixed frontend origin. `SameSite=None` makes the refresh endpoint callable cross-site, but CORS stops an attacker's page from reading the response. Rotation limits the damage of any single token. The frontend has to call `/api/auth/refresh` on page load to restore the session, since the access token only lives in memory.

---

## AD-20: Access Token in Memory with Silent Refresh

**Decision:** The React client keeps the access token in a module-level variable, never in localStorage or sessionStorage. On page load, the client calls `/api/auth/refresh` and the httpOnly cookie restores the session. An Axios interceptor attaches the token to each request. On a 401 it refreshes once and retries the request. Concurrent refreshes share a single in-flight promise.

**Reasoning:**
- JavaScript-readable storage is exposed to XSS, and a token in memory disappears with the tab
- Refresh tokens rotate (AD-14), so two parallel refreshes would revoke each other and log the user out. The shared promise prevents that
- Route guards (`ProtectedRoute`, `PublicOnlyRoute`) wait for the initial refresh before redirecting, so a reload never flashes the login page
- Together with AD-19, no long-lived credential is ever readable by client JavaScript

**Tradeoff:** Every full page load costs one extra request to restore the session. Role-based route guards are a UX layer only, and the API remains the real authorization boundary (AD-09).

---

## AD-21: Course Access Levels

**Decision:** One `CourseAccessService` resolves a user's access to a course as None, Preview, Enrolled or Owner. Course, section and content-item reads all go through it. Non-owners see only published sections and items. Non-enrolled users see section titles but get 403 on content. Archived courses are visible only to the owner and enrolled students.

**Reasoning:** The access rule lives in one place instead of being re-derived per controller. It closes BR-AU4, which was previously unenforced on reads. The syllabus preview lets students decide before enrolling.

**Tradeoff:** One extra query per request to resolve access. A section must be published before its items are visible to students.

---

## AD-22: Instructor Approval Checked Against the Database

**Decision:** Course creation checks `IsApproved` in the database on every request. The `approved` JWT claim is informational only and the API never relies on it.

**Reasoning:** A token issued before approval stays valid for up to 60 minutes. Trusting the claim would force a re-login after approval and would let a revoked approval keep working until expiry.

**Tradeoff:** One extra query per course creation, which is negligible. The frontend's `isApproved` refreshes on reload or next login.