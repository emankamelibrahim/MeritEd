# Security and Authorization Strategy

## Authentication Approach

MeritEd uses JWT (JSON Web Tokens) for stateless authentication.

### Flow

1. User submits email and password to `POST /api/auth/login`
2. Server validates credentials against ASP.NET Identity
3. Server issues a short-lived JWT access token (60 minutes)
4. Server issues a long-lived refresh token (7 days)
5. Client stores tokens and attaches JWT to every subsequent request via Authorization header
6. When access token expires, client uses refresh token to obtain a new one
7. If refresh token expires, user must log in again

### Why JWT

- Stateless — server does not store session state
- Scalable — works across multiple server instances without shared session storage
- Industry standard — demonstrates real-world auth knowledge
- Works naturally with React frontend and ASP.NET Core backend as separate deployments

### Token Storage

- Access token: memory or httpOnly cookie
- Refresh token: httpOnly cookie only — never localStorage
- Reason: localStorage is accessible to JavaScript and vulnerable to XSS attacks

---

## Authorization Strategy

### Roles

| Role | Description |
|---|---|
| Student | Self-registered, active immediately, can enroll and participate |
| Instructor | Self-registered, requires Admin approval before course creation |
| Admin | Platform operator, manages users and approvals |

### Role Matrix

| Action | Student | Instructor | Admin |
|---|---|---|---|
| Register | ✓ | ✓ | — |
| Login | ✓ | ✓ | ✓ |
| Browse open courses | ✓ | ✓ | ✓ |
| Self-enroll in course | ✓ | — | — |
| View enrolled course content | ✓ | — | — |
| Submit assignment/quiz | ✓ | — | — |
| Post discussion thread | ✓ | ✓ | — |
| Create course | — | ✓ (approved only) | — |
| Manage course content | — | ✓ (own courses only) | — |
| Enroll/remove students | — | ✓ (own courses only) | — |
| Approve assignments | — | ✓ (own courses only) | — |
| View instructor dashboard | — | ✓ (own courses only) | — |
| Approve instructor accounts | — | — | ✓ |
| Manage platform badges | — | — | ✓ |
| Manage users | — | — | ✓ |

### Authorization Rules

- Authorization is enforced at the API layer using ASP.NET Core policy-based authorization
- Frontend restrictions are supplementary only and never trusted as security boundaries
- An instructor can only manage resources belonging to their own courses
- A student can only access courses they are enrolled in
- Resource-level ownership checks are performed in the service layer, not just role checks

---

## Security Considerations

### Password Security
- Passwords hashed using ASP.NET Identity defaults (PBKDF2 with SHA-256)
- Minimum password requirements enforced at registration
- Passwords never stored in plain text or logged

### API Security
- All endpoints require authentication except `POST /api/auth/register` and `POST /api/auth/login`
- HTTPS enforced in production
- CORS configured to allow only the known frontend origin
- Input validation on all request DTOs using data annotations or FluentValidation

### Common Threats Addressed

| Threat | Mitigation |
|---|---|
| XSS | httpOnly cookies for refresh token, input sanitization |
| CSRF | JWT in Authorization header is not vulnerable to CSRF |
| Brute force | ASP.NET Identity lockout policy |
| Unauthorized access | JWT validation on every request, role and ownership checks |
| SQL injection | EF Core parameterized queries by default |

---

## JWT Structure

### Access Token Claims

| Claim | Value |
|---|---|
| sub | User ID |
| email | User email |
| role | User role (Student, Instructor, Admin) |
| approved | Instructor approval status |
| iat | Issued at timestamp |
| exp | Expiry timestamp |

### Token Validation

Every request is validated for:
- Valid signature (server secret key)
- Not expired
- Correct issuer and audience