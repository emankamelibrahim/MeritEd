## Testing

MeritEd's core business rules are covered by unit tests. They live in `MeritEd.Tests` and reference only `MeritEd.Core`, so they run without a database, configuration, or running API.

```powershell
dotnet test
```

**Current status:** 39 tests, all passing.

| Service | What it verifies | Tests | Details |
|---|---|---|---|
| XPService | XP calculation, recovery multiplier, recovery eligibility | 9 | [View](docs/testing/unit-tests.md#xpservice) |
| IdentityService | Identity derivation from XP ratios, including threshold boundaries | 7 | [View](docs/testing/unit-tests.md#identityservice) |
| BadgeService | Badge trigger evaluation and one-time awarding | 11 | [View](docs/testing/unit-tests.md#badgeservice) |
| OutlierService | Class median and High/Low outlier classification | 12 | [View](docs/testing/unit-tests.md#outlierservice) |

Each test is documented with its purpose, inputs, and expected output in [`docs/testing/unit-tests.md`](docs/testing/unit-tests.md), along with setup notes and [known gaps](docs/testing/unit-tests.md#known-gaps). Integration tests for database-backed flows are planned for Sprint 13.