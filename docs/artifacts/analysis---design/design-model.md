## Document Control

| Field | Value |
|---|---|
| Phase | Elaboration |
| Status | Draft |
| Iteration | 1 (Cycle 1) |
| Milestone Target | End of Elaboration |
| Author | User-Interface Designer |

### Elaboration Iteration 1 — UI Designer Contribution

- Design Model created with UI view/controller classes, UI Patterns, and Use-Case Realizations (interaction flows) for all 7 UCs of UI significance.
- UI class diagram defines 9 view classes (stereotyped `<<view>>`) and 3 controller classes (stereotyped `<<controller>>`) aligned with SAD component decomposition (COMP-P1 through COMP-P4).
- UI Patterns section published as coordination artifact for Designer, Implementer, and Technical Writer.
- Salt wireframes produced for 3 primary screens: Home/Clock, Directory Search, Admin News Publishing.
- Interaction flow activity diagrams for all 7 UCs trace to use-case flow steps and apply measurable usability requirements (REQ-008 through REQ-045).

## Design Overview
This Design Model captures the complete design of the Employee Portal, combining UI design (view/controller classes, UI patterns, interaction flows) and component design (domain classes, service classes, use-case realizations, state machines, subsystem definitions).

**Contributors:**
- **UI Designer:** View/controller class structure, UI interaction patterns, use-case realizations (interaction flow activity diagrams)
- **Designer (Analysis & Design):** Analysis classes, design class diagrams per package, sequence diagrams, state machines, design subsystems, interface contracts

**Architecture alignment:** UI classes align with the SAD's component decomposition:
- COMP-P1 (Home/Clock) → HomePage, HistoryPage + ClockingController
- COMP-P2 (News) → NewsListPage, NewsDetailPage, AdminNewsPage + NewsController
- COMP-P3 (Directory) → DirectoryPage, AdminDirectoryPage + DirectoryController
- COMP-P4 (HR Admin) → AdminClockingsPage (shares ClockingController)

**Technology constraint:** Razor Pages (CON-001) — no SPA. View classes extend a BasePage abstraction; controllers handle business logic delegation.

### Design Subsystems

The design model is organized into four subsystems corresponding to the SAD's layered architecture. Each subsystem is a package that offers interfaces — no concrete class is referenced across a subsystem boundary.

| Subsystem | ID | Provided Interfaces | Required Interfaces | Contains |
|---|---|---|---|---|
| Presentation | SUB-PRES | (none — consumed by ASP.NET pipeline) | ClockingController, NewsController, DirectoryController (concrete) | HomePage, HistoryPage, AdminClockingsPage, AdminNewsPage, NewsListPage, NewsDetailPage, DirectoryPage, AdminDirectoryPage, BasePage |
| Application | SUB-APP | TimeTrackingService, NewsService, DirectoryService, AuditInterceptor, SyncQueue (concrete services) | INT-001 (IAuthProvider), INT-002 (IRepository<T>), INT-003 (ILocalStore), INT-004 (IExportService), INT-005 (INetworkHealth), INT-006 (IAuditLogger) | Service classes orchestrating business logic |
| Domain | SUB-DOM | Clocking, NewsItem, Employee, AuditEntry, SyncRecord, value objects | (none — pure domain, no dependencies) | Domain entities, enumerations, value objects |
| Infrastructure | SUB-INFRA | INT-001 through INT-006 (all interfaces implemented here) | (none — depends on external: PostgreSQL, SQLite, AD/LDAP) | PostgresRepository<T>, SqliteLocalStore, LdapAuthProvider, CsvExporter, TcpHealthMonitor, EfAuditLogger, PortalDbContext, LocalDbContext |

**Dependency direction (top-down, strictly enforced):**

```plantuml
@startuml
skinparam componentStyle rectangle
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam package {
  BackgroundColor<<presentation>> #e8f5e9
  BackgroundColor<<application>> #e3f2fd
  BackgroundColor<<domain>> #fff3e0
  BackgroundColor<<infrastructure>> #fce4ec
  BorderColor #37474f
}
skinparam interface {
  BackgroundColor #fffde7
  BorderColor #f57f17
}

title Design Model — Package Organization & Subsystem Dependencies

package "Presentation Layer" <<presentation>> {
  [HomePage\n(CLS-001)] as CLS_001
  [HistoryPage\n(CLS-002)] as CLS_002
  [AdminClockingsPage\n(CLS-003)] as CLS_003
  [AdminNewsPage\n(CLS-004)] as CLS_004
  [NewsListPage\n(CLS-005)] as CLS_005
  [NewsDetailPage\n(CLS-006)] as CLS_006
  [DirectoryPage\n(CLS-007)] as CLS_007
  [AdminDirectoryPage\n(CLS-008)] as CLS_008
  [ClockingController\n(CLS-009)] as CLS_009
  [NewsController\n(CLS-010)] as CLS_010
  [DirectoryController\n(CLS-011)] as CLS_011
}

package "Application Layer" <<application>> {
  [TimeTrackingService\n(CLS-012)] as CLS_012
  [NewsService\n(CLS-013)] as CLS_013
  [DirectoryService\n(CLS-014)] as CLS_014
  [AuditInterceptor\n(CLS-015)] as CLS_015
  [SyncQueue\n(CLS-016)] as CLS_016
}

package "Domain Layer" <<domain>> {
  [Clocking\n(CLS-017)] as CLS_017
  [NewsItem\n(CLS-018)] as CLS_018
  [Employee\n(CLS-019)] as CLS_019
  [AuditEntry\n(CLS-020)] as CLS_020
  [SyncRecord\n(CLS-021)] as CLS_021
  [ADEmployeeRecord\n(CLS-022)] as CLS_022
  [SyncResult\n(CLS-023)] as CLS_023
  [Result<T>\n(CLS-024)] as CLS_024
  [ValidationResult\n(CLS-025)] as CLS_025
}

package "Infrastructure Layer" <<infrastructure>> {
  [PostgresRepository<T>\n(CLS-026)] as CLS_026
  [SqliteLocalStore\n(CLS-027)] as CLS_027
  [LdapAuthProvider\n(CLS-028)] as CLS_028
  [CsvExporter\n(CLS-029)] as CLS_029
  [TcpHealthMonitor\n(CLS-030)] as CLS_030
  [EfAuditLogger\n(CLS-031)] as CLS_031
  [PortalDbContext\n(CLS-032)] as CLS_032
  [LocalDbContext\n(CLS-033)] as CLS_033
}

interface "IAuthProvider\n(INT-001)" as INT_001
interface "IRepository<T>\n(INT-002)" as INT_002
interface "ILocalStore\n(INT-003)" as INT_003
interface "IExportService\n(INT-004)" as INT_004
interface "INetworkHealth\n(INT-005)" as INT_005
interface "IAuditLogger\n(INT-006)" as INT_006

CLS_009 --> CLS_012
CLS_010 --> CLS_013
CLS_011 --> CLS_014

CLS_012 --> INT_002
CLS_012 --> INT_003
CLS_012 --> INT_004
CLS_012 --> INT_005
CLS_013 --> INT_002
CLS_013 --> INT_006
CLS_014 --> INT_002
CLS_014 --> INT_001
CLS_014 --> INT_006
CLS_015 --> INT_006
CLS_016 --> INT_003
CLS_016 --> INT_002

CLS_026 ..|> INT_002
CLS_027 ..|> INT_003
CLS_028 ..|> INT_001
CLS_029 ..|> INT_004
CLS_030 ..|> INT_005
CLS_031 ..|> INT_006

CLS_026 ..> CLS_017
CLS_026 ..> CLS_018
CLS_026 ..> CLS_019
CLS_026 ..> CLS_020
CLS_027 ..> CLS_017
CLS_027 ..> CLS_021

note bottom of INT_002
  All cross-layer communication
  is via interfaces.
  No concrete class is referenced
  across a layer boundary.
end note

@enduml
```

**Integration order (bottom-up per SAD):** Infrastructure → Application → Presentation. Domain has no dependencies and is integrated alongside Infrastructure.

### State Machines

Three design classes have complex lifecycle behavior (3+ distinct states) requiring state machine diagrams.

#### Clocking (CLS-017) — Sync Lifecycle

The Clocking entity transitions through sync states as it moves from local offline storage to PostgreSQL. This state machine is the design realization of the offline fault tolerance requirement (REQ-014).

```plantuml
@startuml
title State Machine: Clocking (CLS-017) Sync Lifecycle

[*] --> PENDING : new Clocking()
PENDING --> SYNCED : Flush succeeds
PENDING --> SKIPPED : Conflict detected
PENDING --> PENDING : Network still DOWN
SYNCED --> [*]
SKIPPED --> [*]

PENDING : syncStatus = PENDING
PENDING : Stored in ILocalStore
SYNCED : syncStatus = SYNCED
SYNCED : Persisted in PostgreSQL
SKIPPED : syncStatus = SKIPPED
SKIPPED : Duplicate retained

note right of PENDING
  Trigger to SYNCED:
  IRepository.SaveAsync() returns Ok
  Guard: (employeeId, timestamp) unique
  Action: UpdateSyncStatus(SYNCED)
end note

note right of SKIPPED
  Trigger: SaveAsync returns
  duplicate key error
  Action: UpdateSyncStatus(SKIPPED)
  Rationale: No data loss -
  original clocking retained
end note

@enduml
```

