CareTrack — AGENTS.md

1. Purpose

This file defines how AI coding agents should work inside the CareTrack repository.

CareTrack is an independent portfolio demonstration of a staff-facing clinical referral and workflow management system using synthetic data only. It exists to demonstrate software engineering, architecture, security, testing, frontend engineering, and product-design practices.

CareTrack is not an NHS product, a production healthcare system, a certified clinical system, a medical device, or a patient self-service application in v1.

Do not introduce NHS branding, real patient data, real clinical records, claims of clinical certification, claims of regulatory approval, or claims of production fitness.

2. Working Principle

AI is an implementation assistant, not the product owner or architect.

The agent may accelerate repetitive code, component scaffolding, Tailwind composition, test scaffolding, refactoring, responsive variants, accessibility checks, and design audits.

The agent must not independently redefine domain rules, business workflow, authorization semantics, clinical UX safety rules, API contracts, project scope, design identity, data ownership, or deployment architecture.

When a decision is unclear, preserve the current architecture and ask for clarification rather than inventing a new direction.

3. Repository Structure

Expected high-level structure:

CareTrack/
├── AGENTS.md
├── backend/
│ ├── src/
│ │ ├── CareTrack.Api/
│ │ ├── CareTrack.Application/
│ │ ├── CareTrack.Domain/
│ │ └── CareTrack.Infrastructure/
│ └── tests/
│ ├── CareTrack.UnitTests/
│ └── CareTrack.IntegrationTests/
├── frontend/
│ └── caretrack-web/
├── tools/
│ └── CareTrack.AuthTestClient/
├── docs/
│ └── DESIGN.md
└── .github/

Use the existing structure instead of creating parallel alternatives.

4. Backend Architecture

CareTrack.Domain

Responsibilities:

entities

enums

domain invariants

state transitions

aggregate rules

Must not depend on ASP.NET Core, Entity Framework Core, Microsoft Entra, JWT, HttpContext, Infrastructure, or API.

CareTrack.Application

Responsibilities:

use cases

orchestration

application exceptions

interfaces

commands/results

cross-aggregate coordination

current-user abstraction

Depends on Domain only.

Must not depend on API, Infrastructure, HttpContext, or Microsoft Entra-specific infrastructure.

CareTrack.Infrastructure

Responsibilities:

Entity Framework Core

SQL Server

repository implementations

transaction implementation

persistence mappings/migrations

Depends on Application and Domain.

CareTrack.Api

Responsibilities:

controllers

HTTP contracts

dependency-injection composition

authentication

authorization

HttpCurrentUser

exception handling

OpenAPI

health endpoint

Depends on Application and Infrastructure.

Dependency Direction

Api → Application
Api → Infrastructure
Infrastructure → Application
Infrastructure → Domain
Application → Domain
Domain → no project dependency

Do not violate these boundaries.

5. Backend Business Rules

Do not invent or silently alter workflow rules.

Referral States

Draft
Submitted
AwaitingTriage
MoreInformationRequired
Accepted
Assigned
Scheduled
InProgress
Completed
Rejected
Cancelled

Core progression:

Draft
→ Submitted
→ AwaitingTriage
→ Accepted
→ Assigned
→ Scheduled
→ InProgress
→ Completed

Supported alternate transitions include:

AwaitingTriage → MoreInformationRequired

MoreInformationRequired → AwaitingTriage

AwaitingTriage → Rejected

cancellation where supported by the implemented domain

Appointment States

Scheduled
CheckedIn
InProgress
Completed
Cancelled
DidNotAttend

Core progression:

Scheduled
→ CheckedIn
→ InProgress
→ Completed

Supported alternatives:

Scheduled → Cancelled

CheckedIn → Cancelled

Scheduled → DidNotAttend

Scheduling Rules

use half-open intervals

overlap rule:

existing.Start < requestedEnd
AND
existing.End > requestedStart

overlap applies to the same patient

Cancelled and DidNotAttend do not block future scheduling

Completed appointments still block overlapping scheduling

scheduling is transactionally protected

creating an appointment from an Assigned referral moves the referral to Scheduled atomically

6. Authentication and Authorization

Microsoft Entra ID is the identity provider.

Do not create local password storage, custom login endpoints, custom MFA, custom password reset, custom refresh-token storage, a local Patient role, or authorization bypasses.

Roles

Clinician
ReferralCoordinator
Administrator

Policies

ClinicianAccess
ReferralManagement
AdministrativeAccess

ClinicianAccess

Requires authenticated user + delegated access_as_user scope + Clinician role.

ReferralManagement

Requires authenticated user + delegated access_as_user scope + ReferralCoordinator or Clinician role.

AdministrativeAccess

Requires authenticated user + delegated access_as_user scope + Administrator role.

Administrator is not a universal clinical superuser.

Current User

