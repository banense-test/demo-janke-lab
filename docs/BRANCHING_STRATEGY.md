# Branching Strategy — Employee Portal (Cuba Corp)

**Project:** Demo Janke Lab — Employee Portal  
**Phase:** Elaboration | **Iteration:** 3 | **Cycle:** 1  
**Owner:** Configuration Manager  
**Last Updated:** 2026-07-08  

---

## 1. Purpose

This document defines the canonical branching model, naming conventions, baseline
procedure, and change-control integration for the Employee Portal project. It is
**config-as-code** — committed directly to `main` via `scm_commit_files`, never opened
as a PR. All roles (Implementer, Integrator, Reviewer, Architect) consume this file as
the authoritative source for branch and tag conventions.

**RUP Anchor:** RUP Ch.13 — *Manage Baselines and Releases*: baselines are created at
ends of iterations and at project and delivery milestones. Naming conventions
facilitate communication in larger projects.

---

## 2. Configuration Item Identification Scheme

| CI Category | Identification Scheme | Example |
|---|---|---|
| Source code | File path in Git repository | `src/Portal/Services/ClockService.cs` |
| RUP artifacts | Artifact name (canonical, validated by upsert) | `Vision Document`, `Use Case Model` |
| Branches | `{prefix}/{identifier}` (see §3) | `feature/C1-UC01-clock-in-out` |
| Baseline tags | `baseline-{phase}{n}-v{x}` (see §5) | `baseline-elaboration-E3-v1` |
| Change Requests | GitHub Issues with `change-request` label | Issue #42 |
| CI pipeline | `.github/workflows/{name}.yml` | `ci-build.yml` |
| Documentation | `docs/{FILENAME}.md` | `docs/BRANCHING_STRATEGY.md` |
| Architecture decisions | ADR records in SAD | `ADR-001`, `ADR-002`, `ADR-003` |
| Design mechanisms | Mechanism entries in SAD | `MECH-001` (persistence), `MECH-002` (auth) |
| Test artifacts | Test Case IDs | `TC-001`, `TC-002` |

---

## 3. Branch Naming Conventions

| Prefix | Pattern | Purpose | Lifecycle |
|---|---|---|---|
| `poc/` | `poc/E{n}-{risk-id}-{mechanism}` | Elaboration proof-of-concept spikes | Ephemeral — **never merged to main** |
| `feature/` | `feature/C{n}-{uc-id}-{subject}` | Construction feature implementation | Merged into `iteration/C{n}` via PR |
| `iteration/` | `iteration/C{n}` | Integration workspace per iteration | Merged into `main` via iteration-close PR |
| `hotfix/` | `hotfix/{issue-id}` | Transition hotfixes | Merged directly into `main` via PR |
| `chore/` | `chore/{subject}` | Non-functional repo maintenance | Merged directly into `main` via PR |

**Non-conforming branches** are surfaced as SCM issues with `severity:minor` +
`nature:defect` + `naming-violation` labels.

### 3.1 Current Branch Inventory (Iteration 3)

| Branch | Type | Status | CI | Notes |
|---|---|---|---|---|
| `main` | Integration target | Active | GREEN (run 28869060862) | No E3 baseline tag yet |
| `poc/E1-risk-t01-offline-sync` | PoC | PR #4 open → main | GREEN (run 28860807083) | **SAD-F4: must NOT merge to main** — PoC branches are ephemeral |

---

## 4. Branching Topology