#### SyncRecord (CLS-021) — Queue Entry Lifecycle

SyncRecord tracks each queued clocking through the sync process. The single-writer lock (SemaphoreSlim(1,1)) ensures no concurrent flush operations.

```plantuml
@startuml
title State Machine: SyncRecord (CLS-021) Queue Lifecycle

[*] --> QUEUED : Enqueue(clocking)
QUEUED --> SYNCING : NetworkHealth UP
SYNCING --> COMPLETED : SaveAsync succeeds
SYNCING --> CONFLICT : Duplicate key
SYNCING --> QUEUED : Transient failure
COMPLETED --> [*]
CONFLICT --> [*]

QUEUED : status = PENDING
QUEUED : queuedAt = UtcNow
SYNCING : SemaphoreSlim(1,1) held
COMPLETED : status = SYNCED
COMPLETED : syncedAt = UtcNow
CONFLICT : status = SKIPPED

note right of SYNCING
  Single-writer lock ensures
  no concurrent flush operations.
  Transient failures retry on
  next flush cycle.
end note

@enduml
```

#### Employee (CLS-019) — Directory Lifecycle

The Employee entity tracks active/inactive status and AD sync override state. The overrideFlag mechanism resolves the AD sync conflict risk (RISK-R01, RPN 30).

```plantuml
@startuml
title State Machine: Employee (CLS-019) Directory Lifecycle

[*] --> ACTIVE : Created from AD sync
ACTIVE --> OVERRIDDEN : HR edits local field
OVERRIDDEN --> ACTIVE : HR clears override
ACTIVE --> INACTIVE : HR deactivates
INACTIVE --> ACTIVE : HR reactivates
INACTIVE --> [*]

ACTIVE : isActive=true, overrideFlag=false
OVERRIDDEN : isActive=true, overrideFlag=true
INACTIVE : isActive=false

note right of OVERRIDDEN
  overrideFlag=true means
  AD sync will not overwrite
  local HR changes
end note

note right of INACTIVE
  Not visible in directory search
  AuditEntry created on transition
end note

@enduml
```
## Domain Model
### Analysis Classes

The analysis class model identifies boundary, control, and entity classes for all 7 use cases. Boundary classes are referenced from the UI Designer's contribution (view/controller classes in the Design Overview section); control and entity classes are the Designer's contribution.

**Three-Level Mechanism Resolution:**

| Analysis Mechanism | Design Mechanism | Implementation Mechanism | Risk |
|---|---|---|---|
| Persistence | Repository pattern with `IRepository<T>` | EF Core 10.0 + Npgsql (PostgreSQL) | — |
| Offline persistence | Local store with `ILocalStore` | EF Core 10.0 + SQLite | RISK-T01 |
| Authentication | `IAuthProvider` interface | LDAP/OAuth2 adapter (spike deferred to Construction) | RISK-T02 |
| Audit trail | `IAuditLogger` interface | Append-only AuditEntry table via EF Core | — |
| Network detection | `INetworkHealth` interface | TcpHealthMonitor (TCP probe to pg:5432 every 5s) | RISK-T01 |
| Export | `IExportService` interface | CsvExporter (RFC 4180 compliant) | — |

```plantuml
@startuml
skinparam classAttributeIconSize 0
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam class {
  BackgroundColor<<boundary>> #e8f5e9
  BackgroundColor<<control>> #e3f2fd
  BackgroundColor<<entity>> #fff3e0
  BackgroundColor<<infrastructure>> #fce4ec
  BorderColor #37474f
}

title Analysis Classes — Employee Portal (All UCs)

' === Boundary Classes (from UI Designer, referenced not redefined) ===
package "Boundary (Presentation)" {
  class "HomePage\n(ACL-001)" as ACL_001 <<boundary>> {
    + OnGet() : Task<PageResult>
    + OnPostClockIn() : Task<PageResult>
    + OnPostClockOut() : Task<PageResult>
  }
  class "HistoryPage\n(ACL-002)" as ACL_002 <<boundary>> {
    + OnGet(month: int) : Task<PageResult>
  }
  class "AdminClockingsPage\n(ACL-003)" as ACL_003 <<boundary>> {
    + OnGet(month: int) : Task<PageResult>
    + OnPostExport(month: int) : Task<FileResult>
  }
  class "AdminNewsPage\n(ACL-004)" as ACL_004 <<boundary>> {
    + OnGet() : Task<PageResult>
    + OnPostPublish(newsItem: NewsItem) : Task<PageResult>
  }
  class "NewsListPage\n(ACL-005)" as ACL_005 <<boundary>> {
    + OnGet(category: string?) : Task<PageResult>
  }
  class "NewsDetailPage\n(ACL-006)" as ACL_006 <<boundary>> {
    + OnGet(id: Guid) : Task<PageResult>
  }
  class "DirectoryPage\n(ACL-007)" as ACL_007 <<boundary>> {
    + OnGet(query: string, dept: string?, office: string?) : Task<PageResult>
  }
  class "AdminDirectoryPage\n(ACL-008)" as ACL_008 <<boundary>> {
    + OnGet() : Task<PageResult>
    + OnPostUpdate(emp: Employee) : Task<PageResult>
    + OnPostSyncAD() : Task<PageResult>
  }
}

' === Control Classes (Application Layer) ===
package "Control (Application)" {
  class "TimeTrackingService\n(ACL-009)" as ACL_009 <<control>> {
    + ClockIn(employeeId: Guid) : Result<Clocking>
    + ClockOut(employeeId: Guid) : Result<Clocking>
    + GetClockings(employeeId: Guid, month: DateTime) : List<Clocking>
    + GetAllClockings(month: DateTime) : List<Clocking>
    + ExportClockings(month: DateTime) : byte[]
  }
  class "NewsService\n(ACL-010)" as ACL_010 <<control>> {
    + PublishNews(item: NewsItem) : Result<NewsItem>
    + GetNewsList(category: string?) : List<NewsItem>
    + GetNewsDetail(id: Guid) : NewsItem
  }
  class "DirectoryService\n(ACL-011)" as ACL_011 <<control>> {
    + Search(query: string, dept: string?, office: string?) : List<Employee>
    + UpdateEmployee(emp: Employee) : Result<Employee>
    + SyncFromAD() : SyncResult
  }
  class "AuditInterceptor\n(ACL-012)" as ACL_012 <<control>> {
    + Log(entityType: string, entityId: string, action: string, user: string) : void
  }
  class "SyncQueue\n(ACL-013)" as ACL_013 <<control>> {
    + Enqueue(clocking: Clocking) : Result<int>
    + Flush() : SyncResult
    + GetPendingCount() : int
  }
}

' === Entity Classes (Domain Layer) ===
package "Entity (Domain)" {
  class "Clocking\n(ACL-014)" as ACL_014 <<entity>> {
    + id: Guid
    + employeeId: Guid
    + type: ClockingType
    + timestamp: DateTime
    + syncStatus: SyncStatus
  }
  class "NewsItem\n(ACL-015)" as ACL_015 <<entity>> {
    + id: Guid
    + title: string
    + body: string
    + publishedDate: DateTime
    + category: Category
    + isFeatured: bool
  }
  class "Employee\n(ACL-016)" as ACL_016 <<entity>> {
    + id: Guid
    + adId: string
    + fullName: string
    + jobTitle: string
    + department: string
    + office: string
    + email: string
    + extension: string
    + isActive: bool
    + overrideFlag: bool
  }
  class "AuditEntry\n(ACL-017)" as ACL_017 <<entity>> {
    + id: Guid
    + entityType: string
    + entityId: string
    + action: string
    + user: string
    + timestamp: DateTime
  }
  class "SyncRecord\n(ACL-018)" as ACL_018 <<entity>> {
    + localId: int
    + clockingId: Guid
    + status: SyncStatus
    + queuedAt: DateTime
    + syncedAt: DateTime?
  }
}

' === Enumerations ===
enum "ClockingType" as CT {
  IN
  OUT
}
enum "SyncStatus" as SS {
  PENDING
  SYNCED
  SKIPPED
}
enum "Category" as CAT {
  General
  HR
  IT
  Events
}

' === Relationships ===
' Boundary -> Control
ACL_001 --> ACL_009 : delegates
ACL_002 --> ACL_009 : delegates
ACL_003 --> ACL_009 : delegates
ACL_004 --> ACL_010 : delegates
ACL_005 --> ACL_010 : delegates
ACL_006 --> ACL_010 : delegates
ACL_007 --> ACL_011 : delegates
ACL_008 --> ACL_011 : delegates

' Control -> Entity
ACL_009 ..> ACL_014 : creates/reads
ACL_009 --> ACL_013 : delegates offline
ACL_013 ..> ACL_014 : syncs
ACL_013 ..> ACL_018 : manages
ACL_010 ..> ACL_015 : creates/reads
ACL_010 --> ACL_012 : logs
ACL_011 ..> ACL_016 : reads/updates
ACL_011 --> ACL_012 : logs
ACL_012 ..> ACL_017 : creates

' Entity -> Enum
ACL_014 --> CT
ACL_014 --> SS
ACL_018 --> SS
ACL_015 --> CAT

note bottom of ACL_013
  **Offline Sync Mechanism (Design)**
  Analysis: "persist clockings offline"
  Design: SyncQueue + ILocalStore (SQLite)
  Implementation: SemaphoreSlim(1,1) single-writer lock
  Resolves RISK-T01 (RPN 63)
end note

note bottom of ACL_012
  **Audit Mechanism (Design)**
  Analysis: "log directory/news changes"
  Design: IAuditLogger interface + AuditInterceptor
  Implementation: Append-only AuditEntry table via EF Core
  Resolves REQ-004, REQ-005, REQ-006
end note

@enduml
```

