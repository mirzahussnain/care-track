# CareTrack

## Clinical Referral & Workflow Management System

## 1. Overview
CareTrack is a portfolio demonstration of an internal clinical referral and workflow management application. It is intended to show a structured referral lifecycle using synthetic data in a controlled learning context.

## 2. Purpose
The project is designed to demonstrate software engineering planning, architecture, and delivery practices for a referral workflow domain. It is an educational portfolio project and not a production healthcare system.

## 3. Project Status
**Planning / Architecture Stage**

**No application implementation has started yet.**

## 4. Proposed Referral Workflow
Proposed lifecycle:

Patient → Referral Created → Submitted → Awaiting Triage → Accepted → Assigned → Appointment Scheduled → In Progress → Completed

Possible alternative states include:
- More Information Required
- Rejected
- Cancelled

## 5. Proposed User Roles
1. **Coordinator / Administrator**
   - Future focus: patient/referral administration, assignment, scheduling, queue monitoring.
2. **Clinician**
   - Future focus: triage, reviewing referrals, case notes, workflow updates, completion.
3. **Service Manager**
   - Future focus: dashboards, monitoring, audit visibility, workload and delay insights.

## 6. Planned Technology Stack
The following technologies are **planned for future implementation** and are **not necessarily implemented yet**.

- **Frontend (planned):** Angular, TypeScript, JavaScript, HTML, CSS, Angular Router, Reactive Forms, NgRx, accessible responsive UI.
- **Backend (planned):** C#, .NET / ASP.NET Core Web API, REST, OOP, SOLID, Dependency Injection, EF Core, LINQ, OpenAPI/Swagger.
- **Database (planned):** Microsoft SQL Server, SQL/T-SQL, EF Core migrations.
- **Authentication (planned):** OAuth 2.0, OpenID Connect, role-based authorization, possible Microsoft Entra ID integration.
- **Testing (planned):** xUnit, ASP.NET Core integration testing, Angular testing, Playwright E2E.
- **Infrastructure (planned):** Windows Server, IIS, Azure, GitHub Actions.
- **Approach (planned):** Agile, user stories, acceptance criteria, technical documentation, architecture diagrams.

## 7. Planned System Architecture
A planned layered architecture is documented in `/docs/03-system-architecture.md`:
- Angular SPA
- ASP.NET Core Web API
- Application and Domain layers
- Infrastructure with Entity Framework Core
- Microsoft SQL Server
- OpenID Connect identity integration

## 8. Repository Structure
This repository currently contains **planning documentation and skeleton folders only**.

```text
CareTrack/
├── backend/
├── frontend/
├── database/
├── docs/
└── .github/
```

## 9. Development Roadmap
Planned next phases:
1. Requirements refinement
2. Architecture and data design validation
3. Framework initialization
4. Incremental feature implementation
5. Testing and deployment setup

## 10. Documentation
- `docs/01-requirements.md`
- `docs/02-user-stories.md`
- `docs/03-system-architecture.md`
- `docs/04-database-design.md`
- `docs/05-api-design.md`
- `docs/06-authentication-authorization.md`
- `docs/07-clinical-risk.md`
- `docs/08-testing-strategy.md`
- `docs/09-security.md`
- `docs/10-deployment.md`

## 11. Data & Security Notice
CareTrack is designed for synthetic data only. No real patient data, production credentials, or live healthcare integrations should be used in this portfolio project.

## 12. Disclaimer
**CareTrack is an independent portfolio demonstration using entirely synthetic data. It is not affiliated with, commissioned by, or endorsed by the NHS, University Hospitals Plymouth NHS Trust, or any other healthcare provider.**