```plantuml
@startuml
title Employee Portal — Branching Topology (Elaboration Iteration 3)
skinparam component {
  BackgroundColor #F2F2F2
  BorderColor #333333
}
skinparam note {
  BackgroundColor #FFFDE7
  BorderColor #BFA600
}

package "main (integration target)" {
  [main] as MAIN
  note right of MAIN: CI: GREEN (run 28869060862)\nNo baseline tag yet for E3\nLCA gate BLOCKED (SAD-F4)
}

package "PoC Branches (ephemeral, never merged)" {
  [poc/E1-risk-t01-offline-sync] as POC1
  note right of POC1: PR #4 open → main\nready-for-review\nCI: GREEN (run 28860807083)\nSAD-F4: must NOT merge to main
}

package "Iteration Branches (future)" {
  [iteration/C1] as ITER_C1
  note right of ITER_C1: Not yet created\nConstruction Iteration 1\nIntegrator opens when ready
}

package "Feature Branches (future)" {
  [feature/C1-UC01-clock-in-out] as FEAT1
  [feature/C1-UC02-read-news] as FEAT2
  [feature/C1-UC03-employee-directory] as FEAT3
  note bottom of FEAT1: Construction phase\nNot yet created
}

POC1 ..> MAIN : PR #4 (must close without merge)
ITER_C1 ..> MAIN : future iteration-close PR
FEAT1 ..> ITER_C1 : feature → iteration
FEAT2 ..> ITER_C1 : feature → iteration
FEAT3 ..> ITER_C1 : feature → iteration

@enduml
```

---

## 5. Baseline Tagging Procedure

### 5.1 Tag Naming Convention

| Phase | Tag Pattern | Example |
|---|---|---|
| Elaboration | `baseline-elaboration-E{n}-v{x}` | `baseline-elaboration-E3-v1` |
| Construction | `baseline-construction-C{n}-v{x}` | `baseline-construction-C1-v1` |
| Transition | `baseline-transition-T{n}-v{x}` | `baseline-transition-T1-v1` |

- `{n}` = iteration number (integer, starting at 1)
- `{x}` = patch version (integer, starting at 1)
- Re-tag (`v2`, `v3`, …) only after explicit rollback or post-baseline critical fix

### 5.2 Pre-Tag Audit Gate (MANDATORY)

Before any `scm_create_tag`, the Configuration Manager MUST verify:

1. **Review Gate:** `scm_get_pull_request_review_state(projectId, prNumber) == "APPROVED"`
   on the iteration-close PR
2. **CI Gate:** `scm_get_build_status(projectId, "main") == green` after the merge

Either fails → file an Issue (`severity:blocker` + `nature:defect` + kind label) and
DO NOT tag.

### 5.3 Tag Message (Audit Record)

The tag message MUST contain:
- Iteration-close PR number and head commit SHA
- Architect approval review ID
- `main` CI run URL at tag time
- Any notable findings (naming violations, deferred items, re-tag justifications)

### 5.4 Current Baseline Status (Iteration 3)

| Baseline | Tag | Status | Gate |
|---|---|---|---|
| Elaboration E1 | `baseline-elaboration-E1-v1` | Written (Iteration 1) | Passed |
| Elaboration E2 | `baseline-elaboration-E2-v1` | Written (Iteration 2) | Passed |
| Elaboration E3 | `baseline-elaboration-E3-v1` | **NOT YET WRITTEN** | **BLOCKED** — no iteration-close PR; SAD-F4 (Critical) open |

**Blocking conditions for E3 baseline:**
1. No iteration-close PR (`iteration/Cn → main`) has been opened by the Integrator
2. SAD-F4 (Critical): PR #4 (PoC code) is open against main — must be closed without merging
3. IA-F2 (Major): Iteration Assessment not updated for Iteration 2

The LCA milestone gate remains CLOSED per the Review Coordinator's verdict.

---

## 6. Baseline Pedigree State Machine

```plantuml
@startuml
title Baseline Pedigree State Machine — Elaboration Iteration 3
skinparam state {
  BackgroundColor #E2EFDA
  BorderColor #333333
}

[*] --> NoBaseline

state "No Baseline (E3)" as NoBaseline {
  NoBaseline : Elaboration Iteration 3 in progress
  NoBaseline : No iteration-close PR opened yet
  NoBaseline : LCA gate BLOCKED by SAD-F4 (Critical)
  NoBaseline : Main CI: GREEN
  NoBaseline : PoC PR #4 open — must not merge
}

NoBaseline --> GateCheck : [Integrator opens iteration-close PR]

state "Gate Check" as GateCheck {
  GateCheck : scm_get_pull_request_review_state == APPROVED?
  GateCheck : scm_get_build_status("main") == green?
  GateCheck : SAD-F4 resolved? (PoC PR closed)
}

GateCheck --> TagBaseline : [ALL gates pass]
GateCheck --> Escalate : [any gate fails]

state "Tag: baseline-elaboration-E3-v1" as TagBaseline {
  TagBaseline : scm_create_tag with audit message
  TagBaseline : Audit: PR number, head SHA, review ID, CI URL
  TagBaseline : Pedigree: all commits from APPROVED PRs
}

TagBaseline --> [*]

state "Escalate: File Blocker Issue" as Escalate {
  Escalate : scm_create_issue(severity:blocker, nature:defect)
  Escalate : Wait for gate clearance, re-check next invocation
}

Escalate --> [*]

@enduml
```

