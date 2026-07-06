## Document Control

| Field | Value |
|---|---|
| Phase | Inception |
| Status | Draft |
| Milestone Target | End of Inception (LCO) |
| Iteration | 1 (Cycle 1) |
| Author | Project Manager |

## Iteration Objectives

1. **Establish project scope and boundaries** — Confirm the 4 declared use cases (UC-001 Clock In/Out, UC-002 Read News, UC-003 Employee Directory, AD Authentication) and 4 NFRs as the complete scope ceiling
2. **Identify and classify initial risks** — Produce a Risk List with all risks classified by probability × impact, with mitigation and contingency plans
3. **Produce initial architecture candidate** — Software Architect delivers a candidate architecture addressing the primary technical risk (offline fault tolerance)
4. **Author Vision and Use Case Model** — SystemAnalyst produces the Vision Document and Use Case Model capturing stakeholder needs and system use cases
5. **Author Supplementary Specification** — SystemAnalyst captures NFRs (performance, availability, audit, offline tolerance) as formal requirements
6. **Produce initial analysis classes** — Designer creates preliminary analysis classes from the Use Case Model
7. **Assess LCO milestone readiness** — Determine whether the project is viable to proceed to Elaboration

## Plan and Milestones

### Project Context — Coarse Cross-Iteration Roadmap

This section carries the coarse-grained project roadmap. Fine-grained Gantt details are provided ONLY for the current iteration. Subsequent iterations receive fine-grained plans when they become the current or next iteration.

#### Milestone Schedule

| Milestone | Full Name | Target Date | Phase Boundary |
|---|---|---|---|
| LCO | Lifecycle Objective | 2026-07-17 | End of Inception |
| LCA | Lifecycle Architecture | 2026-08-14 | End of Elaboration |
| IOC | Initial Operational Capability | 2026-09-11 | End of Construction |
| PR | Product Release | 2026-09-25 | End of Transition |

#### Iteration Roadmap (6 ± 3 Rule Applied)

| Phase | Iteration | Duration | Calendar Window | Primary Focus |
|---|---|---|---|---|
| Inception | 1 | 2 weeks | Jul 6 – Jul 17 | Scope, risks, architecture candidate, UC model |
| Elaboration | 1 | 2 weeks | Jul 20 – Jul 31 | PoC: offline sync + AD integration; architecture baseline |
| Elaboration | 2 | 2 weeks | Aug 3 – Aug 14 | Complete design for all UCs; data model; UI mockups; test plan |
| Construction | 1 | 2 weeks | Aug 17 – Aug 28 | Implement UC-001 (Clock In/Out + offline), UC-002 (News), AD auth |
| Construction | 2 | 2 weeks | Aug 31 – Sep 11 | Implement UC-003 (Directory); integration; load testing |
| Transition | 1 | 2 weeks | Sep 14 – Sep 25 | Deploy to Windows Server; UAT; adoption tracking |

**Total: 6 iterations** — within the 6 ± 3 rule. Distribution: [1, 2, 2, 1] across phases. Elaboration is stretched to 2 iterations due to high architectural risk (offline fault tolerance + AD integration). Transition is compressed to 1 iteration (internal deployment, no user training program required).

#### Rubber Profile Justification

| Phase | Schedule % | Iteration Count % | Justification |
|---|---|---|---|
| Inception | ~10% | ~17% (1 of 6) | Standard — small scope, clear stakeholder vision |
| Elaboration | ~30% | ~33% (2 of 6) | Stretched — offline fault tolerance and AD integration are high-magnitude risks requiring PoC validation |
| Construction | ~33% | ~33% (2 of 6) | Compressed from 50% — 3 use cases are moderate complexity; .NET 10 + Razor Pages is well-understood |
| Transition | ~17% | ~17% (1 of 6) | Compressed from 10% — internal deployment, no external users, no training program |

#### Agent Role Assignment Profile

| Role | Inception | Elaboration | Construction | Transition |
|---|---|---|---|---|
| SystemAnalyst | **High** | Medium | — | — |
| SoftwareArchitect | **High** | **High** | — | — |
| ProjectManager | **High** | Medium | Medium | **High** |
| Designer | Medium | **High** | Low | — |
| DatabaseDesigner | — | Medium | Medium | — |
| UIDesigner | — | Medium | — | — |
| Implementer | — | — | **High** | Low |
| TestDesigner | — | Medium | **High** | **High** |
| Deployer | — | — | — | **High** |
| Business Modeling | INACTIVE | INACTIVE | INACTIVE | INACTIVE |

**Parallelism note:** Maximum concurrent roles = 7 (Elaboration). This is justified by the need to resolve architectural risk while simultaneously producing design artifacts. No further parallelism increase is planned — coordination overhead would exceed marginal benefit.

### Coarse Roadmap — Milestones and Iteration Flow

![Coarse Roadmap](plantuml:CoarseRoadmap)

### Fine-Grained Gantt — Inception Iteration 1

![Inception Iteration 1 Gantt](plantuml:InceptionGantt)

#### Task Summary

