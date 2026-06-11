# Non-Functional Requirements

| ID | Requirement |
|---|---|
| NFR-1 | API response time under 500ms for all standard operations |
| NFR-2 | JWT tokens expire after 60 minutes, refresh tokens valid for 7 days |
| NFR-3 | All API endpoints require authentication except register and login |
| NFR-4 | Role-based authorization enforced at API layer, not just frontend |
| NFR-5 | Database migrations managed through EF Core — no manual SQL changes |
| NFR-6 | All passwords hashed using ASP.NET Identity defaults (PBKDF2) |
| NFR-7 | API follows RESTful conventions consistently |
| NFR-8 | Frontend and backend deployable independently |