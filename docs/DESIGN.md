CareTrack — DESIGN.md

1. Design Vision

CareTrack is a staff-facing clinical operations workspace designed around clarity, workflow awareness, patient context, and confident task completion.

The core visual concept is:

Calm Clinical Operations

CareTrack should feel trustworthy, precise, calm, operational, sophisticated, modern, human, highly legible, and quietly premium.

The product should look intentionally designed for prolonged operational use rather than like a marketing website or generic SaaS admin template.

Clarity takes priority over spectacle.

2. Product Context

CareTrack supports staff workflows involving:

patients

referrals

triage

assignment

appointments

workflow progression

clinical notes

The application is staff-facing in v1.

Patients are domain records, not authenticated users.

The design must optimize for:

high information density

rapid scanning

safe patient context

clear state

clear actions

repeat daily use

3. Design Personality

CareTrack is

composed

intelligent

restrained

clinically appropriate

mature

refined

efficient

reassuring

structured

CareTrack is not

playful

loud

cyberpunk

futuristic

overly corporate

sterile

consumer-social

marketing-heavy

decorative for its own sake

4. Primary Design Principles

4.1 Clinical clarity over decoration

Every visual element must improve comprehension, orientation, task completion, confidence, or safety.

Decorative UI that does not support those goals should be removed.

4.2 Calm density

CareTrack is an operational system.

It should support dense information without becoming cluttered.

Use whitespace to separate concepts, not to make every screen oversized.

4.3 Patient context is sacred

Whenever work is patient-specific, the patient context must remain visually obvious.

Users should not need to hunt for the active patient identity.

4.4 Workflow state must be obvious

Referral and appointment state are operationally important.

State must be communicated through visible label, semantic tone, context/position, and icon where helpful.

Never use color alone.

4.5 Actions must have hierarchy

Every view should distinguish primary action, secondary actions, low-priority actions, destructive/high-risk actions, and passive information.

Avoid equal visual emphasis for every button.

4.6 Motion explains change

Motion should communicate navigation, state transition, panel entry, loading, success, or completion.

Motion should not exist simply to feel animated.

4.7 Reuse should not erase domain meaning

Reusable primitives belong in the design system.

Domain-specific experiences should remain feature-owned.

5. Visual Direction

The primary visual direction is:

bright white and soft cool-grey application surfaces

crisp white or subtly blue-tinted work surfaces

deep navy / charcoal text

restrained clinical blue as the primary brand and interaction accent

green reserved primarily for success semantics

amber reserved for warning / attention semantics

red reserved for danger / destructive semantics

carefully chosen semantic colors

subtle borders

low, restrained elevation

small-to-medium corner radii

strong typographic hierarchy

compact operational controls

elegant data presentation

clear status language

Blue is intentional, but it must feel refined rather than generic.

Avoid the default saturated “hospital template” look, overly bright royal blue everywhere, or NHS visual imitation.

Do not reproduce NHS branding or visual identity.

6. Brand Mood

The UI should evoke:

calm
precision
continuity
trust
focus
operational control

It should not evoke:

social media
consumer wellness
gaming
crypto
futurism
marketing SaaS

7. Color Strategy

Colors must be semantic.

Do not couple domain state directly to arbitrary Tailwind color names.

Core token groups:

background
surface
surface-subtle
surface-muted
surface-elevated

text-primary
text-secondary
text-muted
text-inverse

border
border-strong

primary
primary-hover
primary-soft
primary-strong

focus

success
success-soft
warning
warning-soft
danger
danger-soft
info
info-soft
neutral
neutral-soft

Domain features map their states onto semantic tones.

Example:

ReferralStatus.AwaitingTriage → warning
ReferralStatus.Accepted → success
ReferralStatus.Rejected → danger

This mapping belongs in the feature layer. The global design system only knows semantic tones.

8. Initial Palette Direction

The exact palette may be refined during visual exploration.

Background

White should be dominant.