### Analysis Class to Use-Case Traceability

| Analysis Class | ID | Participates In UCs | Stereotype |
|---|---|---|---|
| HomePage | ACL-001 | UC-001 | <<boundary>> |
| HistoryPage | ACL-002 | UC-002 | <<boundary>> |
| AdminClockingsPage | ACL-003 | UC-003 | <<boundary>> |
| AdminNewsPage | ACL-004 | UC-004 | <<boundary>> |
| NewsListPage | ACL-005 | UC-005 | <<boundary>> |
| NewsDetailPage | ACL-006 | UC-005 | <<boundary>> |
| DirectoryPage | ACL-007 | UC-006 | <<boundary>> |
| AdminDirectoryPage | ACL-008 | UC-007 | <<boundary>> |
| TimeTrackingService | ACL-009 | UC-001, UC-002, UC-003 | <<control>> |
| NewsService | ACL-010 | UC-004, UC-005 | <<control>> |
| DirectoryService | ACL-011 | UC-006, UC-007 | <<control>> |
| AuditInterceptor | ACL-012 | UC-004, UC-007 | <<control>> |
| SyncQueue | ACL-013 | UC-001 | <<control>> |
| Clocking | ACL-014 | UC-001, UC-002, UC-003 | <<entity>> |
| NewsItem | ACL-015 | UC-004, UC-005 | <<entity>> |
| Employee | ACL-016 | UC-006, UC-007 | <<entity>> |
| AuditEntry | ACL-017 | UC-004, UC-007 | <<entity>> |
| SyncRecord | ACL-018 | UC-001 | <<entity>> |
## Use-Case Realizations
### Use-Case Realizations — Sequence Diagrams

The following sequence diagrams realize each use case as a collaboration of design objects. Each realization shows the main flow and key alternative/exception flows. The SAD's Use-Case View contains architecturally-focused sequences for UC-001, UC-003, and UC-007; the realizations below provide full design-level detail for all 7 UCs with explicit object responsibilities, interface calls, and error handling.

#### SEQ-001: UC-001 Clock In/Out — Offline Fault Tolerance

**Participating objects:** HomePage (CLS-001), ClockingController (CLS-002), TimeTrackingService (CLS-009), INetworkHealth (INT-005), SyncQueue (CLS-013), Clocking (CLS-014), IRepository<Clocking> (INT-002), ILocalStore (INT-003)

**Design decisions validated:**
- INetworkHealth decouples health detection — TcpHealthMonitor probes pg:5432 every 5s
- SyncQueue manages offline-to-online transition with conflict detection by (employeeId, timestamp) uniqueness
- Transient PostgreSQL failure falls back to offline path — zero data loss (REQ-014)
- User receives immediate confirmation in both modes (<1s, REQ-017)

```plantuml
@startuml
title SEQ-001: UC-001 Clock In/Out — Design Realization (Main + Offline + Sync)

actor "Employee" as EMP
participant "HomePage\n(CLS-001)" as UI
participant "ClockingController\n(CLS-002)" as CTRL
participant "TimeTrackingService\n(CLS-009)" as TS
participant "INetworkHealth\n(INT-005)" as NHM
participant "SyncQueue\n(CLS-013)" as SQ
participant "Clocking\n(CLS-014)" as CLK
participant "IRepository<Clocking>\n(INT-002)" as REPO
participant "ILocalStore\n(INT-003)" as LOCAL

== Main Flow: Clock In (Network UP) ==

EMP -> UI : Click "Clock In"
UI -> CTRL : OnPostClockIn()
CTRL -> TS : ClockIn(employeeId)
TS -> NHM : CheckHealth()
NHM --> TS : HealthStatus.UP
TS -> CLK : new Clocking(employeeId, ClockingType.IN, DateTime.Now)
CLK --> TS : clocking entity
TS -> REPO : SaveAsync(clocking)
REPO --> TS : success
TS --> CTRL : Result<Clocking>.Ok(clocking)
CTRL --> UI : ClockInConfirmation(timestamp)
UI --> EMP : "Clocked In at HH:MM:SS"

== Alternative Flow 1: Clock Out (Network DOWN) ==

EMP -> UI : Click "Clock Out"
UI -> CTRL : OnPostClockOut()
CTRL -> TS : ClockOut(employeeId)
TS -> NHM : CheckHealth()
NHM --> TS : HealthStatus.DOWN
TS -> SQ : Enqueue(clocking)
SQ -> LOCAL : SaveClockingAsync(clocking)
LOCAL --> SQ : localId = 42
SQ -> LOCAL : SaveSyncRecordAsync(localId, SyncStatus.PENDING)
LOCAL --> SQ : saved
SQ --> TS : Result<int>.Ok(localId)
TS --> CTRL : Result<Clocking>.Ok(clocking, syncPending: true)
CTRL --> UI : ClockOutConfirmation(timestamp, "sync pending")
UI --> EMP : "Clocked Out at HH:MM:SS (sync pending)"

== Alternative Flow 2: Auto-Sync on Network Restore ==

NHM -> TS : HealthChanged(HealthStatus.UP)
TS -> SQ : Flush()
SQ -> LOCAL : GetPendingSyncRecords()
LOCAL --> SQ : List<SyncRecord> (N records)
loop for each pending SyncRecord
  SQ -> LOCAL : GetClockingByLocalId(localId)
  LOCAL --> SQ : Clocking entity
  SQ -> REPO : SaveAsync(clocking)
  alt Duplicate (employeeId, timestamp) exists
    REPO --> SQ : DuplicateDetected
    SQ -> LOCAL : UpdateSyncStatus(localId, SyncStatus.SKIPPED)
  else No conflict
    REPO --> SQ : success
    SQ -> LOCAL : UpdateSyncStatus(localId, SyncStatus.SYNCED)
  end
end
SQ --> TS : SyncResult(synced: N-M, skipped: M)
TS -> TS : LogInformation("Sync complete: {N-M} synced, {M} skipped")

== Exception Flow: Transient PostgreSQL Failure ==

EMP -> UI : Click "Clock In"
UI -> CTRL : OnPostClockIn()
CTRL -> TS : ClockIn(employeeId)
TS -> NHM : CheckHealth()
NHM --> TS : HealthStatus.UP
TS -> REPO : SaveAsync(clocking)
REPO --> TS : Exception (timeout/deadlock)
TS -> SQ : Enqueue(clocking)
note right: Fallback to offline path\nensures zero data loss\n(REQ-014)
SQ --> TS : Result<int>.Ok(localId)
TS --> CTRL : Result<Clocking>.Ok(clocking, syncPending: true)
CTRL --> UI : ClockInConfirmation(timestamp, "sync pending")

@enduml
```

#### SEQ-002: UC-004 Publish News — Audit Trail

**Participating objects:** AdminNewsPage (CLS-004), NewsController (CLS-006), NewsService (CLS-010), NewsItem (CLS-015), IAuditLogger (INT-006), IRepository<NewsItem> (INT-002)

**Design decisions validated:**
- IAuditLogger formalizes audit as cross-cutting concern — every news publish produces an immutable AuditEntry (REQ-004, REQ-006)
- Validation occurs in domain entity before persistence
- Error handling distinguishes validation errors (user retry) from infrastructure failures (generic error + retry)

```plantuml
@startuml
title SEQ-002: UC-004 Publish News — Design Realization

actor "HR Admin" as HR
participant "AdminNewsPage\n(CLS-004)" as UI
participant "NewsController\n(CLS-006)" as CTRL
participant "NewsService\n(CLS-010)" as NS
participant "NewsItem\n(CLS-015)" as NEWS
participant "IAuditLogger\n(INT-006)" as AUD
participant "IRepository<NewsItem>\n(INT-002)" as REPO

== Main Flow: Publish News Item ==

HR -> UI : Fill form (title, body, category, isFeatured)
UI -> CTRL : OnPostPublish(newsItem)
CTRL -> NS : PublishNews(newsItem)
NS -> NEWS : new NewsItem(title, body, category, isFeatured)
NEWS --> NS : newsItem entity
NS -> NEWS : Validate()
NEWS --> NS : ValidationResult.Ok
NS -> REPO : SaveAsync(newsItem)
REPO --> NS : saved (id = Guid)
NS -> AUD : Log("NewsItem", id, "PUBLISH", currentUser)
AUD --> NS : logged
NS --> CTRL : Result<NewsItem>.Ok(newsItem)
CTRL --> UI : PublishConfirmation(title)
UI --> HR : "News published successfully"

== Alternative Flow: Validation Error ==

HR -> UI : Submit with empty title
UI -> CTRL : OnPostPublish(newsItem)
CTRL -> NS : PublishNews(newsItem)
NS -> NEWS : Validate()
NEWS --> NS : ValidationResult.Fail("Title required")
NS --> CTRL : Result<NewsItem>.Fail(errors)
CTRL --> UI : ValidationErrors(errors)
UI --> HR : "Title is required" (inline error)

== Exception Flow: Repository Failure ==

HR -> UI : Submit valid news item
UI -> CTRL : OnPostPublish(newsItem)
CTRL -> NS : PublishNews(newsItem)
NS -> NEWS : Validate()
NEWS --> NS : ValidationResult.Ok
NS -> REPO : SaveAsync(newsItem)
REPO --> NS : Exception (DB connection lost)
NS --> CTRL : Result<NewsItem>.Fail("Unable to save. Please try again.")
CTRL --> UI : Error("Unable to save. Please try again.")
UI --> HR : Error message with retry button

@enduml
```

