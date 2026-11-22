# Rentify agent workflow

## Project context

- Rentify is a .NET 10 solution with API, Application, Domain, Infrastructure, and UnitTests projects, plus a Vue 3/Vite frontend in `RentifyWeb`.
- Keep changes within the accepted issue scope and follow the existing patterns in the affected layer.
- Do not commit, push, merge, or deploy unless the user explicitly asks, except for the Jira workflow's required issue-branch commit and push before `Ready To Prod`.

## Orchestration

For work requested through a Jira-to-implementation-to-test workflow, the coordinating agent owns the handoffs and final status. Use focused roles with these boundaries:

- Rentify Jira issues belong in the existing **Zeus** project (`KAN`); do not create a separate Jira project for Rentify unless the user asks.
- Use the `Görev` issue type for ordinary implementation tasks. Choose another type only when the request clearly calls for it or the user specifies one.
- Do not add a label, component, priority, or assignee unless the user requests it or the Jira project conventions make it clear.
- The current Jira project has no assignable user matching `agent`; developer ownership is represented by the workflow handoff and status unless a dedicated agent account becomes available.

### Jira status and handoff flow

For each Jira task, start from the latest `development` branch and create an isolated branch named `feature/<issue-key>-<short-slug>` before editing. Never implement a task directly on `development` unless the user explicitly asks.

Use the Zeus (`KAN`) board's exact column names when describing workflow stages. The board maps these columns to Jira statuses:

- `To Do` — `Yapılacaklar`
- `In Progress` — `Devam Ediyor`
- `Ready To QA` — `TESTE HAZIR`
- `In QA` — `İNCELEMEDE`
- `Ready To Prod` — `Çözüldü` (use the available `Resolved` transition when it targets this status)
- `Done` — `Tamam`
- `Block` — `Block`

The board configuration confirms the `Ready To Prod` column maps to `Çözüldü`; do not mistake the board column label for the underlying Jira status name. Before changing an issue, inspect its currently available transitions and match the transition's destination status to the desired board column.

1. New issues start in `Yapılacaklar`.
2. When picked up, the developer agent owns the task in the orchestration, creates the task branch from the latest `development`, and moves the issue to `Devam Ediyor`.
3. After implementation, the developer adds a short Jira comment describing the change and the branch name, then moves the issue to the `Ready To QA` board column for QA.
4. The QA agent takes issues in `Ready To QA`, moves each to the `In QA` board column, and independently checks the acceptance criteria and implementation.
5. QA adds a Jira comment with the checks run and their results. Screenshot attachments are optional: record evidence in text and never block or fail QA solely because no image is attached. UI acceptance criteria still need reliable verification, but that verification does not require an uploaded screenshot.
6. If QA passes, the coordinator ensures the accepted changes are committed on the issue branch with a concise commit message and pushes that branch to its upstream remote. Verify the push succeeded before moving the issue to the `Ready To Prod` board column; do not leave accepted changes only in a local working tree. Do not merge into `development` as part of this step.
7. If QA finds a failure, include actionable findings and reproduction details in the comment, then return the issue to `To Do` for another developer attempt.
8. Failed QA cycles return to `Yapılacaklar`; this workflow has no automatic three-attempt `Block` rule.

Only transition when the issue's available transitions permit it. If a required transition is unavailable, report the blocker instead of claiming the status changed.

1. **Jira intake**: clarify the requested outcome, inspect for a likely duplicate when Jira access is available, and create one actionable issue with context, scope, and testable acceptance criteria. Return the issue key and link. If Jira access is unavailable, prepare the issue draft and report that it was not created. If the documented Jira project or issue type is unavailable, ask before creating the issue; never invent a priority or acceptance criteria.
2. **Implementation**: work only from the accepted issue and its criteria. Do not assume behavior or requirements that the user or accepted issue did not specify. Check existing code and project conventions first; if they do not resolve a consequential choice, ask the user before implementing that part. In particular, do not invent authorization rules, API response contracts, defaults, or scope. Add the developer comment and follow the status flow above. Do not mark the issue done.
3. **QA**: independently compare the implementation with every acceptance criterion, inspect the diff, and run relevant checks. QA is read-only: add the test comment and follow the pass/fail status flow above. Do not declare success if required checks were skipped or failed.

The coordinator passes the Jira issue key, acceptance criteria, changed-file summary, and verification evidence at each handoff. The coordinator owns the post-QA commit/push and final Jira transition. Stop and ask for clarification when requirements conflict or a consequential decision is missing. Report the final outcome and any unresolved blocker to the user.

## Verification commands

- Backend and unit tests: `dotnet test Rentify.slnx`
- Frontend tests: from `RentifyWeb`, `npm test`
- Frontend checks: from `RentifyWeb`, `npm run typecheck` and `npm run lint`
- Frontend production build: from `RentifyWeb`, `npm run build`

Run checks relevant to the issue. QA should report the exact commands run and their results; do not describe a check as passing if it was not run.