Use pure white for primary work surfaces and a very light cool grey / pale blue-grey for the application canvas where separation is useful.

Primary

Use a refined clinical blue for:

- primary actions
- active navigation
- selection
- focus emphasis
- informational emphasis where appropriate

Prefer a medium-to-deep blue rather than bright electric blue.

Green should not be the main brand color; reserve it mainly for success semantics.

Avoid:

- blue-purple SaaS gradients
- oversaturated hospital-template blue
- neon cyan
- consumer-wellness emerald as the primary brand

Text

Use deep navy / charcoal for primary text, with cool neutral greys for supporting text.

Borders

Use subtle cool grey or blue-grey borders.

Semantic colors

Muted, professional, readable.

Danger should not be overly saturated unless urgency demands it.
Warning must remain readable on light surfaces.
Success should feel calm rather than celebratory.

### Palette Balance

Target visual balance:

```text
70–80%  white / near-white surfaces
10–15%  soft cool-grey / pale blue-grey structure
5–10%   clinical blue interaction and brand emphasis
small amounts of green / amber / red for semantic states
```

The interface should feel bright and clean, not washed out.

### Reference Direction

Current reference direction combines:

- the polished shell and spacing quality of premium modern productivity dashboards
- the blue/white clarity of contemporary healthcare dashboards
- the denser queue, filtering, and scheduling patterns of operational clinical tools

CareTrack should not copy any reference literally. It should combine their strongest qualities into a distinct CareTrack language.

The preferred visual character is:

> **Premium blue-and-white clinical operations software with calm density, clear workflow hierarchy, and subtle interaction polish.**

9. CSS Token Naming

Use semantic custom properties with the --ct- prefix.

Examples:

--ct-bg
--ct-surface
--ct-surface-subtle
--ct-text
--ct-text-secondary
--ct-text-muted
--ct-border
--ct-border-strong
--ct-primary
--ct-primary-hover
--ct-primary-soft
--ct-success
--ct-warning
--ct-danger
--ct-info
--ct-focus
--ct-radius-sm
--ct-radius-md
--ct-radius-lg
--ct-shadow-xs
--ct-shadow-sm
--ct-shadow-md

Avoid implementation-coupled tokens like --green-700, --card-grey, or --blue-button.

10. Typography

Typography must support prolonged reading and scanning.

Primary goals:

legibility

compact hierarchy

calm rhythm

high contrast

clear grouping

Safe initial stack:

font-family:
Inter,
ui-sans-serif,
system-ui,
-apple-system,
BlinkMacSystemFont,
"Segoe UI",
sans-serif;

The final typeface may be revisited if a clearly better option emerges.

Type Scale

Page title: ~28–32px, semibold

Major section heading: ~20–24px

Panel heading: ~16–18px

Body: ~14–16px

Metadata: ~12–13px

Table text: ~13–14px

Avoid giant 48–72px dashboard headings, landing-page hero typography, and excessive uppercase text.

11. Typography Hierarchy

Use size, weight, spacing, and contrast before decorative styling.

Typical hierarchy:

Page title
↓
page description
↓
section heading
↓
panel/card heading
↓
body
↓
metadata

Do not create hierarchy by adding random colors.

12. Spacing System

Use a 4px-based rhythm.

Preferred values:

4
8
12
16
20
24
32
40
48
64

Dense UI may use smaller spacing internally.
Major page sections should breathe more.

Consistency matters more than maximizing whitespace.

13. Radius System

CareTrack should avoid oversized “soft SaaS” rounding.

Suggested:

small controls → 6px
inputs/buttons → 8px
panels → 10–14px
major overlays → 14–18px

Reserve pill shapes for status chips, tags, and compact semantic labels.

Do not use pill shapes for large cards, major navigation containers, or major panels.

14. Elevation

Prefer border contrast before shadows.

Base surface → subtle border, no or near-invisible shadow

Interactive/raised surface → subtle shadow

Dialog/overlay → moderate shadow