#### SEQ-003: UC-006 Search Directory — Performance-Critical Read

**Participating objects:** DirectoryPage (CLS-007), DirectoryController (CLS-008), DirectoryService (CLS-011), IRepository<Employee> (INT-002)

**Design decisions validated:**
- Search delegates through DirectoryService to IRepository — no direct DB access from presentation layer
- Performance constraint ≤2s (REQ-018) ensured by PostgreSQL indexes on fullName, department, office
- isActive filter excludes departed employees from directory (UC-007 deactivation scenario)

```plantuml
@startuml
title SEQ-003: UC-006 Search Directory — Design Realization

actor "Employee" as EMP
participant "DirectoryPage\n(CLS-007)" as UI
participant "DirectoryController\n(CLS-008)" as CTRL
participant "DirectoryService\n(CLS-011)" as DS
participant "IRepository<Employee>\n(INT-002)" as REPO

== Main Flow: Search by Name ==

EMP -> UI : Enter "Juan" in search box
UI -> CTRL : OnGet(query: "Juan", dept: null, office: null)
CTRL -> DS : Search("Juan", null, null)
DS -> REPO : QueryAsync(e => e.fullName.Contains("Juan") && e.isActive)
REPO --> DS : List<Employee> (3 results)
DS --> CTRL : List<Employee>
CTRL --> UI : SearchResult(employees)
UI --> EMP : List of 3 matching colleagues

== Alternative Flow: Filter by Department ==

EMP -> UI : Select dept = "IT", click Search
UI -> CTRL : OnGet(query: "", dept: "IT", office: null)
CTRL -> DS : Search("", "IT", null)
DS -> REPO : QueryAsync(e => e.department == "IT" && e.isActive)
REPO --> DS : List<Employee> (15 results)
DS --> CTRL : List<Employee>
CTRL --> UI : SearchResult(employees)
UI --> EMP : List of 15 IT colleagues

== Alternative Flow: No Results ==

EMP -> UI : Enter "xyz" in search box
UI -> CTRL : OnGet(query: "xyz", dept: null, office: null)
CTRL -> DS : Search("xyz", null, null)
DS -> REPO : QueryAsync(e => e.fullName.Contains("xyz") && e.isActive)
REPO --> DS : List<Employee> (empty)
DS --> CTRL : List<Employee> (empty)
CTRL --> UI : SearchResult(empty)
UI --> EMP : "No colleagues found matching 'xyz'"

note right of DS
  **Performance constraint**
  Search must complete in ≤2s
  (REQ-018). PostgreSQL index
  on fullName, department, office
  ensures sub-second query.
end note

@enduml
```

#### SEQ-004: UC-002 View Clocking History

**Participating objects:** HistoryPage (CLS-003), ClockingController (CLS-002), TimeTrackingService (CLS-009), IRepository<Clocking> (INT-002)

```plantuml
@startuml
title SEQ-004: UC-002 View Clocking History — Design Realization

actor "Employee" as EMP
participant "HistoryPage\n(CLS-003)" as UI
participant "ClockingController\n(CLS-002)" as CTRL
participant "TimeTrackingService\n(CLS-009)" as TS
participant "IRepository<Clocking>\n(INT-002)" as REPO

== Main Flow: View Current Month Clockings ==

EMP -> UI : Navigate to History page
UI -> CTRL : OnGet(month: currentMonth)
CTRL -> TS : GetClockings(employeeId, currentMonth)
TS -> REPO : QueryAsync(c => c.employeeId == empId && c.timestamp.Month == month)
REPO --> TS : List<Clocking> (22 entries)
TS --> CTRL : List<Clocking>
CTRL --> UI : ClockingHistory(clockings)
UI --> EMP : Table of clockings for current month

== Alternative Flow: No Clockings ==

EMP -> UI : Navigate to History (new employee, no clockings)
UI -> CTRL : OnGet(month: currentMonth)
CTRL -> TS : GetClockings(employeeId, currentMonth)
TS -> REPO : QueryAsync(c => c.employeeId == empId && c.timestamp.Month == month)
REPO --> TS : List<Clocking> (empty)
TS --> CTRL : List<Clocking> (empty)
CTRL --> UI : ClockingHistory(empty)
UI --> EMP : "No clockings recorded for this month"

@enduml
```

#### SEQ-005: UC-003 Review and Export Clockings

**Participating objects:** AdminClockingsPage (CLS-005), ClockingController (CLS-002), TimeTrackingService (CLS-009), IRepository<Clocking> (INT-002), IExportService (INT-004)

```plantuml
@startuml
title SEQ-005: UC-003 Review and Export Clockings — Design Realization

actor "HR Admin" as HR
participant "AdminClockingsPage\n(CLS-005)" as UI
participant "ClockingController\n(CLS-002)" as CTRL
participant "TimeTrackingService\n(CLS-009)" as TS
participant "IRepository<Clocking>\n(INT-002)" as REPO
participant "IExportService\n(INT-004)" as EXP

== Main Flow: View All Employees' Clockings ==

HR -> UI : Select month = "July 2026", click View
UI -> CTRL : OnGet(month: 7)
CTRL -> TS : GetAllClockings(2026-07)
TS -> REPO : QueryAsync(c => c.timestamp.Year == 2026 && c.timestamp.Month == 7)
REPO --> TS : List<Clocking> (450 entries)
TS --> CTRL : List<Clocking> grouped by employee
CTRL --> UI : ClockingReport(clockings)
UI --> HR : Table of all clockings (per employee)

== Alternative Flow: Export CSV ==

HR -> UI : Click "Export CSV"
UI -> CTRL : OnPostExport(month: 7)
CTRL -> TS : ExportClockings(2026-07)
TS -> REPO : QueryAsync(c => c.timestamp.Year == 2026 && c.timestamp.Month == 7)
REPO --> TS : List<Clocking> (450 entries)
TS -> EXP : GenerateCSV(clockings)
EXP --> TS : byte[] (RFC 4180 CSV)
TS --> CTRL : byte[]
CTRL --> UI : FileResult("clockings-2026-07.csv", content)
UI --> HR : File download dialog

note right of EXP
  **IExportService interface**
  Columns: employee, date,
  time-in, time-out, duration.
  RFC 4180 compliant.
end note

@enduml
```

#### SEQ-006: UC-005 Read News

**Participating objects:** NewsListPage (CLS-016), NewsDetailPage (CLS-017), NewsController (CLS-006), NewsService (CLS-010), IRepository<NewsItem> (INT-002)

```plantuml
@startuml
title SEQ-006: UC-005 Read News — Design Realization

actor "Employee" as EMP
participant "NewsListPage\n(CLS-016)" as UI
participant "NewsController\n(CLS-006)" as CTRL
participant "NewsService\n(CLS-010)" as NS
participant "IRepository<NewsItem>\n(INT-002)" as REPO

== Main Flow: View News List (Default) ==

EMP -> UI : Navigate to News page
UI -> CTRL : OnGet(category: null)
CTRL -> NS : GetNewsList(null)
NS -> REPO : QueryAsync(n => n.publishedDate <= DateTime.Now, orderBy: publishedDate DESC)
REPO --> NS : List<NewsItem> (20 items)
NS --> CTRL : List<NewsItem> (featured first, then by date)
CTRL --> UI : NewsList(items)
UI --> EMP : Featured banner + news list sorted by date

== Alternative Flow: Filter by Category ==

EMP -> UI : Click category "HR"
UI -> CTRL : OnGet(category: "HR")
CTRL -> NS : GetNewsList("HR")
NS -> REPO : QueryAsync(n => n.category == Category.HR && n.publishedDate <= DateTime.Now)
REPO --> NS : List<NewsItem> (5 items)
NS --> CTRL : List<NewsItem>
CTRL --> UI : NewsList(items)
UI --> EMP : 5 HR news items

== Alternative Flow: Read News Detail ==

EMP -> UI : Click news title "New Policy"
UI -> CTRL : OnGet(id: newsId) [NewsDetailPage]
CTRL -> NS : GetNewsDetail(newsId)
NS -> REPO : FindAsync(newsId)
REPO --> NS : NewsItem
NS --> CTRL : NewsItem
CTRL --> UI : NewsDetail(item)
UI --> EMP : Full news article

@enduml
```

#### SEQ-007: UC-007 Manage Directory — AD Sync + Audit

**Participating objects:** AdminDirectoryPage (CLS-008), DirectoryController (CLS-008), DirectoryService (CLS-011), Employee (CLS-016), IAuditLogger (INT-006), IAuthProvider (INT-001), IRepository<Employee> (INT-002)

**Design decisions validated:**
- IAuthProvider isolates AD protocol — LDAP/OAuth2 swap is a DI registration change (RISK-T02)
- Override flag mechanism: HR local changes win when overrideFlag = true (RISK-R01)
- Three-way merge: skip (override), merge (no override), import (new entry)
- IAuditLogger logs every directory change with user, action, timestamp (REQ-005, REQ-006)

