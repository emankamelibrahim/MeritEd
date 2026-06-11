# Business Rules

Business rules are constraints that govern system behavior independent of implementation.
They sit between requirements and design — they are not features, they are laws the system must never violate.

## Progression Rules

| ID | Rule |
|---|---|
| BR-P1 | XP can only increase. Under no circumstances is XP ever deducted from a student. |
| BR-P2 | XP is scoped per course. A student's XP in one course has no effect on another. |
| BR-P3 | Quiz XP is awarded automatically on submission. No instructor action required. |
| BR-P4 | Assignment XP is awarded only after explicit instructor approval. |
| BR-P5 | Recovery attempts are only available after the original deadline has passed. |
| BR-P6 | Recovery XP is calculated as: base XP × recovery multiplier. |
| BR-P7 | The default recovery multiplier is 0.7. Instructors may configure this per course. |
| BR-P8 | A student may not submit a recovery attempt if the original was already approved. |

## Badge Rules

| ID | Rule |
|---|---|
| BR-B1 | A badge can only be earned once per student per course. |
| BR-B2 | Badge XP bonus is awarded as a one-time XP transaction at the moment of earning. |
| BR-B3 | Badge evaluation is triggered automatically every time XP is awarded. |

## Identity Rules

| ID | Rule |
|---|---|
| BR-I1 | Identity is derived from XP source distribution, not total XP. |
| BR-I2 | Identity is recalculated every time XP is awarded. |
| BR-I3 | Identity thresholds (subject to revision): |
| | — Scholar: Assessment XP > 60% of total |
| | — Challenger: Recovery XP > 30% of total |
| | — Contributor: No single category exceeds 50% |
| | — Mentor: Discussion-based — deferred pending credibility model |
| BR-I4 | Identity is stored on the Enrollment record for performance. |

## Enrollment Rules

| ID | Rule |
|---|---|
| BR-E1 | A student may not enroll in a course with status closed or archived. |
| BR-E2 | A removed student loses access to course content immediately. |
| BR-E3 | Removing a student does not delete their XP history — it is retained for record. |

## Authorization Rules

| ID | Rule |
|---|---|
| BR-AU1 | An instructor account requires Admin approval before course creation is permitted. |
| BR-AU2 | A student account is active immediately upon registration. |
| BR-AU3 | An instructor can only manage courses they own. |
| BR-AU4 | A student can only access courses they are enrolled in. |
| BR-AU5 | Authorization is enforced at the API layer. Frontend restrictions are supplementary only. |

## Discussion Rules

| ID | Rule |
|---|---|
| BR-D1 | Only the original thread author can mark a reply as accepted answer. |
| BR-D2 | No XP is awarded for any discussion action in MVP. |
| BR-D3 | Only enrolled users and the course instructor can participate in a course discussion. |