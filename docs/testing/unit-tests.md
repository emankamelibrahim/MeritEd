# Unit Tests

This document describes the unit tests for MeritEd's core business logic: what each test verifies, the inputs it uses, and the result it expects.

## Scope

The unit tests cover the four pure-logic services in `MeritEd.Core`. These services have no database, HTTP, or framework dependencies, so every test runs in memory in milliseconds.

| Service | Responsibility | Tests |
|---|---|---|
| [XPService](#xpservice) | XP calculation, scored quiz XP, recovery eligibility | 15 |
| [IdentityService](#identityservice) | Derives student identity from the XP ledger | 7 |
| [BadgeService](#badgeservice) | Evaluates which badges a student has earned | 11 |
| [OutlierService](#outlierservice) | Class median and High/Low outlier classification | 12 |
| [QuizGradingService](#quizgradingservice) | Validates, grades and sanitizes quiz questions | 18 |
| **Total** | | **63** |

**Not covered:** database wiring and orchestration (`ActivityService`, `BadgeAwardService`, `DashboardService`) and HTTP endpoints. These need integration tests (see [Known Gaps](#known-gaps)).

## Setup

| Requirement | Detail |
|---|---|
| SDK | .NET 10 SDK |
| Framework | xUnit |
| Project | `MeritEd.Tests` (references `MeritEd.Core` only) |
| Database | None required |
| Configuration | None required |

Run from the repository root:

```powershell
dotnet test
```

Run a single service's tests:

```powershell
dotnet test --filter "FullyQualifiedName~IdentityServiceTests"
```

## Conventions

- **Naming:** `Method_Scenario_ExpectedResult`
- **System under test:** each test class creates one instance, held in a field named `_sut`
- **`[Fact]`:** a single scenario
- **`[Theory]` + `[InlineData]`:** one scenario run over several inputs. Each row counts as its own test and appears separately in Test Explorer
- **Boundary cases:** thresholds are tested exactly at, just below, and just above the cutoff
- **Time-based tests:** `XPServiceTests` use dates relative to `DateTime.UtcNow` (±1 day), so they don't depend on the calendar date

---

## XPService

**Source:** `MeritEd.Core/Services/XPService.cs`
**Tests:** `MeritEd.Tests/Services/XPServiceTests.cs`

**Setup:** none. `new XPService()`.

### CalculateXP

Awards the base XP for on-time work. For recovery work it multiplies by the course's recovery multiplier and rounds down.

| ID | Test | Purpose | Input (baseXP, isRecovery, multiplier) | Expected |
|---|---|---|---|---|
| UT-XP-01 | `CalculateXP_OnTime_ReturnsFullBaseXP` | On-time work earns full XP | 100, false, 0.70 | `100` |
| UT-XP-02 | `CalculateXP_Recovery_AppliesMultiplierAndFloors` | Default multiplier is applied | 100, true, 0.70 | `70` |
| UT-XP-03 | same (theory row) | Instructor-configured multiplier is applied | 100, true, 0.75 | `75` |
| UT-XP-04 | same (theory row) | Fractional results round down | 50, true, 0.75 | `37` (37.5 floored) |
| UT-XP-05 | same (theory row) | Zero-XP activity yields zero | 0, true, 0.70 | `0` |

### IsRecoveryEligible

Recovery is available only when the activity allows it, has a due date, and the due date has passed.

| ID | Test | Purpose | Input (dueDate, allowsRecovery) | Expected |
|---|---|---|---|---|
| UT-XP-06 | `IsRecoveryEligible_PastDueAndAllowed_ReturnsTrue` | Late work on a recovery-enabled activity qualifies | now − 1 day, true | `true` |
| UT-XP-07 | `IsRecoveryEligible_RecoveryNotAllowed_ReturnsFalse` | Instructor disabled recovery | now − 1 day, false | `false` |
| UT-XP-08 | `IsRecoveryEligible_NoDueDate_ReturnsFalse` | No deadline means nothing can be late | null, true | `false` |
| UT-XP-09 | `IsRecoveryEligible_NotYetDue_ReturnsFalse` | Recovery is unavailable before the deadline (BR-P5) | now + 1 day, true | `false` |

### CalculateScoredXP

Quiz XP is `floor(baseXP × correct × multiplier ÷ total)`. The multiplier is 1 for on-time work and the course recovery multiplier for recovery work. It multiplies before dividing so exact results (25.0) don't become 24.999… and floor down.

| ID | Test | Purpose | Input (baseXP, correct/total, isRecovery, multiplier) | Expected |
|---|---|---|---|---|
| UT-XP-10 | `CalculateScoredXP_ScalesByFractionCorrect` | Full marks earn full XP | 100, 3/3, false, 0.70 | `100` |
| UT-XP-11 | same (theory row) | Partial score floors | 50, 2/3, false, 0.70 | `33` |
| UT-XP-12 | same (theory row) | Zero correct earns nothing | 100, 0/4, false, 0.70 | `0` |
| UT-XP-13 | same (theory row) | Recovery multiplier applied | 100, 1/2, true, 0.70 | `35` |
| UT-XP-14 | same (theory row) | Recovery and partial score together, exact result | 50, 2/3, true, 0.75 | `25` |
| UT-XP-15 | `CalculateScoredXP_ZeroQuestions_ReturnsZero` | Guards division by zero | 100, 0/0, false, 0.70 | `0` |

---

## IdentityService

**Source:** `MeritEd.Core/Services/IdentityService.cs`
**Tests:** `MeritEd.Tests/Services/IdentityServiceTests.cs`

**Setup:** `new IdentityService()`. Tests build the XP ledger with a helper, `Ledger(assessment, recovery, badge)`, which returns a list of `(XPCategory, amount)` tuples (zero-amount categories are omitted).

### Rules under test

Evaluated in priority order, first match wins:

1. Recovery XP **> 30%** of total → `Challenger`
2. Assessment XP **> 60%** of total → `Scholar`
3. Otherwise → `Contributor`
4. Total XP of zero → `null` (no identity yet)

| ID | Test | Purpose | Input (assessment / recovery / badge XP) | Expected |
|---|---|---|---|---|
| UT-ID-01 | `Calculate_EmptyLedger_ReturnsNull` | No XP means no identity | empty ledger | `null` |
| UT-ID-02 | `Calculate_AllAssessment_ReturnsScholar` | Pure assessment earner | 50 / 0 / 0 | `Scholar` |
| UT-ID-03 | `Calculate_RecoveryAbove30Percent_ReturnsChallenger` | Heavy recovery share | 110 / 75 / 55 (recovery 31.25%) | `Challenger` |
| UT-ID-04 | `Calculate_RecoveryDropsBelow30AfterBonus_ReturnsContributor` | **Regression:** a badge bonus dilutes recovery below the threshold | 110 / 75 / 75 (recovery 28.8%) | `Contributor` |
| UT-ID-05 | `Calculate_RecoveryTakesPriorityOverScholar` | Priority order: Challenger beats Scholar | 65 / 35 / 0 (assessment 65%, recovery 35%) | `Challenger` |
| UT-ID-06 | `Calculate_RecoveryExactly30Percent_IsNotChallenger` | Boundary: threshold is strictly greater than | 70 / 30 / 0 | `Scholar` |
| UT-ID-07 | `Calculate_AssessmentExactly60Percent_IsNotScholar` | Boundary: threshold is strictly greater than | 60 / 0 / 40 | `Contributor` |

> UT-ID-04 documents the stale-identity bug found during Sprint 7. It covers the rule only. The database wiring that triggers recalculation is outside unit-test scope.

---

## BadgeService

**Source:** `MeritEd.Core/Services/BadgeService.cs`
**Tests:** `MeritEd.Tests/Services/BadgeServiceTests.cs`

**Setup:** `new BadgeService()`. Test helpers:

| Helper | Creates |
|---|---|
| `MakeBadge(triggerType, triggerValue)` | A `Badge` with a new ID and the given trigger |
| `MakeEnrollment(xp, identity)` | An `Enrollment` with the given total XP and identity |
| `Evaluate(...)` | Calls `EvaluateTriggers` and returns a list |

`EvaluateTriggers` returns the badges that should be newly awarded. It never touches a database. The caller supplies the already-earned badge IDs and the first-submission and first-post flags.

| ID | Test | Purpose | Input | Expected |
|---|---|---|---|---|
| UT-BG-01 | `XPMilestone_AwardedAtOrAboveThreshold` | Below threshold: not awarded | milestone 100, student XP 99 | not awarded |
| UT-BG-02 | same (theory row) | Boundary: exactly at threshold | milestone 100, XP 100 | awarded |
| UT-BG-03 | same (theory row) | Above threshold | milestone 100, XP 250 | awarded |
| UT-BG-04 | `AlreadyEarnedBadge_IsNotAwardedAgain` | A badge can be earned only once (BR-B1) | milestone 100, XP 500, badge ID already earned | empty result |
| UT-BG-05 | `FirstSubmission_DependsOnFlag` | Awarded when this is the first approved submission | `isFirstSubmission = true` | 1 badge |
| UT-BG-06 | same (theory row) | Not awarded otherwise | `isFirstSubmission = false` | 0 badges |
| UT-BG-07 | `FirstDiscussionPost_DependsOnFlag` | Awarded on the first discussion post | `isFirstDiscussionPost = true` | 1 badge |
| UT-BG-08 | same (theory row) | Not awarded otherwise | `isFirstDiscussionPost = false` | 0 badges |
| UT-BG-09 | `IdentityReached_MatchesOnlyTheCurrentIdentity` | Only the badge for the current identity is awarded | identity `Challenger`; badges for Scholar (0) and Challenger (1) | only the Challenger badge |
| UT-BG-10 | `IdentityReached_NoIdentityYet_AwardsNothing` | No identity, no identity badge | identity `null`, Scholar badge | empty result |
| UT-BG-11 | `MultipleTriggers_AwardsEveryMatchingBadge` | All qualifying badges are returned in one pass | XP 125; milestones at 1, 100, 300 | 2 badges (1 and 100), not 300 |

> `FirstSubmission` and `FirstDiscussionPost` are driven by boolean flags passed in by the caller. The service deliberately doesn't query history, which keeps it testable without a database.

---

## OutlierService

**Source:** `MeritEd.Core/Services/OutlierService.cs`
**Tests:** `MeritEd.Tests/Services/OutlierServiceTests.cs`

**Setup:** `new OutlierService()`.

### CalculateMedian

| ID | Test | Purpose | Input | Expected |
|---|---|---|---|---|
| UT-OL-01 | `Median_Empty_ReturnsZero` | Empty class is handled safely | `[]` | `0` |
| UT-OL-02 | `Median_OddCount_ReturnsMiddleValue` | Sorts first, then takes the middle | `[90, 10, 50]` | `50` |
| UT-OL-03 | `Median_EvenCount_ReturnsMidpoint` | Averages the two middle values | `[0, 180]` | `90` |

### Classify

A student is `High` when XP is more than 50% above the median, `Low` when more than 50% below it, otherwise `Normal`. Threshold defaults to 0.5 and is configurable (AD-17).

| ID | Test | Purpose | Input (xp, median) | Expected |
|---|---|---|---|---|
| UT-OL-04 | `Classify_UsesFiftyPercentThreshold` | Well above median | 180, 90 | `High` |
| UT-OL-05 | same (theory row) | Well below median | 0, 90 | `Low` |
| UT-OL-06 | same (theory row) | At the median | 90, 90 | `Normal` |
| UT-OL-07 | same (theory row) | Boundary: exactly +50% | 135, 90 | `Normal` |
| UT-OL-08 | same (theory row) | Just over +50% | 136, 90 | `High` |
| UT-OL-09 | same (theory row) | Boundary: exactly −50% | 45, 90 | `Normal` |
| UT-OL-10 | same (theory row) | Just under −50% | 44, 90 | `Low` |
| UT-OL-11 | `Classify_ZeroMedian_NeverFlags` | A percentage of zero is meaningless, so nobody is flagged | 500, 0 | `Normal` |
| UT-OL-12 | `Classify_CustomThreshold_IsRespected` | Threshold parameter overrides the default | 130, 100, threshold 0.25 | `High` |

---

## QuizGradingService

**Source:** `MeritEd.Core/Services/QuizGradingService.cs`
**Tests:** `MeritEd.Tests/Services/QuizGradingServiceTests.cs`

**Setup:** `new QuizGradingService()`. Tests parse JSON with a `Json(string)` helper and reuse a valid two-question quiz (`Valid`).

Question JSON shape: `[{"question": "...", "options": ["a", "b"], "answer": 0}]`, where `answer` is the zero-based index of the one correct option.

### Parse

| ID | Test | Purpose | Input | Expected |
|---|---|---|---|---|
| UT-QZ-01 | `Parse_ValidQuiz_ReturnsQuestions` | A well-formed quiz parses | Valid two-question quiz | valid, 2 questions, answers and option counts preserved |
| UT-QZ-02 | `Parse_InvalidShapes_AreRejected` | Root must be an array | `{}` | invalid |
| UT-QZ-03 | same (theory row) | Quiz must not be empty | `[]` | invalid |
| UT-QZ-04 | same (theory row) | Root must be an array, not a string | `"text"` | invalid |
| UT-QZ-05 | same (theory row) | Question text required | no `question` property | invalid |
| UT-QZ-06 | same (theory row) | At least two options | one option | invalid |
| UT-QZ-07 | same (theory row) | Answer must be a valid index | `answer: 2` with 2 options | invalid |
| UT-QZ-08 | same (theory row) | Negative answer index rejected | `answer: -1` | invalid |
| UT-QZ-09 | same (theory row) | Answer required | no `answer` property | invalid |
| UT-QZ-10 | same (theory row) | Blank option text rejected | `["a", ""]` | invalid |
| UT-QZ-11 | `Parse_MoreThanMaxQuestions_IsRejected` | Size cap (50) enforced | 51 questions | invalid |

### Grade

| ID | Test | Purpose | Input (answers) | Expected |
|---|---|---|---|---|
| UT-QZ-12 | `Grade_AllCorrect_ReturnsFullCount` | Full marks | `[1, 0]` | valid, 2 correct |
| UT-QZ-13 | `Grade_PartiallyCorrect_CountsOnlyCorrectOnes` | Partial score | `[1, 1]` | 1 correct |
| UT-QZ-14 | `Grade_AllWrong_ReturnsZero` | No correct answers | `[0, 2]` | 0 correct |
| UT-QZ-15 | `Grade_WrongNumberOfAnswers_IsInvalid` | Every question must be answered | `[1]` | invalid |
| UT-QZ-16 | `Grade_OptionOutOfRange_IsInvalid` | Cannot choose a non-existent option | `[1, 3]` | invalid |
| UT-QZ-17 | `Grade_NegativeIndex_IsInvalid` | Negative index rejected | `[-1, 0]` | invalid |

### StripAnswers

| ID | Test | Purpose | Input | Expected |
|---|---|---|---|---|
| UT-QZ-18 | `StripAnswers_RemovesAnswerFieldAndKeepsTheRest` | Students never receive correct answers (AD-24) | Valid quiz | no `"answer"` in output; questions and options intact |

> UT-QZ-18 covers the sanitizing logic. That the controller actually uses it for students is verified manually (Postman check #3) until integration tests exist.

---

## Known Gaps

| Gap | Why it matters | Planned |
|---|---|---|
| `BadgeAwardService` identity recalculation after bonuses | The Sprint 7 stale-identity bug lived here. Unit tests cover the rule, not the wiring | Integration tests (Sprint 13) |
| `ActivityService` submit/approve flow | Orchestrates XP, ledger, identity, and badges against the database | Integration tests (Sprint 13) |
| Dashboard aggregation | Queries and grouping happen in `DashboardService` | Integration tests (Sprint 13) |
| Authorization and ownership checks | Enforced in controllers and services against the database | Integration tests (Sprint 13) |
| Badges triggering other badges in a single request | By design (AD-18), but not asserted by a test | Integration tests (Sprint 13) |
| Quiz submission flow in `ActivityService` | Duplicate-submission index, late-work rejection and ledger writes happen against the database | Integration tests (Sprint 13) |
| Answer stripping in `ContentItemsController` | The helper is unit tested, the role-based choice of response isn't | Integration tests (Sprint 13) |

## Maintaining These Tests

- When a threshold changes (e.g. the 30% Challenger cutoff), update the boundary tests (UT-ID-06, UT-ID-07, UT-OL-07 to UT-OL-10) in the same commit as the code change.
- When a new identity, badge trigger type, or XP category is added, add at least one passing case, one failing case, and one boundary case.
- When a bug is found in a Core service, add a regression test before fixing it (UT-ID-04 is an example).
- Keep the counts in the [Scope](#scope) table and in the README in sync with the actual test total.