```plantuml
@startuml
title SEQ-007: UC-007 Manage Directory — Design Realization (Update + AD Sync + Audit)

actor "HR Admin" as HR
participant "AdminDirectoryPage\n(CLS-008)" as UI
participant "DirectoryController\n(CLS-008)" as CTRL
participant "DirectoryService\n(CLS-011)" as DS
participant "Employee\n(CLS-016)" as EMP_CLS
participant "IAuditLogger\n(INT-006)" as AUD
participant "IAuthProvider\n(INT-001)" as AD
participant "IRepository<Employee>\n(INT-002)" as REPO

== Main Flow: Update Employee Entry ==

HR -> UI : Edit "Juan Pérez" → change extension to "4567"
UI -> CTRL : OnPostUpdate(employee)
CTRL -> DS : UpdateEmployee(employee)
DS -> EMP_CLS : Validate(changes)
EMP_CLS --> DS : ValidationResult.Ok
DS -> REPO : SaveAsync(employee)
REPO --> DS : saved
DS -> AUD : Log("Employee", employee.id, "UPDATE", currentUser)
AUD --> DS : logged
DS --> CTRL : Result<Employee>.Ok(employee)
CTRL --> UI : UpdateConfirmation("Juan Pérez updated")
UI --> HR : "Employee updated successfully"

== Alternative Flow: AD Sync with Override Conflict ==

HR -> UI : Click "Sync from AD"
UI -> CTRL : OnPostSyncAD()
CTRL -> DS : SyncFromAD()
DS -> AD : FetchAllEmployeesAsync()
AD --> DS : List<ADEmployeeRecord> (200 records)
loop for each AD record
  DS -> REPO : FindByAdIdAsync(adId)
  alt Local entry exists, overrideFlag = true
    REPO --> DS : existing (override)
    DS -> DS : Skip — local override wins
  else Local entry exists, no override
    REPO --> DS : existing
    DS -> EMP_CLS : Merge(local, adRecord)
    EMP_CLS --> DS : merged
    DS -> REPO : SaveAsync(merged)
    DS -> AUD : Log("Employee", id, "AD_SYNC", "SYSTEM")
  else No local entry
    DS -> EMP_CLS : CreateFromAD(adRecord)
    EMP_CLS --> DS : new Employee
    DS -> REPO : SaveAsync(new)
    DS -> AUD : Log("Employee", id, "AD_IMPORT", "SYSTEM")
  end
end
DS --> CTRL : SyncResult(updated: 180, skipped: 15, imported: 5)
CTRL --> UI : SyncSummary(result)
UI --> HR : "Sync complete: 180 updated, 15 skipped (override), 5 imported"

== Exception Flow: AD Unavailable ==

HR -> UI : Click "Sync from AD"
UI -> CTRL : OnPostSyncAD()
CTRL -> DS : SyncFromAD()
DS -> AD : FetchAllEmployeesAsync()
AD --> DS : Exception (LDAP connection refused)
DS --> CTRL : Result<SyncResult>.Fail("AD server unavailable. Please try again later.")
CTRL --> UI : Error("AD server unavailable")
UI --> HR : Error message with retry button

note right of AD
  **IAuthProvider interface**
  AD protocol isolated behind
  interface. LDAP/OAuth2 swap
  is a DI registration change.
  RISK-T02 (RPN 35).
end note

@enduml
```

### Use-Case Realization Coverage

| UC ID | Use Case | Seq ID | Flows Covered | Architectural Significance |
|---|---|---|---|---|
| UC-001 | Clock In/Out | SEQ-001 | Main, Offline, Auto-Sync, Transient Failure | Critical — offline fault tolerance |
| UC-002 | View Clocking History | SEQ-004 | Main, No Clockings | Low — simple read |
| UC-003 | Review and Export Clockings | SEQ-005 | Main (View), Export CSV | High — CSV export mechanism |
| UC-004 | Publish News | SEQ-002 | Main, Validation Error, Repo Failure | Medium — audit trail |
| UC-005 | Read News | SEQ-006 | Main, Category Filter, Detail View | Low — read-only |
| UC-006 | Search Directory | SEQ-003 | Main, Dept Filter, No Results | Medium — performance constraint |
| UC-007 | Manage Directory | SEQ-007 | Update, AD Sync, AD Unavailable | High — AD sync + audit |
## Design Packages and Classes
### Design Packages and Classes

The design model is organized into four packages corresponding to the SAD's layered architecture. Each package contains design classes with full signatures — visibility, parameters, return types, and relationships. All cross-layer communication is via interfaces (INT-001 through INT-006).

#### Package: Application Layer

The Application package contains service classes that orchestrate business logic, delegate to repositories via interfaces, and coordinate cross-cutting concerns (audit, sync). All services depend on interfaces, never on concrete infrastructure classes.

```plantuml
@startuml
skinparam classAttributeIconSize 0
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam class {
  BackgroundColor #e3f2fd
  BorderColor #37474f
}
skinparam interface {
  BackgroundColor #fffde7
  BorderColor #f57f17
}

title Package: Application Layer — Design Classes (Full Signatures)

package "Application" {
  interface "IAuthProvider\n(INT-001)" as INT_001 {
    + Authenticate(username: string, password: string) : Task<AuthResult>
    + GetCurrentUser(ClaimsPrincipal) : PortalUser
    + FetchAllEmployeesAsync() : Task<List<ADEmployeeRecord>>
  }

  interface "IRepository<T>\n(INT-002)" as INT_002 {
    + GetByIdAsync(id: Guid) : Task<T?>
    + QueryAsync(predicate: Expression<Func<T, bool>>) : Task<List<T>>
    + SaveAsync(entity: T) : Task<Result<T>>
    + DeleteAsync(id: Guid) : Task<bool>
  }

  interface "ILocalStore\n(INT-003)" as INT_003 {
    + SaveClockingAsync(clocking: Clocking) : Task<int>
    + SaveSyncRecordAsync(localId: int, status: SyncStatus) : Task
    + GetPendingSyncRecords() : Task<List<SyncRecord>>
    + GetClockingByLocalId(localId: int) : Task<Clocking?>
    + UpdateSyncStatus(localId: int, status: SyncStatus) : Task
  }

  interface "IExportService\n(INT-004)" as INT_004 {
    + GenerateCSV(clockings: List<Clocking>) : byte[]
  }

  interface "INetworkHealth\n(INT-005)" as INT_005 {
    + CheckHealth() : HealthStatus
    + SubscribeHealthChanges(callback: Action<HealthStatus>) : IDisposable
  }

  interface "IAuditLogger\n(INT-006)" as INT_006 {
    + Log(entityType: string, entityId: string, action: string, user: string) : Task
  }

  class "TimeTrackingService\n(CLS-009)" as CLS_009 {
    - _repo: IRepository<Clocking>
    - _localStore: ILocalStore
    - _networkHealth: INetworkHealth
    - _syncQueue: SyncQueue
    - _exportService: IExportService
    + TimeTrackingService(repo, localStore, networkHealth, syncQueue, exportService)
    + ClockIn(employeeId: Guid) : Task<Result<Clocking>>
    + ClockOut(employeeId: Guid) : Task<Result<Clocking>>
    + GetClockings(employeeId: Guid, month: DateTime) : Task<List<Clocking>>
    + GetAllClockings(month: DateTime) : Task<List<Clocking>>
    + ExportClockings(month: DateTime) : Task<byte[]>
    - HandleHealthChanged(status: HealthStatus) : Task
  }

  class "NewsService\n(CLS-010)" as CLS_010 {
    - _repo: IRepository<NewsItem>
    - _auditLogger: IAuditLogger
    - _currentUser: string
    + NewsService(repo, auditLogger, currentUserAccessor)
    + PublishNews(item: NewsItem) : Task<Result<NewsItem>>
    + GetNewsList(category: string?) : Task<List<NewsItem>>
    + GetNewsDetail(id: Guid) : Task<NewsItem?>
  }

  class "DirectoryService\n(CLS-011)" as CLS_011 {
    - _repo: IRepository<Employee>
    - _authProvider: IAuthProvider
    - _auditLogger: IAuditLogger
    - _currentUser: string
    + DirectoryService(repo, authProvider, auditLogger, currentUserAccessor)
    + Search(query: string, dept: string?, office: string?) : Task<List<Employee>>
    + UpdateEmployee(emp: Employee) : Task<Result<Employee>>
    + SyncFromAD() : Task<Result<SyncResult>>
  }

  class "SyncQueue\n(CLS-013)" as CLS_013 {
    - _localStore: ILocalStore
    - _repo: IRepository<Clocking>
    - _writeLock: SemaphoreSlim(1, 1)
    - _flushLock: SemaphoreSlim(1, 1)
    + SyncQueue(localStore, repo)
    + Enqueue(clocking: Clocking) : Task<Result<int>>
    + Flush() : Task<SyncResult>
    + GetPendingCount() : Task<int>
    - DetectConflict(clocking: Clocking) : Task<bool>
  }

  class "AuditInterceptor\n(CLS-012)" as CLS_012 {
    - _auditLogger: IAuditLogger
    + AuditInterceptor(auditLogger)
    + Log(entityType: string, entityId: string, action: string, user: string) : Task
  }
}

CLS_009 --> INT_002 : uses
CLS_009 --> INT_003 : uses
CLS_009 --> INT_005 : uses
CLS_009 --> INT_004 : uses
CLS_009 --> CLS_013 : delegates
CLS_010 --> INT_002 : uses
CLS_010 --> INT_006 : uses
CLS_011 --> INT_002 : uses
CLS_011 --> INT_001 : uses
CLS_011 --> INT_006 : uses
CLS_013 --> INT_003 : uses
CLS_013 --> INT_002 : uses
CLS_012 --> INT_006 : uses

note right of CLS_013
  **Concurrency Design**
  SemaphoreSlim(1,1) for single-writer
  access to SQLite. Separate flush lock
  allows enqueue during flush.
  .NET 10 async/await — no manual threads.
end note

note left of INT_001
  **IAuthProvider**
  AD protocol isolated.
  LDAP or OAuth2 implementation
  is a DI registration change.
  Spike deferred to Construction.
end note

@enduml
```

