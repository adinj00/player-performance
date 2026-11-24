# Foundations and access

These features support every staff workflow. Their order reflects the real dependency path from the application foundation through authenticated and accessible use.

## Foundation

### 1. Application foundation · existing · GA

The frontend, backend, database, error handling, forms, tests, file storage abstraction, and development configuration provide the working base for later features.

**Done when:** the documented application structure builds, core health and error paths are covered, and later slices can use the shared data, API, and interface foundations.

Check the explicit PostgreSQL migration chain and live schema, including settings, venue and opponent repairs, Identity and player assignments. Carry that evidence into the dependent checks.

- [ ] Verify it: `/check verify application foundation`
- [ ] Test it: `/test application foundation`
- [ ] Review it: `/check review application foundation`
- [ ] Document it: `/document application foundation`

Legacy specs Units 01 to 13 and 40 · code in `backend/`, `frontend/`, and root configuration

### 2. Staff authentication and sessions · existing · GA

Club staff can sign in through secure cookie sessions, complete the required first password change, use protected routes, and sign out. Public registration and player accounts do not exist.

**Done when:** an active staff account can sign in, establish a protected session, change a required temporary password, and sign out with server owned authorization and CSRF protection.

- [ ] Verify it: `/check verify staff authentication and sessions`
- [ ] Test it: `/test staff authentication and sessions`
- [ ] Review it: `/check review staff authentication and sessions`
- [ ] Document it: `/document staff authentication and sessions`

Legacy specs Units 14 to 20 · code in `backend/src/Api/Endpoints/Auth/` and `frontend/src/features/auth/`

### 3. Staff access and account administration · in-progress · GA

Administrators can invite staff, assign one role, set team scope and explicit permissions, and disable or reactivate accounts. The implementation exists, but the progress record still identifies focused lifecycle and team scope verification as incomplete.

**Done when:** invitations, role changes, team scopes, permissions, disable and reactivate behavior, and the final active administrator safeguard pass the real application check and focused tests.

Exercise invited and inactive administrators and concurrent changes to active administrators. The inventory flags the guard behavior for runtime reproduction.

- [x] Build the current backend and interface from legacy specs Units 23 and 24
- [ ] Verify it: `/check verify staff access and account administration`
- [ ] Test it: `/test staff access and account administration`
- [ ] Review it: `/check review staff access and account administration`
- [ ] Document it: `/document staff access and account administration`

Legacy specs [Unit 23](../legacy/context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md) and [Unit 24](../legacy/context/feature-specs/24-staff-users-roles-ui.md) · code in `backend/src/Api/Endpoints/Users/` and `frontend/src/features/users/`

### 4. Responsive shell and accessibility · existing

The Bosnian Latin staff interface provides the club shell, responsive navigation, route focus, keyboard access, readable tables, accessible charts, and practical WCAG 2.2 AA hardening.

**Done when:** protected routes retain clear landmarks and headings, keyboard and narrow viewport use remains practical, and privacy restricted content never leaks through hidden or focusable interface state.

The intended dark theme and the current light shell remain unresolved under spec 0001. Verify the current shell and route any design reconciliation through `/architect`.

- [ ] Verify it: `/check verify responsive shell and accessibility`
- [ ] Test it: `/test responsive shell and accessibility`

Legacy spec [Unit 56](../legacy/context/feature-specs/56-responsive-accessibility-hardening.md) · code in `frontend/src/app/`, `frontend/src/components/`, and `frontend/src/index.css`
