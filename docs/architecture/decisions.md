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