#### Package: Domain Layer

The Domain package contains entity classes, value objects, and domain enums. Entities encapsulate business rules (validation, state transitions, merge logic). No dependencies on infrastructure or application layers.

```plantuml
@startuml
skinparam classAttributeIconSize 0
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam class {
  BackgroundColor #fff3e0
  BorderColor #37474f
}

title Package: Domain Layer — Design Classes (Full Signatures)

package "Domain" {
  class "Clocking\n(CLS-014)" as CLS_014 {
    + Id: Guid
    + EmployeeId: Guid
    + Type: ClockingType
    + Timestamp: DateTime
    + SyncStatus: SyncStatus
    + Clocking(employeeId: Guid, type: ClockingType, timestamp: DateTime)
    + MarkSynced() : void
    + MarkSkipped() : void
    + Validate() : ValidationResult
  }

  class "NewsItem\n(CLS-015)" as CLS_015 {
    + Id: Guid
    + Title: string
    + Body: string
    + PublishedDate: DateTime
    + Category: Category
    + IsFeatured: bool
    + NewsItem(title: string, body: string, category: Category, isFeatured: bool)
    + Validate() : ValidationResult
    + MarkAsFeatured() : void
    + UnmarkFeatured() : void
  }

  class "Employee\n(CLS-016)" as CLS_016 {
    + Id: Guid
    + AdId: string
    + FullName: string
    + JobTitle: string
    + Department: string
    + Office: string
    + Email: string
    + Extension: string
    + IsActive: bool
    + OverrideFlag: bool
    + Validate() : ValidationResult
    + MergeFromAD(adRecord: ADEmployeeRecord) : void
    + CreateFromAD(adRecord: ADEmployeeRecord) : Employee
    + SetOverrideFlag(flag: bool) : void
    + Deactivate() : void
  }

  class "AuditEntry\n(CLS-017)" as CLS_017 {
    + Id: Guid
    + EntityType: string
    + EntityId: string
    + Action: string
    + User: string
    + Timestamp: DateTime
    + AuditEntry(entityType: string, entityId: string, action: string, user: string)
  }

  class "SyncRecord\n(CLS-018)" as CLS_018 {
    + LocalId: int
    + ClockingId: Guid
    + Status: SyncStatus
    + QueuedAt: DateTime
    + SyncedAt: DateTime?
    + SyncRecord(clockingId: Guid, queuedAt: DateTime)
    + MarkSynced() : void
    + MarkSkipped() : void
  }

  class "ADEmployeeRecord\n(CLS-019)" as CLS_019 {
    + AdId: string
    + FullName: string
    + JobTitle: string
    + Department: string
    + Office: string
    + Email: string
  }

  class "SyncResult\n(CLS-020)" as CLS_020 {
    + Synced: int
    + Skipped: int
    + Imported: int
    + Errors: List<string>
    + SyncResult(synced: int, skipped: int, imported: int)
    + HasErrors: bool
  }

  class "Result<T>\n(CLS-021)" as CLS_021 {
    + Value: T?
    + IsSuccess: bool
    + Errors: List<string>
    + static Ok(value: T) : Result<T>
    + static Fail(errors: List<string>) : Result<T>
    + static Fail(error: string) : Result<T>
  }

  class "ValidationResult\n(CLS-022)" as CLS_022 {
    + IsValid: bool
    + Errors: List<string>
    + static Ok() : ValidationResult
    + static Fail(errors: List<string>) : ValidationResult
  }

  enum "ClockingType" as CT {
  IN
  OUT
  }
  enum "SyncStatus" as SS {
  PENDING
  SYNCED
  SKIPPED
  }
  enum "Category" as CAT {
  General
  HR
  IT
  Events
  }
  enum "HealthStatus" as HS {
  UP
  DOWN
  }
}

CLS_014 --> CT
CLS_014 --> SS
CLS_018 --> SS
CLS_015 --> CAT
CLS_016 ..> CLS_019 : created from
CLS_020 ..> CLS_021 : uses

note right of CLS_016
  **AD Sync Conflict Resolution**
  OverrideFlag = true → local HR
  changes win over AD sync data.
  RISK-R01 (RPN 30).
end note

note right of CLS_014
  **Clocking Aggregate Root**
  MarkSynced/MarkSkipped transition
  the SyncStatus state machine.
  See state machine diagram.
end note

@enduml
```

#### Package: Infrastructure Layer

The Infrastructure package contains concrete implementations of all application-layer interfaces. Each class realizes an interface, enabling substitution via DI. EF Core 10.0 with Npgsql for PostgreSQL, EF Core with SQLite for local store.

```plantuml
@startuml
skinparam classAttributeIconSize 0
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam class {
  BackgroundColor #fce4ec
  BorderColor #37474f
}
skinparam interface {
  BackgroundColor #fffde7
  BorderColor #f57f17
}

title Package: Infrastructure Layer — Design Classes (Full Signatures)

package "Infrastructure" {
  class "PostgresRepository<T>\n(CLS-023)" as CLS_023 {
    - _context: PortalDbContext
    - _dbSet: DbSet<T>
    + PostgresRepository(context: PortalDbContext)
    + GetByIdAsync(id: Guid) : Task<T?>
    + QueryAsync(predicate: Expression<Func<T, bool>>) : Task<List<T>>
    + SaveAsync(entity: T) : Task<Result<T>>
    + DeleteAsync(id: Guid) : Task<bool>
  }

  class "SqliteLocalStore\n(CLS-024)" as CLS_024 {
    - _sqliteContext: LocalDbContext
    + SaveClockingAsync(clocking: Clocking) : Task<int>
    + SaveSyncRecordAsync(localId: int, status: SyncStatus) : Task
    + GetPendingSyncRecords() : Task<List<SyncRecord>>
    + GetClockingByLocalId(localId: int) : Task<Clocking?>
    + UpdateSyncStatus(localId: int, status: SyncStatus) : Task
  }

  class "LdapAuthProvider\n(CLS-025)" as CLS_025 {
    - _ldapConfig: LdapConfig
    + Authenticate(username: string, password: string) : Task<AuthResult>
    + GetCurrentUser(claims: ClaimsPrincipal) : PortalUser
    + FetchAllEmployeesAsync() : Task<List<ADEmployeeRecord>>
  }

  class "CsvExporter\n(CLS-026)" as CLS_026 {
    + GenerateCSV(clockings: List<Clocking>) : byte[]
    - FormatRow(c: Clocking) : string
  }

  class "TcpHealthMonitor\n(CLS-027)" as CLS_027 {
    - _connectionString: string
    - _probeInterval: TimeSpan
    - _timer: Timer
    - _currentStatus: HealthStatus
    - _subscribers: List<Action<HealthStatus>>
    + TcpHealthMonitor(connectionString: string, probeInterval: TimeSpan)
    + CheckHealth() : HealthStatus
    + SubscribeHealthChanges(callback: Action<HealthStatus>) : IDisposable
    - ProbeAsync() : Task
    - NotifySubscribers(status: HealthStatus) : void
  }

  class "EfAuditLogger\n(CLS-028)" as CLS_028 {
    - _context: PortalDbContext
    + EfAuditLogger(context: PortalDbContext)
    + Log(entityType: string, entityId: string, action: string, user: string) : Task
  }

  class "PortalDbContext\n(CLS-029)" as CLS_029 {
    + Clockings: DbSet<Clocking>
    + NewsItems: DbSet<NewsItem>
    + Employees: DbSet<Employee>
    + AuditEntries: DbSet<AuditEntry>
    + SaveChangesAsync() : Task<int>
  }

  class "LocalDbContext\n(CLS-030)" as CLS_030 {
    + LocalClockings: DbSet<Clocking>
    + SyncRecords: DbSet<SyncRecord>
    + SaveChangesAsync() : Task<int>
  }

  class "LdapConfig\n(CLS-031)" as CLS_031 {
    + Server: string
    + Port: int
    + BaseDn: string
    + BindDn: string
    + BindPassword: string
  }

  class "AuthResult\n(CLS-032)" as CLS_032 {
    + IsSuccess: bool
    + User: PortalUser?
    + Error: string?
  }

  class "PortalUser\n(CLS-033)" as CLS_033 {
    + EmployeeId: Guid?
    + Username: string
    + DisplayName: string
    + IsHrAdmin: bool
  }
}

CLS_023 ..|> INT_002 : IRepository<T>
CLS_024 ..|> INT_003 : ILocalStore
CLS_025 ..|> INT_001 : IAuthProvider
CLS_026 ..|> INT_004 : IExportService
CLS_027 ..|> INT_005 : INetworkHealth
CLS_028 ..|> INT_006 : IAuditLogger

interface "IRepository<T>\n(INT-002)" as INT_002
interface "ILocalStore\n(INT-003)" as INT_003
interface "IAuthProvider\n(INT-001)" as INT_001
interface "IExportService\n(INT-004)" as INT_004
interface "INetworkHealth\n(INT-005)" as INT_005
interface "IAuditLogger\n(INT-006)" as INT_006

CLS_023 --> CLS_029 : uses
CLS_024 --> CLS_030 : uses
CLS_028 --> CLS_029 : uses
CLS_025 --> CLS_031 : config
CLS_025 ..> CLS_032 : returns
CLS_025 ..> CLS_033 : returns

note right of CLS_027
  **TcpHealthMonitor**
  TCP probe to PostgreSQL port 5432
  every 5 seconds. Maximum 5s
  detection window for network restore.
end note

note right of CLS_025
  **LdapAuthProvider**
  AD protocol implementation.
  Spike deferred to Construction
  per Elaboration decision.
  RISK-T02 (RPN 35).
end note

@enduml
```

