<!--
Sync Impact Report
- Version change: placeholder → 1.0.0
- Modified principles: n/a → 5 principles defined
- Added sections: Security Requirements, Development Workflow
- Removed sections: none
- Follow-up TODOs: none
-->

# ContosoDashboard Constitution

## Core Principles

### I. Secure-by-Default Access
The project MUST treat security as a non-negotiable requirement for every feature. Authentication, authorization, and data access checks MUST be enforced at both the page and service layers so users see only data they are allowed to access. This protects training scenarios from common IDOR and authorization bypass issues while preserving the app’s learning objectives.

### II. Offline-First Architecture
ContosoDashboard MUST remain compatible with offline, local-only development. Work must rely on local filesystem and local database patterns unless a feature explicitly adds an isolated cloud abstraction. The project MUST prefer infrastructure abstractions over direct environment coupling, so future migration remains possible without rewriting business logic.

### III. Evidence-Driven Delivery
Every feature change MUST be validated with fresh evidence before completion. The team MUST run the relevant restore, build, and startup verification commands, and MUST record whether the project still builds cleanly and starts without runtime errors. This requirement prevents silent regressions and ensures all work is grounded in observed behavior.

### IV. Minimal-Complexity Design
The project MUST favor the simplest design that satisfies the current feature and training goal. New abstractions, services, or frameworks MUST be justified by real complexity or a documented migration need. The application architecture MUST remain understandable to students and maintainable by small teams working within a single Blazor Server application.

### V. Traceable Feature Work
Every user-visible improvement MUST be grounded in a concrete specification, plan, and task trail. Feature work MUST be broken down so it can be understood, tested, and reviewed independently. This reduces ambiguity and preserves the repository’s purpose as a Spec-Driven Development example.

## Security Requirements

- The application MUST protect authenticated and non-authenticated routes with explicit authorization checks.
- Role-based policies MUST be used for employee, team lead, project manager, and administrator access paths.
- Service-layer authorization MUST validate access before returning project, task, or notification data.
- Files stored for feature work MUST remain outside the web root unless a feature explicitly requires a different approved pattern.
- Generated storage paths MUST avoid direct user-controlled naming that can create path traversal or duplicate-key issues.
- Mock authentication MUST remain clearly marked as training-only and MUST NOT be treated as production identity infrastructure.
- Security headers and safe defaults MUST remain enabled for the web application.

## Development Workflow

- New work MUST begin with a feature specification that states the user value, scope, and acceptance criteria.
- The technical plan MUST describe the architecture and implementation approach before coding begins.
- Tasks MUST be organized so implementation can proceed in coherent, independently verifiable increments.
- Code changes MUST be validated with the smallest relevant build or runtime check before completion.
- Documentation updates MUST reflect user-facing behavior and architectural changes when they affect the app’s training value or operating model.

## Governance

This Constitution governs all repository decisions related to product behavior, security posture, architecture, and feature planning. When a conflict exists between this document and convenience, speed, or local preference, the Constitution takes precedence. All changes must preserve the project’s educational purpose, training-safe architecture, and security expectations.

Amendments require a documented rationale, a version update, and a clear statement of the effect on the project. Compliance review is expected for major changes to security, storage, authentication, and project structure. The project MUST remain aligned with the repository’s Spec Kit workflow: specification, planning, tasking, implementation, and evidence-based verification.

**Version**: 1.0.0 | **Ratified**: 2026-09-15 | **Last Amended**: 2026-09-15