---

## 7. Configuration Manager State Machine (Iteration 3)

```plantuml
@startuml
title Configuration Manager — Elaboration Iteration 3 State
skinparam state {
  BackgroundColor #E2EFDA
  BorderColor #333333
}

[*] --> S1_DISCOVER

state "S1: Load Architecture + SCM State" as S1_DISCOVER {
  S1_DISCOVER : list_artifacts(projectId)
  S1_DISCOVER : read_artifact(Review Record)
  S1_DISCOVER : scm_get_file_content("docs/BRANCHING_STRATEGY.md")
  S1_DISCOVER : scm_list_issues(projectId, "open")
  S1_DISCOVER : scm_list_pull_requests(projectId, "all")
  S1_DISCOVER : scm_get_build_status(projectId, "main")
  S1_DISCOVER : exit: 13 artifacts loaded, 1 PR (#4 PoC), main CI green, 7 open CRs
}

S1_DISCOVER --> c_lam_pr

state c_lam_pr <<choice>>
c_lam_pr --> S2_GATE : [iteration-close PR targeting main exists]
c_lam_pr --> S_UPDATE : [no iteration-close PR — update BRANCHING_STRATEGY.md and exit]

state "S_UPDATE: Update Branching Strategy" as S_UPDATE {
  S_UPDATE : Evolve docs/BRANCHING_STRATEGY.md for Iteration 3
  S_UPDATE : Update iteration metadata, baseline status, open findings
  S_UPDATE : scm_commit_files to main (no PR — docs are commits)
  S_UPDATE : Verify branch naming compliance
  S_UPDATE : exit: strategy updated, naming verified
}

S_UPDATE --> [*]

state "S2: Pre-Tag Gate Verification" as S2_GATE {
  S2_GATE : scm_get_pull_request_review_state(projectId, prNumber)
  S2_GATE : scm_get_build_status(projectId, "main")
  S2_GATE : exit: both gate checks recorded
}

S2_GATE --> c_gates

state c_gates <<choice>>
c_gates --> S3_TAG : [review == APPROVED AND main CI == green]
c_gates --> S_ESCALATE : [review != APPROVED OR main CI != green]

state "S3: Write Architecture Baseline Tag" as S3_TAG {
  S3_TAG : scm_create_tag(projectId, "baseline-elaboration-E3-v1", audit_message)
  S3_TAG : audit_message contains: pr.number, head SHA, review ID, CI URL
  S3_TAG : exit: tag written, pedigree defensible
}

S3_TAG --> [*]

state "S_ESCALATE: File Gate-Failure Issue" as S_ESCALATE {
  S_ESCALATE : scm_create_issue(projectId, title, body, labels)
  S_ESCALATE : labels: severity:blocker, nature:defect, missing-approval OR ci-broken-on-main
  S_ESCALATE : exit: blocker filed
}

S_ESCALATE --> [*]

@enduml
```

---

## 8. Change Control Integration

### 8.1 CR Label Convention

| Label | Meaning |
|---|---|
| `change-request` | Issue is a formal Change Request |
| `cr:new` | Newly logged, awaiting triage |
| `cr:logged` | Triaged and logged in Risk List / artifacts |
| `cr:approved` | Approved by CCB (Change Control Manager) |
| `cr:complete` | Implemented and verified |
| `severity:blocker` | Blocks baseline / milestone gate |
| `severity:major` | Major impact, must resolve before gate |
| `severity:minor` | Minor impact, can defer |
| `nature:defect` | Defect in existing work product |
| `nature:enhancement` | Enhancement to existing work product |
| `impact:architectural` | Affects architecture decisions |
| `impact:cross-cutting` | Affects multiple components |
| `impact:local` | Localized to single component |
| `naming-violation` | Branch/PR naming convention violation |
| `needs-architect-review` | Requires Architect evaluation |