Use ICurrentUser. The API derives identity from trusted Microsoft Entra object-ID claims.

Do not use email, display name, request-body user IDs, or arbitrary client-supplied identifiers for security-sensitive ownership.

Clinical Note Ownership

ClinicalNote.CreatedBy is server-derived from the authenticated user. Do not reintroduce client-controlled CreatedBy.

7. Patient Identity Boundary

A Patient is a domain entity in v1, not an authenticated CareTrack actor.

Do not add a Patient role without a separate architectural decision.

A future patient portal would require patient-to-identity mapping, resource-level ownership checks, patient-safe contracts, and privacy/consent design.

8. Frontend Stack

The frontend uses:

Angular 22

standalone components

strict TypeScript

zoneless Angular

lazy-loaded routes

Angular HttpClient

Tailwind CSS v4

CSS custom properties for semantic design tokens

Phosphor Icons

Vitest

Microsoft Entra / MSAL planned for Phase 6C

Do not replace this stack without explicit approval.

Do not introduce React, Next.js, Angular Material as the default visual identity, PrimeNG as the default visual identity, Bootstrap, a second CSS framework, or Zone.js unless a documented dependency genuinely requires it.

9. Angular Conventions

Prefer modern Angular APIs.

Use standalone components, input() / output() where appropriate, signals for local synchronous state, computed signals for derived state, RxJS for asynchronous streams where it improves clarity, HttpClient for API communication, lazy-loaded routes, and typed reactive forms when forms are introduced.

Avoid unnecessary NgModules, service-locator patterns, giant components, manual DOM manipulation, global mutable state without reason, duplicated API logic, and route components that contain all feature logic.

The application is intentionally zoneless. Do not add provideZoneChangeDetection() or Zone.js without a documented technical reason.

10. Frontend Folder Ownership

Use:

src/app/
├── core/
├── shared/
├── features/
└── design-system/

core

Application-wide infrastructure: auth, config, http, layout, guards, interceptors, singleton services.

shared

Generic reusable utilities: shared models, directives, pipes, utilities. Do not turn it into a dumping ground.

features

Business capabilities:

features/
├── dashboard/
├── patients/
├── referrals/
├── appointments/
└── clinical-notes/

A feature may own pages, components, data-access, models, and routes.

design-system

Reusable visual primitives and UX patterns:

design-system/
├── tokens/
├── components/
├── patterns/
└── styles/

11. Feature Data Access

Do not create one giant application-wide API service.

Prefer feature-specific clients:

features/patients/data-access/patient-api.service.ts
features/referrals/data-access/referral-api.service.ts
features/appointments/data-access/appointment-api.service.ts
features/clinical-notes/data-access/clinical-note-api.service.ts

Do not invent backend endpoints to simplify frontend work.

If an endpoint appears missing:

identify the capability

explain why it is needed

propose an API contract

wait for approval before changing backend behavior

12. Design Source of Truth

Before implementing or redesigning CareTrack UI, read docs/DESIGN.md.

It defines visual personality, information hierarchy, accessibility requirements, motion rules, color strategy, typography, density, patient-context patterns, navigation rules, status semantics, and anti-patterns.

Do not override it because a generic AI pattern looks attractive.

13. Taste Skill

A Taste Skill is installed for Codex.

Use it for design-quality guidance, anti-generic UI pressure, layout guidance, motion guidance, and visual audits.

Do not treat it as the CareTrack design system.

Priority order:

CareTrack product/business constraints
→ docs/DESIGN.md
→ AGENTS.md
→ installed Taste Skill
→ agent preference

If the Taste Skill conflicts with CareTrack-specific rules, CareTrack rules win.

## Taste Skill Adaptation Rule

The installed taste skill may contain guidance originally optimized for
marketing or highly expressive product interfaces.

Do not discard the skill because CareTrack is an operational application.

Instead, adapt its design principles to CareTrack's context.

Use the skill for:

- visual hierarchy
- composition quality
- typography rhythm
- spacing
- motion quality
- interaction polish
- anti-generic layout guidance
- visual pre-flight review

Do not directly apply marketing-specific patterns such as:

- hero storytelling
- conversion funnels
- oversized CTA sections
- decorative motion
- campaign-style sections
- promotional content structure

CareTrack-specific rules in `docs/DESIGN.md` take precedence over
marketing-oriented structural guidance.

The goal is:
**tasteful operational software, not an unstyled enterprise tool and not a marketing site.**

CareTrack should still feel visually rich, polished, and responsive.

"Operational software" does not mean visually static software.

Use high-quality motion and interaction polish where they improve:

- orientation
- hierarchy
- perceived responsiveness
- workflow understanding
- state feedback
- task confidence

The restriction is against marketing-specific structure and distracting decorative motion, not against thoughtful animation or visual refinement.

14. Styling Rules