Avoid dramatic blur and large floating shadows on normal cards.

15. Surface Hierarchy

Recommended levels:

Application background → page background
Surface → primary working panel
Surface subtle → secondary grouped information
Surface elevated → dropdown/dialog/popover

Use surfaces to express hierarchy, not decoration.

16. Layout Philosophy

Use screen width intelligently.

Desktop

Prioritize scanability, stable navigation, useful side-by-side information, and dense but readable data.

Tablet

Reduce secondary panels while preserving primary tasks.

Mobile

Remain functional, but do not pretend the product is primarily a consumer mobile app.

Avoid turning every desktop data table into unrelated cards unless necessary.

17. Application Shell

The shell should provide:

persistent primary navigation

current-area orientation

user/account context

room for role-aware actions

responsive behavior

future administrative entry points where appropriate

Avoid oversized sidebar logos, giant navigation icon bubbles, strong marketing gradients, and excessive chrome.

Preferred shell direction:

- slim or compact primary navigation
- clear active-state treatment
- bright main work canvas
- blue used selectively for active/primary states
- subtle separators rather than heavy dark framing
- utility actions and user context kept visually quiet

18. Navigation

Expected primary areas:

Dashboard
Patients
Referrals
Appointments

Clinical Notes are generally contextual to appointment/patient work rather than a dominant global destination.

Administrative navigation appears only when implemented and role-appropriate.

Active navigation must be identifiable through more than color alone.

19. Page Header Pattern

A page header may contain:

page title

short description

primary action

secondary action(s)

filters or summary context

Do not place every possible action in the header.

20. Patient Identity Pattern

This is a critical CareTrack-specific pattern.

Whenever the user enters patient-specific workflow, display patient identity in a stable, visually distinct area.

Potential information:

Patient name
Patient reference
Date of birth
Relevant synthetic identifier

It should remain visible during referral review, appointment processing, clinical-note editing, and other patient-specific workflows.

Do not style it like a promotional card. It is a safety/context component.

21. Status System

Use semantic tones:

neutral
info
success
warning
danger

Status components must include visible text.

Color is reinforcement only.

Status chips should be compact, readable, low-saturation, and table-friendly.

Do not turn the UI into a collection of decorative pills.

22. Referral Workflow Visualization

Referral progression deserves a clear visual pattern.

Potential forms:

horizontal progression on wide screens

vertical progression on narrow screens

explicit current state

completed/current/future states visually distinct

terminal states clearly represented

Avoid decorative timeline dots without labels, animated percentage bars, and ambiguous semantics.

23. Appointment Workflow

The current appointment state should be obvious, and the UI should expose only valid next actions where possible.

Examples:

Scheduled → Check In
Checked In → Start
In Progress → Complete

Do not display impossible actions and rely on backend errors as the primary UX.

The backend remains the source of truth.

24. Data Tables

Tables are first-class CareTrack UI.

Prioritize:

scanability

stable columns

readable row density

meaningful alignment

clear hover/focus

visible sorting where implemented

filters

pagination

readable status cells

useful empty states

Alignment guidance:

text → left

dates/times → consistent

numeric values → generally right where useful

status → visually stable

actions → predictable column

Rows may be clickable only if discoverable and accessible.

25. Filters and Search

Filters should remain compact, show active state, be removable/resettable, and avoid taking over the page.

Search should have a visible label or accessible name, clear scope, and debounce where appropriate.

Avoid giant standalone search bars unless search is the page’s dominant function.

26. Forms

Forms must prioritize:

explicit labels

clear required state

logical grouping

inline validation

useful help text

predictable keyboard flow

preserved input

Never use placeholder-only labels.

Avoid giant floating labels, overly tall controls, equal emphasis for every field, and unexplained validation messages.

27. Form Layout

Group related fields.

Example:

Patient identity
Contact information
Referral details
Scheduling details

Use:

1 column on narrow layouts

2 columns where relationships are obvious

3+ columns only for compact structured information