### 8.2 Open Change Requests (Iteration 3)

| Issue # | Title | Severity | Status | Assigned |
|---|---|---|---|---|
| #1 | CR-001: Update Vision Document Control iteration marker | Minor | cr:approved | (deferred F6) |
| #2 | CR-002: Update Iteration Assessment objective statuses | Minor | cr:approved | (deferred F7) |
| #3 | CR-003: Formalize design file impact assessment | Major | cr:logged | needs-architect-review |
| #5 | CR: PoC architecture validation tests excluded from CI | Major | cr:approved | assigned:implementer |
| #6 | CR: Main branch SmokeTest.cs is placeholder | Minor | cr:approved | assigned:implementer |
| #7 | CR: TcpHealthMonitor sync-over-async pattern | Major | cr:logged | needs-architect-review |
| #8 | CR: SqliteLocalStore reflection on init-only properties | Minor | cr:logged | needs-architect-review |

**No `severity:blocker` issues are open.** The LCA gate is blocked by Review Record
findings (SAD-F4, IA-F2), not by SCM issues.

---

## 9. Open Review Findings Impacting Baseline

| Finding ID | Artifact | Severity | Impact on Baseline |
|---|---|---|---|
| SAD-F4 | Software Architecture Document | **Critical** | PR #4 (PoC) must not merge to main — blocks LCA gate; baseline cannot be written until resolved |
| IA-F2 | Iteration Assessment | **Major** | Iteration Assessment not updated — blocks LCA gate independently |
| DM-MR-F1 | Design Model | Minor | Deferred to Construction Iter 1 — no baseline impact |
| IP-F1 | Iteration Plan | Minor | Metadata typo — no baseline impact |
| IA-F1 | Iteration Assessment | Minor | Stale objectives — no baseline impact |
| PoC-F1 | Architectural Proof-of-Concept | Minor | LAM typo — no baseline impact |

**Baseline gate assessment:** The E3 baseline tag CANNOT be written until:
1. SAD-F4 is resolved (PR #4 closed without merging)
2. IA-F2 is resolved (Iteration Assessment updated)
3. An iteration-close PR is opened, reviewed, and APPROVED
4. Post-merge `main` CI is GREEN

---

## 10. Naming Compliance Audit

| Branch | Conforms? | Pattern Matched |
|---|---|---|
| `main` | ✅ | Default branch |
| `poc/E1-risk-t01-offline-sync` | ✅ | `poc/E{n}-{risk-id}-{mechanism}` |

No naming violations detected in Iteration 3.

---

## 11. Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| Branch naming conventions | RUP Ch.13 (Manage Baselines and Releases) | Derives | Implementer, Integrator, Reviewer workflows |
| Baseline tag convention | RUP Ch.13 (baseline at iteration close) | Derives | scm_create_tag operations |
| Pre-tag audit gate | RUP Ch.13 (baseline integrity) | Derives | scm_get_pull_request_review_state, scm_get_build_status |
| Change control integration | RUP Ch.13 (Change Control Board) | Derives | GitHub Issues (cr:* labels) |
| CI item identification | Development Case (Tool Assessment) | Refines | .github/workflows/ |
| Branching topology diagram | RUP Ch.13 (workspace hierarchy) | Derives | Integrator, Implementer branch creation |
| Baseline pedigree state machine | RUP Ch.13 (baseline procedure) | Derives | Configuration Manager workflow |
| Elaboration baseline convention | SAD (LCA milestone target) | Refines | baseline-elaboration-E3-v1 tag (pending) |
| PoC branch evidence | Architectural Proof-of-Concept (PoC-1) | Derives | CI run 28860807083 |
| SAD-F4 finding | Review Record (Elaboration Iter 2) | Reviews | PR #4, LCA gate, E3 baseline |
| IA-F2 finding | Review Record (Elaboration Iter 2) | Reviews | Iteration Assessment, LCA gate |
| Open CRs (#1-#8) | Change Control Manager triage | Derives | Branch creation, PR authorization |