#### Package: Design Model Organization

```plantuml
@startuml
skinparam packageStyle rectangle
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam component {
  BackgroundColor<<presentation>> #e8f5e9
  BackgroundColor<<application>> #e3f2fd
  BackgroundColor<<domain>> #fff3e0
  BackgroundColor<<infrastructure>> #fce4ec
  BorderColor #37474f
}

title Design Model — Package Organization & Dependencies

package "Presentation Layer" <<presentation>> {
  [HomePage\n(CLS-001)] as P1
  [HistoryPage\n(CLS-003)] as P2
  [AdminClockingsPage\n(CLS-005)] as P3
  [AdminNewsPage\n(CLS-004)] as P4
  [NewsListPage\n(CLS-016)] as P5
  [NewsDetailPage\n(CLS-017)] as P6
  [DirectoryPage\n(CLS-007)] as P7
  [AdminDirectoryPage\n(CLS-008)] as P8
  [ClockingController\n(CLS-002)] as PC1
  [NewsController\n(CLS-006)] as PC2
  [DirectoryController\n(CLS-008b)] as PC3
}

package "Application Layer" <<application>> {
  [TimeTrackingService\n(CLS-009)] as A1
  [NewsService\n(CLS-010)] as A2
  [DirectoryService\n(CLS-011)] as A3
  [SyncQueue\n(CLS-013)] as A4
  [AuditInterceptor\n(CLS-012)] as A5
}

package "Domain Layer" <<domain>> {
  [Clocking\n(CLS-014)] as D1
  [NewsItem\n(CLS-015)] as D2
  [Employee\n(CLS-016)] as D3
  [AuditEntry\n(CLS-017)] as D4
  [SyncRecord\n(CLS-018)] as D5
  [ADEmployeeRecord\n(CLS-019)] as D6
  [SyncResult\n(CLS-020)] as D7
  [Result<T>\n(CLS-021)] as D8
  [ValidationResult\n(CLS-022)] as D9
}

package "Infrastructure Layer" <<infrastructure>> {
  [PostgresRepository<T>\n(CLS-023)] as I1
  [SqliteLocalStore\n(CLS-024)] as I2
  [LdapAuthProvider\n(CLS-025)] as I3
  [CsvExporter\n(CLS-026)] as I4
  [TcpHealthMonitor\n(CLS-027)] as I5
  [EfAuditLogger\n(CLS-028)] as I6
  [PortalDbContext\n(CLS-029)] as I7
  [LocalDbContext\n(CLS-030)] as I8
}

P1 ..> A1
P2 ..> A1
P3 ..> A1
P4 ..> A2
P5 ..> A2
P6 ..> A2
P7 ..> A3
P8 ..> A3
PC1 ..> A1
PC2 ..> A2
PC3 ..> A3

A1 ..> D1
A1 ..> D5
A1 ..> D7
A2 ..> D2
A3 ..> D3
A3 ..> D6
A3 ..> D7
A4 ..> D1
A4 ..> D5
A5 ..> D4

A1 ..> I1 : IRepository<Clocking>
A1 ..> I2 : ILocalStore
A1 ..> I5 : INetworkHealth
A1 ..> I4 : IExportService
A2 ..> I1 : IRepository<NewsItem>
A2 ..> I6 : IAuditLogger
A3 ..> I1 : IRepository<Employee>
A3 ..> I3 : IAuthProvider
A3 ..> I6 : IAuditLogger

I1 ..> D1
I1 ..> D2
I1 ..> D3
I1 ..> D4
I2 ..> D1
I2 ..> D5
I7 ..> D1
I7 ..> D2
I7 ..> D3
I7 ..> D4
I8 ..> D1
I8 ..> D5

note bottom of A4
  **Dependency direction:** Presentation → Application → Domain
  Infrastructure implements interfaces defined in Application.
  No upward dependencies. All cross-layer
  communication via interfaces (INT-001 through INT-006).
end note

@enduml
```

#### State Machine: SyncRecord Lifecycle

SyncRecord (CLS-018) has 3 distinct lifecycle states, requiring a state machine diagram per the quality criteria.

```plantuml
@startuml
skinparam shadowing false
skinparam defaultFontName "Segoe UI"

title State Machine: SyncRecord Lifecycle (CLS-018)

[*] --> PENDING : Enqueue(clocking)

PENDING --> SYNCED : Flush() succeeds\n(no timestamp conflict)
PENDING --> SKIPPED : Flush() detects\nduplicate (employeeId, timestamp)

SYNCED --> [*]
SKIPPED --> [*]

note right of PENDING
  Clocking persisted in SQLite
  local store. User has received
  confirmation (sync pending).
  Zero data loss guaranteed.
end note

note right of SYNCED
  Clocking successfully written
  to PostgreSQL. SyncRecord
  updated with syncedAt timestamp.
end note

note right of SKIPPED
  Duplicate detected by
  (employeeId, timestamp) uniqueness
  constraint in PostgreSQL.
  No data loss — original entry
  already persisted.
end note

@enduml
```

#### Design Class Summary

| ID | Class | Package | Layer | Realizes Interface |
|---|---|---|---|---|
| CLS-001 | HomePage | Presentation | Presentation | — |
| CLS-002 | ClockingController | Presentation | Presentation | — |
| CLS-003 | HistoryPage | Presentation | Presentation | — |
| CLS-004 | AdminNewsPage | Presentation | Presentation | — |
| CLS-005 | AdminClockingsPage | Presentation | Presentation | — |
| CLS-006 | NewsController | Presentation | Presentation | — |
| CLS-007 | DirectoryPage | Presentation | Presentation | — |
| CLS-008 | AdminDirectoryPage | Presentation | Presentation | — |
| CLS-009 | TimeTrackingService | Application | Application | — |
| CLS-010 | NewsService | Application | Application | — |
| CLS-011 | DirectoryService | Application | Application | — |
| CLS-012 | AuditInterceptor | Application | Application | — |
| CLS-013 | SyncQueue | Application | Application | — |
| CLS-014 | Clocking | Domain | Domain | — |
| CLS-015 | NewsItem | Domain | Domain | — |
| CLS-016 | Employee | Domain | Domain | — |
| CLS-017 | AuditEntry | Domain | Domain | — |
| CLS-018 | SyncRecord | Domain | Domain | — |
| CLS-019 | ADEmployeeRecord | Domain | Domain | — |
| CLS-020 | SyncResult | Domain | Domain | — |
| CLS-021 | Result<T> | Domain | Domain | — |
| CLS-022 | ValidationResult | Domain | Domain | — |
| CLS-023 | PostgresRepository<T> | Infrastructure | Infrastructure | INT-002 |
| CLS-024 | SqliteLocalStore | Infrastructure | Infrastructure | INT-003 |
| CLS-025 | LdapAuthProvider | Infrastructure | Infrastructure | INT-001 |
| CLS-026 | CsvExporter | Infrastructure | Infrastructure | INT-004 |
| CLS-027 | TcpHealthMonitor | Infrastructure | Infrastructure | INT-005 |
| CLS-028 | EfAuditLogger | Infrastructure | Infrastructure | INT-006 |
| CLS-029 | PortalDbContext | Infrastructure | Infrastructure | — |
| CLS-030 | LocalDbContext | Infrastructure | Infrastructure | — |
| CLS-031 | LdapConfig | Infrastructure | Infrastructure | — |
| CLS-032 | AuthResult | Infrastructure | Infrastructure | — |
| CLS-033 | PortalUser | Infrastructure | Infrastructure | — |
## Interface Contracts
### Interface Contracts

All subsystem boundaries are defined by interfaces. No concrete class is referenced across a layer boundary. Each interface specifies operation signatures with pre/postconditions. The Implementer translates these directly into C# interface definitions.

