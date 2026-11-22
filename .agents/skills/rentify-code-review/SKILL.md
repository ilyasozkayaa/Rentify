---
name: rentify-code-review
description: >
  Review Rentify code changes or existing code for correctness, security,
  reliability, performance, scalability, maintainability, and architectural
  fit. Use for pull-request reviews, pre-merge checks, bug investigations,
  and focused audits. Do not modify code unless explicitly asked to fix findings.
---

# Rentify Code Review Skill

## Mission

Act as a rigorous senior-level reviewer for a production-oriented,
multi-service rental and booking platform. Find actionable defects and risks,
not stylistic preferences. Prioritize correctness, security, data integrity,
reliability, and operational impact.

## Review mode: read-only by default

- Review the requested diff or scope; do not edit files or apply fixes unless
  the user explicitly asks.
- Read applicable `AGENTS.md` instructions and the relevant
  `rentify-architecture` Skill if available.
- Inspect surrounding code and call sites before concluding that something
  is a defect.
- Distinguish confirmed findings from questions or recommendations.
- Do not invent runtime behavior, test results, or vulnerabilities.
- Do not report speculative issues as definite bugs.

## Rentify context

Rentify is intended to support rentals and bookings across categories such
as vehicles, properties, and hotels. The known technical direction includes
.NET 10, separate Web/MVC and API applications, layered Domain/Application/
Infrastructure responsibilities, CQRS with MediatR, FluentValidation, and
LLM-assisted search-intent extraction.

Verify the actual repository implementation before relying on these notes.
Review against the codebase's established patterns and the
`rentify-architecture` Skill when present.

## Finding severity

Use these levels consistently:

- **Critical:** exploitable security issue, severe data corruption/loss, or
  a production failure with broad and immediate impact.
- **High:** likely correctness, authorization, financial, booking/inventory,
  or reliability defect with significant impact.
- **Medium:** meaningful bug, edge-case failure, resource issue, or
  maintainability problem likely to cause future defects.
- **Low:** limited-impact issue or clear improvement with a concrete benefit.
- **Info:** non-blocking observation or question; do not inflate severity.

A finding should include:

1. Severity.
2. File and line (or the closest available location).
3. The specific problem.
4. Why it matters and a realistic failure scenario.
5. A concrete remediation direction.

Do not include findings without actionable evidence.

## Review checklist

### Correctness and business rules

- Are rental type, location, dates, price ranges, and category-specific
  criteria interpreted and validated correctly?
- Are date ranges valid, ordered, and handled consistently across time zones?
- Are null, empty, boundary, and unexpected values handled safely?
- Are vehicle, property, and hotel rules kept distinct where their semantics
  differ?
- Are business invariants enforced in a reliable layer rather than only in
  the UI or prompt?
- Can invalid or incomplete search intent produce misleading results?
- Are errors and status codes consistent with the API contract?

### Booking, inventory, and data integrity

When the reviewed code touches availability, reservations, inventory,
payments, or booking state, examine:

- Race conditions and concurrent requests.
- Double booking and overselling.
- Transaction boundaries and isolation assumptions.
- Optimistic/pessimistic concurrency handling.
- Idempotency for retried or duplicated commands.
- Correct state transitions and prevention of invalid transitions.
- Monetary precision, currency handling, and rounding.
- Whether external side effects can occur before a database transaction is
  safely committed.
- Whether an outbox or another reliable delivery pattern is warranted for
  cross-system events; recommend it only when the use case justifies it.

Do not assume booking/payment features exist if they are outside the review
scope.

### API and application design

- Are controllers/endpoints thin and focused on transport concerns?
- Is authorization enforced server-side for the relevant resource/action?
- Are handlers cohesive and use-case oriented?
- Are validation rules correct and located at appropriate boundaries?
- Are abstractions and dependency directions consistent with the architecture?
- Are public contracts changed compatibly?
- Are cancellation tokens propagated through request handling and I/O?
- Are exceptions mapped safely and consistently?

### Database and persistence

- Look for N+1 queries, missing filters, unbounded reads, unnecessary
  tracking, inefficient projections, and repeated queries.
