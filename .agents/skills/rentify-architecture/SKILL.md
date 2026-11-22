---
name: rentify-architecture
description: >
  Apply and protect Rentify's established architecture when exploring,
  designing, implementing, or changing code. Use for new features, refactors,
  dependency registration, API and MVC changes, persistence, integrations,
  and architectural reviews.
---

# Rentify Architecture Skill

## Mission

Make changes that fit Rentify's existing architecture and are suitable for
production. Preserve established decisions unless the user explicitly approves
a change. Prefer the smallest coherent change over unnecessary restructuring.

## First: inspect the repository

Before proposing or changing code:

1. Read the root `AGENTS.md` if present and any more-specific `AGENTS.md`
   files in the directories you will touch.
2. Inspect the solution, project references, target frameworks, folder layout,
   existing implementations, tests, configuration, and dependency registration.
3. Treat the repository as the source of truth. The project notes below are
   known design context, not permission to assume every item is already
   implemented exactly as described.
4. Reuse existing abstractions and conventions. Do not create a parallel
   pattern merely because it is familiar.
5. If the actual repository conflicts with this Skill, report the conflict and
   follow the current code unless the user authorizes a migration.

## Known project direction

Rentify is a multi-service rental and booking platform. Search should support
natural-language requests across rental categories such as vehicles,
properties, and hotels, rather than forcing users into unrelated search flows.

Known architectural direction:

- .NET 10.
- Separate Web/MVC and API applications.
- Web/MVC is a presentation layer: validate presentation input as needed and
  communicate with the API; do not place domain or application business logic
  in MVC controllers.
- The API owns HTTP endpoints and authorization.
- Layered solution with Domain, Application, Infrastructure, API, and Web
  responsibilities. Confirm actual project names and references in the
  repository before editing.
- CQRS with MediatR for application use cases.
- FluentValidation and an application validation pipeline where appropriate.
- Centralized exception handling and consistent API error responses.
- Scalar as the preferred API reference UI; do not introduce Swagger solely
  by habit.
- Infrastructure services such as the database and Redis may run through
  Docker. Confirm current compose files and actual providers before relying
  on them.
- LLM-based search-intent extraction is part of the product direction.
  Keep prompts, schemas, and provider integration isolated behind suitable
  abstractions where the current design supports that separation.

## Layer responsibilities

Use the existing project boundaries. The intended responsibilities are:

- **Domain:** business concepts, invariants, domain behavior, and domain
  events where justified. Must not depend on Infrastructure, API, or Web.
- **Application:** use cases, commands/queries, handlers, application
  abstractions, DTOs, validation, and orchestration. Avoid framework or
  persistence details leaking into business workflows.
- **Infrastructure:** implementations for persistence, external providers,
  messaging, caching, and other technical integrations. Depend inward on
  abstractions where the existing architecture defines them.
- **API:** transport concerns, endpoint/controller binding, authentication/
  authorization integration, HTTP status mapping, and middleware. Keep
  business decisions out of controllers.
- **Web/MVC:** presentation, view models, UI validation, and API communication.
  Do not bypass the API to access application or infrastructure internals.

Do not force every feature into every layer. Add only the components the
feature genuinely needs.

## Dependency and design rules

- Keep dependencies pointing inward. Domain should remain independent.
- Depend on abstractions at architectural boundaries; avoid interfaces for
  trivial implementation details without a real substitution or testing need.
- Register services in the composition root or established dependency
  registration extensions. Avoid scattered service-locator patterns.
- Respect existing MediatR request/handler conventions.
- Keep controllers thin. Handlers should coordinate use cases, not become
  dumping grounds for unrelated business rules.
- Put business invariants in the domain model or an appropriate domain
  service, not only in request validators.
- Use clear, explicit names and cohesive types. Avoid speculative generic
  frameworks and premature abstractions.
- Do not introduce a new library, architectural pattern, project, or service
  without explaining its value, operational cost, and alternatives.

## Async, reliability, and resource use

- Use async APIs for I/O-bound work; do not wrap synchronous I/O in
  `Task.Run`.
- Propagate `CancellationToken` from the entry point through handlers and
  I/O operations when supported.
- Avoid `.Result`, `.Wait()`, sync-over-async, and unbounded fire-and-forget.
- Apply explicit timeouts and bounded concurrency to external calls where
  appropriate.
- Consider retries only for transient failures and only when operations are
  safe to retry. Use backoff and avoid retry storms.