```plantuml
@startuml
skinparam classAttributeIconSize 0
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam interface {
  BackgroundColor #fffde7
  BorderColor #f57f17
}

title Interface Contracts — Subsystem Boundary Specifications

interface "IAuthProvider\n(INT-001)" as INT_001 {
  + Authenticate(username: string, password: string) : Task<AuthResult>
  + GetCurrentUser(claims: ClaimsPrincipal) : PortalUser
  + FetchAllEmployeesAsync() : Task<List<ADEmployeeRecord>>
  --
  Pre: username and password not null
  Post: AuthResult.IsSuccess == true iff AD bind succeeds
  Post: PortalUser.IsHrAdmin determined by AD group membership
}

interface "IRepository<T>\n(INT-002)" as INT_002 {
  + GetByIdAsync(id: Guid) : Task<T?>
  + QueryAsync(predicate: Expression<Func<T, bool>>) : Task<List<T>>
  + SaveAsync(entity: T) : Task<Result<T>>
  + DeleteAsync(id: Guid) : Task<bool>
  --
  Pre: entity != null for SaveAsync
  Post: SaveAsync returns Result.Ok with persisted entity (Id assigned)
  Post: QueryAsync never returns null (empty list if no matches)
}

interface "ILocalStore\n(INT-003)" as INT_003 {
  + SaveClockingAsync(clocking: Clocking) : Task<int>
  + SaveSyncRecordAsync(localId: int, status: SyncStatus) : Task
  + GetPendingSyncRecords() : Task<List<SyncRecord>>
  + GetClockingByLocalId(localId: int) : Task<Clocking?>
  + UpdateSyncStatus(localId: int, status: SyncStatus) : Task
  --
  Pre: SQLite file exists and is writable
  Post: SaveClockingAsync returns auto-incremented localId
  Post: GetPendingSyncRecords returns only status == PENDING records
}

interface "IExportService\n(INT-004)" as INT_004 {
  + GenerateCSV(clockings: List<Clocking>) : byte[]
  --
  Pre: clockings list not null
  Post: Returns RFC 4180 compliant CSV byte array
  Post: Columns: employee, date, time-in, time-out, duration
}

interface "INetworkHealth\n(INT-005)" as INT_005 {
  + CheckHealth() : HealthStatus
  + SubscribeHealthChanges(callback: Action<HealthStatus>) : IDisposable
  --
  Post: CheckHealth returns current cached status (no blocking I/O)
  Post: SubscribeHealthChanges invokes callback on status transitions
  Post: Probe interval = 5 seconds (configurable)
}

interface "IAuditLogger\n(INT-006)" as INT_006 {
  + Log(entityType: string, entityId: string, action: string, user: string) : Task
  --
  Pre: all parameters non-null, non-empty
  Post: AuditEntry persisted with immutable timestamp (DateTime.UtcNow)
  Post: Audit entries are append-only — no update or delete
}

note bottom of INT_001
  **Provided by:** Infrastructure (LdapAuthProvider, CLS-025)
  **Consumed by:** DirectoryService (CLS-011)
  **Subsystem boundary:** Application ↔ Infrastructure
end note

note bottom of INT_002
  **Provided by:** Infrastructure (PostgresRepository<T>, CLS-023)
  **Consumed by:** TimeTrackingService, NewsService, DirectoryService, SyncQueue
  **Subsystem boundary:** Application ↔ Infrastructure
end note

note bottom of INT_003
  **Provided by:** Infrastructure (SqliteLocalStore, CLS-024)
  **Consumed by:** SyncQueue (CLS-013)
  **Subsystem boundary:** Application ↔ Infrastructure
end note

note bottom of INT_004
  **Provided by:** Infrastructure (CsvExporter, CLS-026)
  **Consumed by:** TimeTrackingService (CLS-009)
  **Subsystem boundary:** Application ↔ Infrastructure
end note

note bottom of INT_005
  **Provided by:** Infrastructure (TcpHealthMonitor, CLS-027)
  **Consumed by:** TimeTrackingService (CLS-009)
  **Subsystem boundary:** Application ↔ Infrastructure
end note

note bottom of INT_006
  **Provided by:** Infrastructure (EfAuditLogger, CLS-028)
  **Consumed by:** NewsService, DirectoryService, AuditInterceptor
  **Subsystem boundary:** Application ↔ Infrastructure
end note

@enduml
```

### Interface Summary

| Interface | ID | Provider | Consumers | Operations | NFR Addressed |
|---|---|---|---|---|---|
| IAuthProvider | INT-001 | LdapAuthProvider (CLS-025) | DirectoryService (CLS-011) | Authenticate, GetCurrentUser, FetchAllEmployeesAsync | REQ-001 (AD auth), RISK-T02 |
| IRepository<T> | INT-002 | PostgresRepository<T> (CLS-023) | TimeTrackingService, NewsService, DirectoryService, SyncQueue | GetByIdAsync, QueryAsync, SaveAsync, DeleteAsync | REQ-017 (<1s clock), REQ-018 (<2s search) |
| ILocalStore | INT-003 | SqliteLocalStore (CLS-024) | SyncQueue (CLS-013) | SaveClockingAsync, SaveSyncRecordAsync, GetPendingSyncRecords, GetClockingByLocalId, UpdateSyncStatus | REQ-014 (offline, zero data loss) |
| IExportService | INT-004 | CsvExporter (CLS-026) | TimeTrackingService (CLS-009) | GenerateCSV | UC-003 (CSV export) |
| INetworkHealth | INT-005 | TcpHealthMonitor (CLS-027) | TimeTrackingService (CLS-009) | CheckHealth, SubscribeHealthChanges | REQ-014 (5-min offline tolerance) |
| IAuditLogger | INT-006 | EfAuditLogger (CLS-028) | NewsService, DirectoryService, AuditInterceptor | Log | REQ-004, REQ-005, REQ-006 (audit trail) |

### Testability Entry Points

All interfaces are DI-injectable, enabling unit testing with mock implementations:

| Interface | Test Double Strategy | Observable State |
|---|---|---|
| IAuthProvider | Mock with predefined AuthResult and ADEmployeeRecord list | AuthResult.IsSuccess, PortalUser.IsHrAdmin |
| IRepository<T> | In-memory list with LINQ-based QueryAsync | Entity.Id assigned on Save, list contents |
| ILocalStore | In-memory dictionary keyed by localId | SyncRecord.Status transitions (PENDING→SYNCED/SKIPPED) |
| IExportService | Mock returning predefined byte[] | CSV byte array content |
| INetworkHealth | Mock with controllable HealthStatus and callback trigger | HealthStatus.UP/DOWN transitions |
| IAuditLogger | Capture Log calls in test list | AuditEntry records (entityType, action, user) |
## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| HomePage (<<view>>) | UC-001, UC-005, COMP-P1 | Derives | ClockingController, NewsController |
| HistoryPage (<<view>>) | UC-002, COMP-P1 | Derives | ClockingController |
| NewsListPage (<<view>>) | UC-005, COMP-P2 | Derives | NewsController |
| NewsDetailPage (<<view>>) | UC-005, COMP-P2 | Derives | NewsController |
| DirectoryPage (<<view>>) | UC-006, COMP-P3 | Derives | DirectoryController |
| AdminClockingsPage (<<view>>) | UC-003, COMP-P4 | Derives | ClockingController |
| AdminNewsPage (<<view>>) | UC-004, COMP-P2 | Derives | NewsController |
| AdminDirectoryPage (<<view>>) | UC-007, COMP-P3 | Derives | DirectoryController |
| ClockingController (<<controller>>) | UC-001, UC-002, UC-003 | Derives | (Designer: domain classes) |
| NewsController (<<controller>>) | UC-004, UC-005 | Derives | (Designer: domain classes) |
| DirectoryController (<<controller>>) | UC-006, UC-007 | Derives | (Designer: domain classes) |
| P-001 (Nav Bar) | REQ-042 | Derives | All view classes |
| P-002 (Primary Button) | REQ-030, REQ-038, REQ-039 | Derives | HomePage, AdminNewsPage, AdminClockingsPage |
| P-003 (Confirmation) | REQ-031, REQ-035 | Derives | HomePage, AdminNewsPage, AdminDirectoryPage |
| P-004 (Error Recovery) | REQ-036, REQ-041, REQ-043 | Derives | All view classes |
| P-005 (Real-Time Search) | REQ-008, REQ-033 | Derives | DirectoryPage |
| P-006 (Single-Screen Form) | REQ-039, REQ-040 | Derives | AdminNewsPage, AdminDirectoryPage |
| P-007 (Table List) | REQ-032, REQ-038, REQ-040 | Derives | HistoryPage, AdminClockingsPage, AdminDirectoryPage |
| P-008 (Loading Indicator) | REQ-045 | Derives | BasePage (all) |
| P-009 (Keyboard Accessibility) | REQ-044 | Derives | BasePage (all) |
| UC-001 Interaction Flow | UC-001 Main Flow, AF-1, EF-1 | Realizes | HomePage, ClockingController |
| UC-002 Interaction Flow | UC-002 Main Flow | Realizes | HistoryPage, ClockingController |
| UC-003 Interaction Flow | UC-003 Main Flow | Realizes | AdminClockingsPage, ClockingController |
| UC-004 Interaction Flow | UC-004 Main Flow | Realizes | AdminNewsPage, NewsController |
| UC-005 Interaction Flow | UC-005 Main Flow | Realizes | NewsListPage, NewsDetailPage, NewsController |
| UC-006 Interaction Flow | UC-006 Main Flow | Realizes | DirectoryPage, DirectoryController |
| UC-007 Interaction Flow | UC-007 Main Flow, S3 | Realizes | AdminDirectoryPage, DirectoryController |