- Check pagination, sorting, indexes, and query shape where relevant.
- Check transaction scope and concurrency behavior.
- Check nullability and schema/model consistency.
- Check migrations for data loss, locks, unsafe defaults, and deployment
  ordering risks.
- Check that user-controlled input cannot alter query structure unsafely.

### Performance and scalability

- Identify blocking calls, sync-over-async, thread-pool starvation risks,
  unbounded parallelism, and unbounded queues.
- Look for repeated expensive work, excessive allocations, large object
  graphs, and unnecessary serialization.
- Check external service calls for timeouts, bounded retries, and resilience.
- Check cache keys, expiration, invalidation, and cross-user data isolation.
- Check resource disposal and correct lifetimes for injected services.
- Explain the likely workload and impact; avoid vague "this may be slow"
  claims without a reason.

### Security and privacy

- Check authentication and authorization, including object-level access.
- Check mass assignment/over-posting and trust boundaries.
- Check secrets, tokens, connection strings, and sensitive data in logs.
- Check injection risks, unsafe deserialization, path handling, and
  server-side request forgery where applicable.
- Check rate limiting and abuse controls on expensive endpoints.
- Check that exception responses do not leak stack traces or internals.
- Check personal data minimization and retention in logs and telemetry.

### LLM integration

For prompt/model/provider changes:

- Treat model output as untrusted and validate it before use.
- Check structured-output/schema alignment with application validators.
- Check enum handling, missing fields, invalid dates, contradictory criteria,
  and unsupported rental types.
- Ensure model output cannot claim actual availability, price, or booking
  success without trusted backend confirmation.
- Check prompt injection exposure when user content or retrieved content is
  included in prompts.
- Check token usage, duplicated instructions, model selection, request
  limits, timeouts, retries, and provider cost.
- Check sensitive-data handling and logging.
- Check fallback behavior for malformed output, provider outage, quota, and
  rate-limit errors.
- Do not recommend blindly retrying non-idempotent operations.

### Tests and observability

- Are important success, failure, boundary, and concurrency cases tested?
- Do tests assert meaningful behavior rather than implementation details?
- Are external dependencies isolated appropriately?
- Are logs and metrics useful without exposing sensitive information?
- Can failures be diagnosed through correlation/request identifiers where
  the existing system supports them?


## Formatting and Whitespace Review

Check modified files for:

- Unnecessary line breaks.
- Unnecessary blank lines.
- Multiple consecutive blank lines.
- Trailing spaces or tabs.
- Whitespace-only lines.
- Missing or excessive final newlines.
- Unnecessary formatting changes unrelated to the task.

Follow existing project formatting conventions.
Do not recommend broad reformatting of unrelated files.

## User Testing Marker Review

- Check whether newly implemented methods intended for manual user testing have the exact `//ilyas` comment immediately above their declarations.
- Do not require this marker on every method.
- Do not flag existing methods unrelated to the current changes.
- Report a missing marker as a Low severity finding when applicable.


## Avoid noisy findings

Do not flag:

- Formatting or naming preferences without a meaningful consequence.
- Missing abstractions solely because an interface could be added.
- Hypothetical performance issues without a plausible workload or code path.
- Missing tests for behavior outside the requested scope unless the risk is
  material.
- Existing issues unrelated to the changed code, unless the user requested
  a broader audit.

If the review scope is a diff, prioritize regressions introduced by that diff.
Mention pre-existing issues separately and label them clearly.

## Required output format

Start with a short verdict:

- **No findings:** say so only after reviewing the requested scope.
- **Findings:** summarize the highest-severity risks first.

Then list findings in severity order:

### [Severity] Short finding title
- **Location:** `path/to/file.cs:line`
- **Problem:** What is wrong.
- **Impact:** A realistic consequence.
- **Recommendation:** A concrete fix or mitigation.

Finish with:

- **Tests:** commands run and actual outcomes; otherwise state "Not run".
- **Scope/limitations:** files or behaviors not inspected, if relevant.
- **Residual risks:** only material unresolved risks.

If no actionable findings exist, do not manufacture any. You may provide a
brief positive assessment and note any untested areas.
