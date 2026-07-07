## Document Control

| Field | Value |
|---|---|
| Phase | Elaboration |
| Status | Draft |
| Iteration | 1 (Cycle 1) |
| Milestone Target | End of Elaboration (LAM) |
| Author | Implementer (contributor) — Software Architect is primary owner |
| Branch | `poc/E1-risk-t01-offline-sync` |
| CI Status | Green (3/3 pushes passed) |

## Objective and Risks Addressed

### PoC-1: Offline Sync Mechanism (RISK-T01, RPN 63 — High)

**Risk:** If the corporate network drops for up to 5 minutes, the system must continue to accept clock in/out operations and sync data once the network is restored. No data loss is acceptable. (REQ-014, NFR: Offline Fault Tolerance)

**Hypothesis:** The layered architecture with `INetworkHealth` → `SyncQueue` → `ILocalStore` (SQLite) → `IRepository<Clocking>` (PostgreSQL) can reliably buffer clockings during network outage and flush them with conflict detection on restore, achieving zero data loss.

**Architectural mechanism validated:**
- `INetworkHealth` (INT-005) — TCP probe to PostgreSQL port 5432
- `SyncQueue` (ACL-013, COMP-D4) — SemaphoreSlim(1,1) single-writer lock, flush-on-restore
- `ILocalStore` (INT-003, COMP-I3) — SQLite in-memory/offline buffer
- `IRepository<Clocking>` (INT-002, COMP-I2) — Primary store abstraction
- `TimeTrackingService` (ACL-009, COMP-A1) — Online/offline path switching with transient failure fallback

**Related risks also partially addressed:**
- RISK-T03 (consequence of RISK-T01) — Sync strategy validated by flush mechanism

## Approach

### PoC Structure

The PoC was implemented as a self-contained multi-project solution under `samples/poc/risk-t01-offline-sync/`, following the SAD's Implementation View package structure:

| Project | Layer | Responsibility |
|---|---|---|
| `EmployeePortal.Poc.Domain` | Domain | Clocking, SyncRecord entities; ClockingType, SyncStatus enums; Result<T> type |
| `EmployeePortal.Poc.Application` | Application | INetworkHealth, ILocalStore, IRepository<T> interfaces; TimeTrackingService, SyncQueue |
| `EmployeePortal.Poc.Infrastructure` | Infrastructure | TcpHealthMonitor, SqliteLocalStore, InMemoryClockingRepository |
| `EmployeePortal.Poc.Tests` | Test | xUnit dual-coverage tests (black-box + white-box) |

### Compilation Dependency Hierarchy (Bottom-Up)

1. **Domain** (no dependencies) → 2. **Application** (depends on Domain) → 3. **Infrastructure** (depends on Application) → 4. **Tests** (depends on Application + Infrastructure)

### Component Diagram — PoC Subsystems and Interface Dependencies

```plantuml
@startuml
skinparam componentStyle rectangle
skinparam shadow false
skinparam defaultFontName "Segoe UI"
skinparam component {
  BackgroundColor<<domain>> #fff3e0
  BackgroundColor<<application>> #e3f2fd
  BackgroundColor<<infrastructure>> #fce4ec
  BackgroundColor<<test>> #f3e5f5
  BorderColor #37474f
}
skinparam interface {
  BackgroundColor #fffde7
  BorderColor #f57f17
}

title PoC-1: Offline Sync — Component Diagram (RISK-T01)

package "samples/poc/risk-t01-offline-sync" {

  package "Domain Layer" <<domain>> {
    component "Clocking\nEntity" as CLK
    component "SyncRecord\nEntity" as SYNC
    component "ClockingType\nEnum" as CT
    component "SyncStatus\nEnum" as SS
    component "Result<T>\nType" as RESULT
  }

  package "Application Layer" <<application>> {
    component "TimeTrackingService" as TS
    component "SyncQueue" as SQ
    interface "INetworkHealth" as INH
    interface "ILocalStore" as ILS
    interface "IRepository<Clocking>" as IREPO
  }

  package "Infrastructure Layer" <<infrastructure>> {
    component "TcpHealthMonitor" as THM
    component "SqliteLocalStore" as SLS
    component "InMemoryClockingRepository" as IMR
  }

  package "Test Layer" <<test>> {
    component "SyncQueueTests\n(Black-box + White-box)" as SQT
    component "TimeTrackingServiceTests\n(Black-box + White-box)" as TST
    component "TcpHealthMonitorTests\n(Black-box + White-box)" as THMT
    component "SqliteLocalStoreTests\n(Black-box + White-box)" as SLST
  }
}

THM ..|> INH
SLS ..|> ILS
IMR ..|> IREPO

TS --> INH : checks health
TS --> IREPO : writes online
TS --> SQ : delegates offline
TS ..> CLK : creates
SQ --> ILS : persists local
SQ --> IREPO : flushes to remote
SQ ..> SYNC : manages records
SQ ..> CLK : syncs

CLK --> CT
CLK --> SS
SYNC --> SS

SQT --> SQ
SQT --> ILS
SQT --> IREPO
TST --> TS
TST --> INH
TST --> SQ
THMT --> THM
SLST --> SLS

note right of SQ
  **Validates RISK-T01 (RPN 63)**
  - Offline enqueue with SemaphoreSlim(1,1)
  - Flush on network restore
  - Conflict detection by (employeeId, timestamp)
  - Zero data loss guarantee
end note

note right of THM
  **Validates RISK-T01 mechanism**
  TCP probe to pg:5432 every 5s
  HealthStatus: UP / DOWN
end note

@enduml
```