Do not create dense 4-column forms just because space exists.

28. Buttons

Primary

Main local workflow action.

Secondary

Supporting action.

Tertiary / Ghost

Low-priority commands.

Destructive

Consequential operations only.

Avoid multiple visually dominant buttons side by side unless genuinely equal in priority.

29. Icon Buttons

Icon-only buttons:

require accessible labels

need predictable hover/focus state

use Phosphor Icons

should not replace text where text improves clarity

30. Icons

Use Phosphor Icons.

Default weights:

Regular → standard UI

Bold → selected/emphasized state

Typical mapping:

Patients → user/person
Referrals → workflow/directional symbol
Appointments → calendar
Clinical Notes → note/document
Search → magnifying glass
Edit → pencil
Add → plus
Warning → warning
Close → x

Do not use emoji as interface icons, mix icon families, add decorative icons to every card, or put every icon inside a colored circle.

31. Cards and Panels

Use cards only when content benefits from grouping.

Good:

grouped summary

patient identity context

compact actionable workflow block

meaningful dashboard summary

Bad:

every table row as a desktop card

every statistic as a giant card

nested cards inside cards

decorative cards with no information role

32. Dashboard Design

The dashboard should answer:

what needs attention?

what is happening today?

what work is waiting?

what should the current role do next?

Avoid generic KPI grids, meaningless charts, huge welcome heroes, and decorative analytics.

A dashboard may still use compact summary metrics when they directly support action, for example:

- awaiting triage
- need assignment
- appointments today
- overdue work

Prefer compact summaries integrated with the operational layout rather than oversized equal-weight cards.

Metrics should support action.

33. Role-Aware Experience

ReferralCoordinator

Prioritize:

referrals

patient registration/update

assignment

scheduling

queues

Clinician

Prioritize:

patient context

appointment workflow

clinical notes

clinical workload

Administrator

Only show admin functionality when it actually exists.

Role-aware UI should shape the workspace, not merely hide buttons.

34. Empty States

Every empty state should answer:

what is empty?

is that expected?

what can the user do next?

Avoid giant illustrations and vague “Nothing here yet” messages.

35. Loading States

Use skeletons for structured content, compact spinners for command actions, and page-level loading only when necessary.

Do not block the whole application for a small request.

36. Error States

Errors should explain what failed, preserve context, provide recovery where possible, and distinguish validation from system errors.

Never expose stack traces or raw backend exception details.

37. Toasts and Notifications

Use for:

successful command completion

recoverable warnings

non-blocking errors

Do not use a toast as the only indication of an important workflow state.

38. Dialogs

Use dialogs for focused, bounded tasks or confirmations.

Avoid large forms and long workflows inside dialogs.

Destructive confirmation dialogs should name the action and consequence, not merely ask “Are you sure?”

39. Motion

Motion should be subtle, purposeful, and polished.

Operational does not mean static.

Use motion where it improves orientation, hierarchy, perceived responsiveness, workflow understanding, or feedback.

Typical timing:

micro interactions → 120–180ms
panels/context → 180–280ms

Prefer opacity, transform, and controlled height where needed.

Avoid parallax, bouncy springs, floating UI, looping motion, cinematic transitions, and animation that slows task completion.

Appropriate polish includes:

- smooth navigation selection changes
- subtle sidebar expansion/collapse
- restrained hover and press feedback
- dialog and panel entrance/exit
- table/filter state transitions
- skeleton-to-content transitions
- workflow-step feedback
- success-state confirmation

Animation should make the product feel responsive and intentional, not promotional.

40. Reduced Motion

Support:

@media (prefers-reduced-motion: reduce)

The interface must remain understandable without animation.

41. Accessibility

Minimum expectations:

semantic landmarks

keyboard-accessible controls

visible focus

sufficient contrast

accessible form labels

accessible errors

accessible dialogs

icon-button labels

table semantics

reduced-motion support

status not conveyed by color alone

Accessibility is part of component design, not final polish.