Use Tailwind primarily for layout, spacing, sizing, responsive behavior, alignment, and typography composition.

Use semantic CSS custom properties for brand colors, surfaces, text hierarchy, borders, status colors, shadows, radius values, and focus treatments.

Prefer semantic tokens over arbitrary colors.

Good:

<div class="bg-[var(--ct-surface)] text-[var(--ct-text)]">

Avoid arbitrary color utilities when they represent semantic product meaning.

Component CSS is acceptable where it improves clarity.

15. Design-System Promotion Rule

A component belongs in design-system when it is visually reusable, has a stable semantic contract, and multiple features can reasonably use it.

Good candidates:

button

icon button

status chip

form field

surface

dialog shell

empty state

skeleton

page header

patient identity banner

data toolbar

Feature-owned:

referral triage panel

referral action panel

appointment workflow controls

clinical note editor

referral history

appointment timeline

16. Phosphor Icons

Use Phosphor Icons for common UI iconography.

Preferred weights:

Regular → standard navigation/actions

Bold → selected/emphasized state

Do not mix icon families without explicit approval.

Do not hand-build common UI SVG icons.

Decorative icons use aria-hidden="true". Icon-only buttons require an accessible name.

17. Visual Quality Rules

CareTrack should feel calm, precise, trustworthy, mature, premium, operational, clinically appropriate, highly legible, visually refined, and intentionally polished.

Avoid by default:

marketing-style gradient hero banners

heavy glassmorphism

neon colors

excessive pill UI

giant rounded cards

decorative blobs

random icon circles

generic SaaS metric-card grids

excessive shadows

marketing-page typography

parallax

unnecessary animation

decorative healthcare stock imagery

NHS visual imitation

Visual novelty must never reduce clarity.

18. Clinical UX Safety

Patient context must be visually obvious whenever the user is acting on patient-specific work.

Prioritize patient name, patient reference, date of birth, and other safe synthetic identifiers where appropriate.

Do not hide patient context during referral review, appointment workflows, or clinical-note editing.

High-impact actions must have clear hierarchy and confirmation where appropriate.

Do not invent medical terminology or clinical meaning.

19. Status Rules

Workflow state must never rely on color alone.

Use text label + semantic tone + position/context + icon where helpful.

Global semantic tones:

neutral
info
success
warning
danger

Feature code maps domain states to these tones. The design system must not contain domain enums.

20. Accessibility

Always consider:

semantic HTML

keyboard navigation

visible focus

form labels

accessible errors

reduced motion

sufficient contrast

screen-reader labels

touch target size

dialog focus handling

table semantics

responsive layouts

Do not communicate state by color alone. Do not use placeholder-only labels.

21. Motion

Motion must communicate state or structure.

Appropriate:

panel entry/exit

route context changes

loading transitions

status transitions

success confirmation

hover/focus feedback

Avoid bouncing controls, floating cards, looping motion, parallax, excessive springs, cinematic transitions, and animation that delays work.

Typical timing:

micro: 120–180ms

panels/context: 180–280ms

Respect prefers-reduced-motion.

22. Testing

Keep:

npm run build
npm test

green.

Add meaningful tests for component contracts, input/output behavior, conditional rendering, state mapping, route guards, auth interceptors, accessibility-critical behavior, and feature data transformations.

Avoid tests that only assert CSS class lists, implementation details, or trivial framework behavior.

23. Git / Change Discipline

Before significant changes:

inspect relevant code

read this file

read docs/DESIGN.md for UI work

preserve current architecture

make the smallest coherent change

run tests/build

summarize what changed and why

Do not perform unrelated refactors in the same change.

24. Security Rules

Never commit access tokens, refresh tokens, client secrets, passwords, private keys, database credentials, or real patient data.

Safe public SPA configuration may include API base URLs, Entra client ID, tenant ID, scope URI, and redirect URI.

Anything sent to the browser is public.

Do not log access tokens, clinical-note content, secrets, or unnecessary personal data.

25. Documentation

When implementation changes architecture or behavior, update relevant documentation.

Do not knowingly leave docs describing implemented functionality as merely “planned”.

26. Agent Output Expectations

For implementation tasks:

inspect relevant files first

explain intended change briefly

preserve architecture

avoid unrelated work

implement

run verification

report changed files

report tests/build results

call out assumptions

For analysis/review/proposal tasks:

do not modify files

clearly separate findings from suggestions

27. Final Pre-Flight Checklist

Before considering a frontend change complete:

Does it follow docs/DESIGN.md?

Does it preserve feature ownership?

Does it use the design system appropriately?

Does it avoid invented backend behavior?

Is patient context safe where relevant?

Is the primary action clear?

Are statuses understandable without color alone?

Is it keyboard accessible?

Does it work responsively?

Does it avoid generic AI-dashboard patterns?

Does npm run build pass?

Do relevant tests pass?