### Package Diagram — Compilation Dependencies

```plantuml
@startuml
skinparam shadow false
skinparam defaultFontName "Segoe UI"

title PoC-1: Offline Sync — Package Diagram (Compilation Dependencies)

package "EmployeePortal.Poc.Domain" as POC_DOMAIN {
  class Clocking
  class SyncRecord
  enum ClockingType
  enum SyncStatus
  class Result
}

package "EmployeePortal.Poc.Application" as POC_APP {
  interface INetworkHealth
  interface ILocalStore
  interface IRepository
  class TimeTrackingService
  class SyncQueue
  class HealthStatus
}

package "EmployeePortal.Poc.Infrastructure" as POC_INFRA {
  class TcpHealthMonitor
  class SqliteLocalStore
  class InMemoryClockingRepository
}

package "EmployeePortal.Poc.Tests" as POC_TESTS {
  class SyncQueueTests
  class TimeTrackingServiceTests
  class TcpHealthMonitorTests
  class SqliteLocalStoreTests
}

POC_INFRA --> POC_APP : implements interfaces
POC_APP --> POC_DOMAIN : uses entities
POC_TESTS --> POC_APP : tests services
POC_TESTS --> POC_DOMAIN : tests entities
POC_TESTS --> POC_INFRA : tests implementations

note bottom of POC_DOMAIN
  **Build order: 1st**
  No dependencies — pure domain model
end note

note bottom of POC_APP
  **Build order: 2nd**
  Depends on Domain only
  Defines interfaces for Infrastructure
end note

note bottom of POC_INFRA
  **Build order: 3rd**
  Implements Application interfaces
  EF Core SQLite + TCP probing
end note

note bottom of POC_TESTS
  **Build order: 4th**
  xUnit + FluentAssertions
  Dual coverage: black-box + white-box
end note

@enduml
```

### Experiment Design

| Test Suite | Test Count | Coverage Type | Scenarios |
|---|---|---|---|
| SyncQueueTests | 10 | Black-box (5) + White-box (5) | Enqueue, flush, conflict detection, concurrent access, zero data loss, null input, failure path, mixed records, double flush |
| TimeTrackingServiceTests | 9 | Black-box (4) + White-box (5) | Online clock-in/out, offline clock-in, sync after restore, empty employeeId, transient failure fallback, zero data loss with 5 employees |
| TcpHealthMonitorTests | 6 | Black-box (2) + White-box (4) | Active listener UP, no listener DOWN, constructor validation, unreachable host |
| SqliteLocalStoreTests | 8 | Black-box (4) + White-box (4) | Save with incremental ID, get pending, update status, count, empty state, skipped status, ordering, multiple records |
| **Total** | **33** | **17 black-box + 16 white-box** | |

## Results and Findings

### Build Evidence

| Push | SHA | CI Result | Layer |
|---|---|---|---|
| 1 | `0ee1cb5857ead145c443fe8eb7aab91b4484593c` | ✅ Green | Domain + Application |
| 2 | `21725b2eade8e181e68b3d228086af956cb1d010` | ✅ Green | Infrastructure |
| 3 | `b4b6d03a404ee8173e23b439701877d26eaf1de4` | ✅ Green | Tests (all 33 tests pass) |

CI runs:
- https://github.com/banense-test/demo-janke-lab/actions/runs/28860607749
- https://github.com/banense-test/demo-janke-lab/actions/runs/28860651520
- https://github.com/banense-test/demo-janke-lab/actions/runs/28860717112

### Outcome: VALIDATED ✅

The PoC empirically validates the offline sync mechanism for RISK-T01:

1. **Offline enqueue works:** When `INetworkHealth.CheckHealth()` returns `DOWN`, `TimeTrackingService` enqueues clockings to `SyncQueue` → `SqliteLocalStore`. User receives immediate confirmation. ✅