| Task ID | Task | Owner Role | Duration | Start | End | Dependencies |
|---|---|---|---|---|---|---|
| T1 | Conceive project & identify risks | ProjectManager | 2d | Jul 6 | Jul 7 | — |
| T2 | Develop Risk List | ProjectManager | 2d | Jul 8 | Jul 9 | T1 |
| T3 | Develop Iteration Plan + Coarse Roadmap | ProjectManager | 3d | Jul 8 | Jul 10 | T1 |
| T4 | Author Vision Document | SystemAnalyst | 3d | Jul 6 | Jul 8 | — |
| T5 | Author Use Case Model | SystemAnalyst | 4d | Jul 9 | Jul 14 | T4 |
| T6 | Author Supplementary Specification | SystemAnalyst | 3d | Jul 10 | Jul 14 | T4 |
| T7 | Initial Architecture Candidate | SoftwareArchitect | 4d | Jul 9 | Jul 14 | T4 |
| T8 | Initial Analysis Classes | Designer | 3d | Jul 13 | Jul 15 | T5 |
| T9 | Risk List Review & Update | ProjectManager | 1d | Jul 16 | Jul 16 | T2 |
| T10 | Iteration Plan Finalization | ProjectManager | 1d | Jul 16 | Jul 16 | T3 |
| T11 | LCO Milestone Review | ReviewCoordinator | 1d | Jul 17 | Jul 17 | T6, T7, T8, T9, T10 |

## Resources

### Agent Role Assignments — Inception Iteration 1

| Agent Role | Assigned Tasks | Effort Allocation |
|---|---|---|
| ProjectManager | T1, T2, T3, T9, T10 | 40% — risk identification, planning, finalization |
| SystemAnalyst | T4, T5, T6 | 35% — Vision, Use Case Model, Supplementary Spec |
| SoftwareArchitect | T7 | 15% — architecture candidate addressing offline + AD |
| Designer | T8 | 10% — initial analysis classes from UC model |
| ReviewCoordinator | T11 | LCO milestone review at iteration end |

### Infrastructure Resources

| Resource | Status | Notes |
|---|---|---|
| Git/SCM repository | Available | Project repository initialized |
| .NET 10 SDK | Available | Per stakeholder constraint |
| PostgreSQL | Available | On Windows Server (internal) |
| PlantUML tooling | Available | Via process tooling |
| CI/CD pipeline | To configure | Deferred to Elaboration per Development Case |

## Use Cases and Scenarios Addressed

This iteration addresses ALL declared use cases at the **analysis and planning** level — no implementation occurs in Inception.

| Use Case | ID | Iteration Activity | Status at LCO |
|---|---|---|---|
| Clock In/Out | UC-001 | Captured in Use Case Model; analyzed for offline tolerance risk | Analyzed |
| Read News | UC-002 | Captured in Use Case Model | Analyzed |
| Employee Directory | UC-003 | Captured in Use Case Model | Analyzed |
| Active Directory Authentication | (cross-cutting) | Captured in Supplementary Spec as constraint; NOT a separate UC | Specified |

**Scope boundary:** The 4 declared use cases + 4 NFRs constitute the complete scope ceiling. Any additions require a Change Request approved by the CCM. The following are explicitly EXCLUDED: native mobile app, push notifications, payroll integration, vacation/sick-leave management, biometric clocking, external access.

## Evaluation Criteria

### LCO Milestone Exit Criteria

| Criterion | Measurement Method | Target |
|---|---|---|
| Scope agreement | Vision Document reviewed by stakeholders | Laura Gómez confirms scope |
| Risk identification | Risk List completeness review | All 8 identified risks classified with mitigation |
| Architecture candidate | Software Architect delivers candidate architecture | Addresses offline fault tolerance approach |
| Use case coverage | Use Case Model contains all 4 declared UCs | 100% of declared scope modeled |
| NFR specification | Supplementary Specification contains all 4 NFRs | 100% of declared NFRs specified |
| Project viability | Coarse roadmap + iteration plan reviewed | 6-iteration plan within schedule |

### Measurement Goals

| Metric | Goal (Decision Enabled) | Primitive Measure | Frequency |
|---|---|---|---|
| Risk count by magnitude | **Decide:** Which risks require active mitigation in Elaboration | Count of risks per magnitude tier | Per iteration |
| Use case coverage | **Decide:** Whether scope is fully captured before Elaboration | % of declared UCs in Use Case Model | Per iteration |
| NFR coverage | **Decide:** Whether all constraints are formalized | % of declared NFRs in Supplementary Spec | Per iteration |
| Iteration velocity | **Decide:** Whether to adjust next iteration scope | Tasks completed vs. planned | Per iteration |

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| Iteration Plan | Development Case | Derives | Risk List, Elaboration Iteration Plan |
| LCO Milestone | RUP Phase Exit Criteria | Derives | Elaboration Phase Entry |
| Coarse Roadmap | Rubber Profile Heuristic, 6±3 Rule | Derives | All subsequent Iteration Plans |
| Iteration Objectives | Stakeholder Scope, Business Goals | Derives | Iteration Assessment (end of iteration) |
| Evaluation Criteria | Acceptance Criteria (stakeholder) | Derives | LCO Milestone Review |
| UC-001 (Clock In/Out) | Declared Scope | Derives | RISK-T01, RISK-T03, RISK-T04 |
| UC-002 (Read News) | Declared Scope | Derives | RISK-S01 (scope creep guard) |
| UC-003 (Employee Directory) | Declared Scope | Derives | RISK-R01 (AD schema) |
| AD Authentication | Declared Constraint | Derives | RISK-T02, RISK-R01 |