- Make write operations idempotent where duplicate requests or message
  delivery can occur.
- Consider transaction boundaries, concurrency conflicts, and consistency
  requirements before implementing multi-step writes.
- Avoid N+1 queries, unnecessary materialization, over-fetching, and
  unbounded collections.
- Use pagination for potentially large result sets.
- Introduce caching only with a clear key, expiration, invalidation, and
  consistency strategy. Do not cache user-specific or sensitive data
  incorrectly.
- Avoid unnecessary allocations and repeated serialization, but do not
  sacrifice clarity for unmeasured micro-optimizations.

## LLM-specific principles

For LLM-backed functionality:

- Treat model output as untrusted input. Validate and normalize it before use.
- Use structured outputs/schemas where supported; validate required fields,
  enum values, dates, ranges, and cross-field constraints in application code.
- Never let the model invent availability, prices, inventory, or booking
  confirmations. Those must come from trusted application data/services.
- Keep system instructions concise and focused. Avoid duplicating domain
  rules across prompts, code, and schemas; establish a clear source of truth.
- Minimize token usage without removing instructions needed for correctness.
- Set appropriate timeouts, cancellation, and failure handling.
- Do not log API keys, secrets, full sensitive prompts, or personal data.
- Consider rate limits, provider errors, cost ceilings, observability, and
  fallback behavior.
- Keep provider-specific code behind the existing integration boundary.

## Configuration and security

- Never hardcode secrets, API keys, passwords, or connection strings.
- Use the repository's established configuration and secret-management
  approach; do not commit local secrets.
- Validate configuration at startup when practical.
- Validate untrusted input at system boundaries.
- Enforce authorization at the API/application boundary appropriate to the
  existing design; never rely on hiding UI elements as authorization.
- Avoid exposing internal exception details or sensitive data in API errors.
- Use parameterized database access and safe serialization defaults.


## Code Formatting and Whitespace Rules

- Follow the formatting conventions already used in the surrounding files.
- Do not introduce unnecessary line breaks.
- Avoid splitting short method signatures, parameter lists, simple expressions and fluent calls across multiple lines without a readability reason.
- Do not add unnecessary blank lines between statements, methods or sections.
- Never leave trailing spaces or tabs at the end of lines.
- Never leave whitespace-only lines.
- Avoid multiple consecutive blank lines.
- Keep formatting changes minimal and focused.
- Do not reformat unrelated files or existing code unnecessarily.
- Ensure every modified text file ends with exactly one newline character.

## User Manual Testing Markers

When implementing a method specifically intended for the user's manual testing, add the exact comment `//ilyas` immediately above the method declaration.

Example:

//ilyas
public async Task<SearchResult> SearchAsync(CancellationToken cancellationToken)
{
    // Implementation
}

Rules:
- Apply `//ilyas` only to methods the user is expected to manually test.
- Do not add this comment to every method.
- Do not add it to constructors, generated code or internal helper methods.
- Preserve the exact lowercase spelling: `//ilyas`.
- If multiple methods are independently intended for manual testing, mark each one.

## Testing expectations

- Add or update tests for changed behavior.
- Test domain invariants and application use cases at the appropriate level.
- Test validation failures and important boundary conditions.
- For external integrations, prefer deterministic tests with mocked/faked
  boundaries; add integration tests where they provide meaningful coverage.
- Do not claim tests passed unless they were actually run. Report commands
  and results accurately.
- Do not weaken or delete tests merely to make a change pass.

## Change workflow

For a requested implementation:

1. Summarize the relevant existing design and identify the files likely to
   change.
2. State material assumptions or architectural conflicts before making a
   high-impact change.
3. Implement the smallest complete solution consistent with the repository.
4. Add/update tests and configuration/documentation as needed.
5. Run relevant formatting, build, and test commands when available.
6. Review the diff for unintended changes, dependency direction, security,
   performance, and operational impact.
7. Report what changed, what was tested, remaining risks, and any follow-up.

For analysis-only requests, do not modify files.

## Approval boundaries

Ask before:

- Changing project boundaries or project references.
- Introducing/removing major infrastructure, packages, or architectural
  patterns.
- Changing public API contracts or persisted data formats in a breaking way.
- Performing destructive database, file, or migration operations.
- Replacing an established implementation with a materially different one.

Do not silently "modernize" unrelated code while implementing a feature.