2. **Zero data loss:** 5 concurrent offline clockings were all persisted to SQLite and successfully flushed to the remote repository on network restore. All 5 records synced, 0 lost. ✅

3. **Conflict detection works:** When a clocking with the same `(employeeId, timestamp)` already exists remotely, the `SyncQueue.FlushAsync()` marks the record as `SKIPPED` instead of creating a duplicate. ✅

4. **Transient failure fallback works:** When `IRepository.SaveAsync()` returns `false` (simulating a transient PostgreSQL failure), `TimeTrackingService` falls back to the offline path. The clocking is enqueued locally with `source = "OFFLINE"`. ✅

5. **Thread safety works:** 10 concurrent `EnqueueAsync` calls all succeeded with correct pending count, validating the `SemaphoreSlim(1,1)` single-writer lock. ✅

6. **TCP health probe works:** `TcpHealthMonitor` correctly returns `UP` when a TCP listener is active and `DOWN` when no listener is available or the host is unreachable. ✅

7. **Flush is idempotent:** Calling `FlushAsync()` twice in succession results in the second call processing 0 records (all already synced/skipped). ✅

### Key Implementation Decisions

| Decision | Rationale | SAD Reference |
|---|---|---|
| `SemaphoreSlim(1,1)` for write lock | .NET async/await compatible; serializes SQLite writes | Process View |
| Separate flush lock from write lock | Allows new enqueues during flush — prevents blocking | Process View |
| Conflict detection by `(employeeId, timestamp)` | Matches PostgreSQL `UNIQUE(employee_id, timestamp)` constraint | Data View |
| `InMemoryClockingRepository` for PoC | Simulates PostgreSQL without requiring a live instance; `SetFailureMode` enables white-box transient failure testing | — |
| SQLite in-memory for PoC | No file system dependency; `EnsureCreated()` creates schema | Data View |

## Architectural Implications

### Validated Architecture Decisions

1. **ADR-002 (Offline Sync Strategy):** The layered architecture with interface isolation (`INetworkHealth`, `ILocalStore`, `IRepository<T>`) is validated. The offline mechanism can be tested in isolation by substituting test doubles for infrastructure components.

2. **COMP-D4 (Sync Queue) + COMP-I5 (Network Health Monitor):** Both components are validated as architecturally sound. The `SyncQueue` correctly manages the offline-to-online lifecycle, and `TcpHealthMonitor` provides reliable health detection.

3. **Process View concurrency model:** The `SemaphoreSlim(1,1)` single-writer lock and separate flush lock are validated. No deadlocks observed in concurrent enqueue tests.

4. **Data View conflict detection:** The `(employeeId, timestamp)` uniqueness key is validated at the application level by `SyncQueue.FlushAsync()`, matching the PostgreSQL `UNIQUE` constraint.

### Recommendations for Construction

1. **EF Core migrations:** The PoC uses `EnsureCreated()` for SQLite. Production should use EF Core migrations for PostgreSQL schema management.
2. **Health probe cadence:** The PoC validates the TCP probe mechanism. Production should implement a background timer (5s interval) per the SAD Process View.
3. **AD Integration (RISK-T02):** Deferred to Construction per key decisions. The `IAuthProvider` interface isolation is validated by the same pattern used for `INetworkHealth` and `ILocalStore`.
4. **PostgreSQL-specific testing:** The PoC uses `InMemoryClockingRepository`. Construction should add integration tests against a real PostgreSQL instance.

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| PoC Clocking.cs | ACL-014 (Clocking) | Implements | — |
| PoC SyncRecord.cs | ACL-018 (SyncRecord) | Implements | — |
| PoC SyncQueue.cs | ACL-013 (SyncQueue), COMP-D4 | Implements | RISK-T01 |
| PoC TimeTrackingService.cs | ACL-009 (TimeTrackingService), COMP-A1, SEQ-001 | Implements | RISK-T01, RISK-T03 |
| PoC TcpHealthMonitor.cs | COMP-I5, INT-005 (INetworkHealth) | Implements | RISK-T01 |
| PoC SqliteLocalStore.cs | COMP-I3, INT-003 (ILocalStore) | Implements | RISK-T01 |
| PoC InMemoryClockingRepository.cs | COMP-I2, INT-002 (IRepository) | Implements | — |
| SyncQueueTests.cs | SyncQueue.cs | Tests | — |
| TimeTrackingServiceTests.cs | TimeTrackingService.cs | Tests | — |
| TcpHealthMonitorTests.cs | TcpHealthMonitor.cs | Tests | — |
| SqliteLocalStoreTests.cs | SqliteLocalStore.cs | Tests | — |
| PoC-1 (Offline Sync) | RISK-T01 (RPN 63), REQ-014 | Derives | ADR-002, COMP-D4, COMP-I3, COMP-I5 |