42. Focus States

Focus must be visible and consistent.

Avoid removing outlines without replacement, barely visible focus rings, and hover-only affordances.

43. Responsive Design

Target:

mobile
tablet
desktop
wide desktop

CareTrack is desktop-oriented operational software, but core workflows must remain usable at smaller widths.

Desktop should use available width for meaningful information density.

44. Density

Default density should be comfortable for scanning and compact enough for operational work.

Tables, filters, and metadata can be denser than forms or page sections.

45. Visual Anti-Patterns

Avoid by default:

marketing-style gradient hero banners

blue-purple SaaS gradients

glassmorphism

neon accents

giant rounded cards

excessive pills

decorative icon bubbles

random shadows

giant typography

stock medical photography

decorative heartbeat lines

fake “AI” glows

meaningless charts

excessive floating panels

card grids without hierarchy

NHS visual imitation

46. Generic AI Dashboard Warning

Question any generated screen that contains:

four equal KPI cards

generic sidebar

giant welcome heading

random chart

floating gradient shapes

pills everywhere

icon circles on every statistic

blue-purple accent gradients

excessive rounded containers

These patterns are allowed only when justified by actual information architecture.

47. Design-System Architecture

Implementation lives under:

src/app/design-system/
├── tokens/
├── components/
├── patterns/
└── styles/

tokens

Semantic status tones, reusable variants, and design constants where TypeScript is appropriate.

components

Reusable visual primitives such as:

button

icon-button

status-chip

form-field

surface

dialog-shell

empty-state

skeleton

patterns

Higher-level reusable UX structures such as:

page-header

patient-identity-banner

data-toolbar

workflow-timeline

styles

Shared design helpers where Tailwind + CSS variables are not enough.

48. Component Promotion Rule

Promote a component when:

it is visually reusable

multiple features can reasonably use it

it has a stable semantic contract

Example:

StatusChip → design system
ReferralTriagePanel → referrals feature

49. Tailwind Usage

Use Tailwind mainly for spacing, layout, responsive behavior, typography sizing, alignment, and state utilities.

Use semantic CSS variables for product colors, surfaces, text hierarchy, borders, statuses, focus, shadows, and radius.

Do not hardcode arbitrary product colors throughout templates.

50. Component CSS

Component CSS is allowed when semantic state styling, complex selectors, reusable variants, or accessibility behavior are clearer there.

Tailwind does not mean “never write CSS”.

51. Initial Design-System Components

Likely early primitives:

StatusChip
Button
IconButton
Surface
FormField
EmptyState
Skeleton

Likely early patterns:

PageHeader
PatientIdentityBanner
DataToolbar
WorkflowTimeline

Do not build all of them before real use cases exist.

52. Visual Exploration Process

Before freezing the final palette and shell treatment:

create 2–3 distinct visual directions

keep the same CareTrack information architecture

compare typography, accent direction, surface hierarchy, density, and navigation treatment

choose one

freeze tokens

remove rejected experiments

AI may help generate exploration variants, but the selected direction becomes the official CareTrack system.

53. Design Review Checklist

Before calling a screen complete, ask:

Is the information hierarchy obvious?

Does it feel like CareTrack rather than a generic template?

Is patient context obvious where relevant?

Is the current workflow state obvious?

Is the primary action obvious?

Are destructive actions appropriately separated?

Can the page be understood without color?

Is the page keyboard usable?

Is focus visible?

Is density appropriate?

Does it work responsively?

Are icons purposeful?

Is animation necessary?

Is any visual element decorative without value?

If yes to the last question, simplify.

54. Design Quality Bar

A successful CareTrack screen should feel:

custom

coherent

calm

operational

trustworthy

polished

accessible

maintainable

The goal is not to look “AI-designed”.

The goal is to look like a thoughtful, visually distinctive clinical operations product built around real workflow needs.

The product should be beautiful enough to feel premium, while remaining calm enough for prolonged daily use.
