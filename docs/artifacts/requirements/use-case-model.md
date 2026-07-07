## Document Control

| Field | Value |
|---|---|
| Phase | Elaboration |
| Status | Draft |
| Iteration | 1 (Cycle 1) |
| Milestone Target | End of Elaboration |
| Author | System Analyst |

### Elaboration Iteration 1 Changes

- Phase transition from Inception (LCO approved). All 7 UCs now fully specified with activity diagrams.
- UC-001 (architecturally significant) enhanced with offline sync sequence diagram and 3 concrete scenarios.
- UC-004 and UC-007 activity diagrams added showing audit trail integration and AD sync conflict handling.
- All UC specifications preserved from Inception baseline; activity diagrams and scenario walkthroughs added for Elaboration depth (~80% detail).

## Use-Case Diagram

```plantuml
@startuml
left to right direction
skinparam packageStyle rectangle
skinparam actorStyle awesome
skinparam usecaseBorderColor #2c3e50
skinparam usecaseBackgroundColor #ecf0f1

actor "Employee" as EMP
actor "HR Administrator" as HR
actor "Active Directory" as AD <<external system>>

rectangle "Employee Portal — System Boundary" {
  usecase "UC-001\nClock In/Out" as UC01
  usecase "UC-002\nView Clocking History" as UC02
  usecase "UC-003\nReview and Export\nClockings" as UC03
  usecase "UC-004\nPublish News" as UC04
  usecase "UC-005\nRead News" as UC05
  usecase "UC-006\nSearch Directory" as UC06
  usecase "UC-007\nManage Directory" as UC07
}

EMP --> UC01
EMP --> UC02
EMP --> UC05
EMP --> UC06

HR --> UC03
HR --> UC04
HR --> UC07

UC01 ..> AD : <<include>>
UC02 ..> AD : <<include>>
UC03 ..> AD : <<include>>
UC04 ..> AD : <<include>>
UC05 ..> AD : <<include>>
UC06 ..> AD : <<include>>
UC07 ..> AD : <<include>>

note bottom of UC01
  **Architecturally significant**
  Offline fault tolerance:
  5-min network drop, no data loss,
  auto-sync on restore.
  Priority: Must | Stability: Low
end note

note bottom of UC04
  Audit trail required
  (REQ-004, REQ-006)
end note

note bottom of UC07
  Audit trail required
  (REQ-005, REQ-006)
  AD sync conflict handling
end note

@enduml
```

## Actors

| ID | Actor | Type | Description | Associated UCs |
|---|---|---|---|---|
| ACT-001 | Employee | Human (primary) | 200 corporate users across 3 offices. Uses AD credentials to access portal. Clocks in/out, views own history, reads news, searches directory. | UC-001, UC-002, UC-005, UC-006 |
| ACT-002 | HR Administrator | Human (primary) | HR staff member with elevated permissions. Publishes news, manages directory entries, reviews all clockings, exports CSV reports. | UC-003, UC-004, UC-007 |
| ACT-003 | Active Directory | External system | Corporate identity provider. Authenticates all users via LDAP/OAuth2. Provides employee data for directory synchronization. Cross-cutting mechanism — not a use case actor in the traditional sense; included by all UCs. | <<include>> from all UCs |

## Use-Case Survey

| ID | Use Case | Primary Actor | Trigger | Outcome (Value) | MoSCoW | Stability | Architecturally Significant? | Stakeholder Source |
|---|---|---|---|---|---|---|---|---|
| UC-001 | Clock In/Out | Employee | Employee accesses portal to record work time | Timestamp recorded with confirmation; works offline | Must | Low | **Yes** — offline fault tolerance drives architectural decisions | "Employee logs in with corporate credentials (Active Directory). Main screen shows Clock In or Clock Out button depending on current status. System records exact time and shows confirmation." |
| UC-002 | View Clocking History | Employee | Employee wants to review own clockings | Current month clocking history displayed | Must | High | No | "Employee can view their clocking history for the current month." |
| UC-003 | Review and Export Clockings | HR Administrator | HR needs monthly clocking report | All employees' clockings viewable; CSV export generated | Must | Medium | No | "HR can view all employees' clockings and export a monthly report in CSV." |
| UC-004 | Publish News | HR Administrator | HR has announcement to distribute | News item published with title, body, date, category, featured flag | Must | Medium | No | "HR publishes internal news and announcements (title, body, date, category)." |
| UC-005 | Read News | Employee | Employee opens portal main page | News list displayed sorted by date with category filter and featured banner | Must | High | No | "Employees see news on main page sorted by date, can filter by category (General, HR, IT, Events). Featured news appears with a banner at the top. Read-only for employees — no comments or reactions." |
| UC-006 | Search Directory | Employee | Employee needs colleague's contact info | Matching directory entries displayed with name, title, department, office, email, extension | Must | High | No | "Employee searches for colleagues by name, department, or office. Each entry shows: name, job title, department, office, email, and extension phone number." |
| UC-007 | Manage Directory | HR Administrator | HR needs to update employee directory data | Directory entry created/updated/deactivated via admin panel | Must | Medium | No | "HR keeps data up to date from an administration panel. Directory shows corporate data only — no private personal information." |

**ATM Test verification:** All 7 use cases pass — each has (a) a primary actor who initiates, (b) a clear trigger, and (c) a measurable outcome delivering observable value.

**Scope guard notes:**
- AD authentication is a cross-cutting mechanism included by all UCs — NOT a standalone use case (per Rule 7).
- No UCs inferred beyond declared scope. All 7 UCs trace verbatim to declared stakeholder requirements.
- No `[SCOPE_QUESTION]` or `[DERIVED]` markers needed — all UCs are literally declared.
- Stakeholder confirmation (S1, 2026-07-07): all 4 declared processes confirmed correct.

## Use-Case Specifications

### UC-001: Clock In/Out ⭐ Architecturally Significant

| Field | Value |
|---|---|
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee accesses portal to record work time start or end |
| Precondition | Employee is authenticated via AD (or has valid cached session for offline mode) |
| Postcondition | Clocking timestamp is recorded and confirmed; if offline, timestamp is queued for sync |
| Priority | Must |
| Stability | Low — offline fault tolerance mechanism is primary technical risk |
| Includes | AD Authentication (cross-cutting) |

**Main Flow:**
1. Employee navigates to portal home page
2. System authenticates employee via Active Directory (`<<include>>`)
3. System checks employee's current clocking status
4. System displays "Clock In" button (if clocked out) or "Clock Out" button (if clocked in)
5. Employee clicks the displayed button
6. System records exact timestamp
7. System displays confirmation with recorded time

**Alternative Flows:**
- **AF-1: Network drop (offline mode):** If AD or server is unreachable (network drop ≤5 min), system uses cached session, records timestamp locally, queues for sync. On network restore, syncs queued data with zero data loss.
- **AF-2: Already clocked in/out:** If employee attempts to clock in when already clocked in, system shows current status and does not create duplicate entry.

**Activity Diagram (UC-001 Flow):**

```plantuml
@startuml
start
:Employee accesses portal;
:Portal authenticates via AD (<<include>>);
if (AD reachable?) then (yes)
  :Authentication succeeds;
else (no — network drop)
  :Use cached session token;
  note right
    Offline fault tolerance:
    up to 5 min network drop.
    No data loss acceptable.
  end note
endif
:Portal checks current clocking status;
if (Currently clocked OUT?) then (yes)
  :Show **Clock In** button;
  :Employee clicks Clock In;
  :Record timestamp (local + queued for sync);
  :Show confirmation with exact time;
else (no — currently clocked IN)
  :Show **Clock Out** button;
  :Employee clicks Clock Out;
  :Record timestamp (local + queued for sync);
  :Show confirmation with exact time;
endif
if (Network restored & data pending sync?) then (yes)
  :Sync queued clockings to server;
  :Verify no data loss;
else (no)
endif
stop
@enduml
```

**Sequence Diagram (UC-001 Offline Sync Scenario):**

```plantuml
@startuml
title UC-001: Clock In/Out — Offline Sync Sequence (Architecturally Significant)

actor "Employee" as EMP
participant "Portal UI\n(Razor Pages)" as UI
participant "Clocking\nController" as CTRL
participant "Local Cache\n(Offline Store)" as CACHE
participant "AD Auth\nService" as AD
participant "Clocking\nAPI (.NET 10)" as API
participant "PostgreSQL" as DB

== Normal (Online) Flow ==
EMP -> UI: Access portal
UI -> AD: Authenticate (LDAP/OAuth2)
AD --> UI: Auth success + session token
UI -> CTRL: Get clocking status
CTRL -> API: GET /clockings/status/{employeeId}
API -> DB: Query current status
DB --> API: Status = clocked OUT
API --> CTRL: Show "Clock In" button
CTRL --> UI: Render clock-in view
EMP -> UI: Click "Clock In"
UI -> CTRL: POST clock-in
CTRL -> API: POST /clockings {timestamp}
API -> DB: INSERT clocking record
DB --> API: Record saved
API --> CTRL: Confirmation
CTRL --> UI: Show confirmation with time

== Offline Flow (Network Drop ≤5 min) ==
EMP -> UI: Access portal (network down)
UI -> AD: Authenticate (unreachable)
note right: AD unreachable → use cached session token
UI -> CACHE: Validate cached session
CACHE --> UI: Cached session valid
UI -> CTRL: Get clocking status
CTRL -> CACHE: Read local status
CACHE --> CTRL: Status = clocked IN
CTRL --> UI: Show "Clock Out" button
EMP -> UI: Click "Clock Out"
UI -> CTRL: POST clock-out
CTRL -> CACHE: Store timestamp locally + queue for sync
CACHE --> CTRL: Queued confirmation
CTRL --> UI: Show confirmation with time

== Network Restore — Auto Sync ==
CACHE -> API: Sync queued clockings (on network restore)
API -> DB: INSERT queued records
DB --> API: All records saved
API --> CACHE: Sync success — clear queue
note right: Zero data loss verified;\nqueue cleared on confirmed sync

@enduml
```

**Concrete Scenarios (Discovery Walkthroughs):**

| Scenario | Actor | Context | Flow | Outcome | Discovered Requirements |
|---|---|---|---|---|---|
| S-001: First clock-in of the day | Carlos (Employee, Havana office) | Arrives at 8:45 AM, opens portal on Chrome | Main flow steps 1-7 | Clock-in recorded at 08:45:12, confirmation shown | None new — standard flow |
| S-002: Network drop during clock-out | María (Employee, Santiago office) | Clocks out at 17:02, network drops at 17:01 | AF-1: Cached session used, timestamp stored locally, queued for sync. Network restores at 17:04. Sync completes. | Clock-out recorded at 17:02:00, synced at 17:04:15, zero data loss | REQ-013 (offline tolerance), REQ-014 (no data loss), REQ-015 (graceful recovery) |
| S-003: Duplicate clock-in attempt | Jorge (Employee, Havana office) | Already clocked in at 08:30, clicks portal again at 10:15 | AF-2: System shows "You are currently clocked in since 08:30" — no duplicate created | No duplicate entry; current status displayed | None new — AF-2 covers this |

### UC-002: View Clocking History

| Field | Value |
|---|---|
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee wants to review own clockings for current month |
| Precondition | Employee is authenticated via AD |
| Postcondition | Current month clocking history is displayed |
| Priority | Must |
| Stability | High |

**Main Flow:**
1. Employee navigates to clocking history view
2. System authenticates employee via AD (`<<include>>`)
3. System retrieves employee's clocking records for the current month
4. System displays history list (date, clock-in time, clock-out time)

**Alternative Flows:**
- **AF-1: No clockings this month:** System displays empty state message.

**Activity Diagram (UC-002 Flow):**

```plantuml
@startuml
title UC-002: View Clocking History — Activity Diagram

start
:Employee navigates to clocking history view;
:Portal authenticates via AD (<<include>>);
:System retrieves employee clocking records for current month;
if (Records exist?) then (yes)
  :Display history list (date, clock-in time, clock-out time);
else (no)
  :Display empty state message;
endif
stop
@enduml
```

### UC-003: Review and Export Clockings

| Field | Value |
|---|---|
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR needs to review or export monthly clocking data |
| Precondition | HR Administrator is authenticated via AD with HR role |
| Postcondition | Clocking data displayed and/or CSV file generated |
| Priority | Must |
| Stability | Medium |

**Main Flow:**
1. HR Administrator navigates to clocking review panel
2. System authenticates via AD (`<<include>>`)
3. System displays all employees' clockings for the current month
4. HR Administrator optionally selects a different month
5. HR Administrator clicks "Export CSV"
6. System generates CSV file with all clocking records for selected month
7. System downloads CSV file to HR Administrator's browser

**Alternative Flows:**
- **AF-1: No clockings for selected month:** System displays empty state.

**Activity Diagram (UC-003 Flow):**

```plantuml
@startuml
title UC-003: Review and Export Clockings — Activity Diagram

start
:HR Admin navigates to clocking review panel;
:Portal authenticates via AD (<<include>>);
:System loads all employees' clockings for current month;
:Display clocking summary table (employee, date, in-time, out-time);

if (HR selects different month?) then (yes)
  :HR selects target month;
  :System reloads clockings for selected month;
  :Display updated table;
else (no)
endif

if (HR clicks "Export CSV"?) then (yes)
  :System generates CSV file (RFC 4180);
  :CSV contains: employee, date, clock-in, clock-out;
  :Browser downloads CSV file;
  :Display export confirmation;
else (no — view only)
  :HR reviews data on screen;
endif

if (No clockings for selected month?) then (yes)
  :Display empty state message;
else (no)
endif

stop
@enduml
```

### UC-004: Publish News

| Field | Value |
|---|---|
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR has an announcement to publish |
| Precondition | HR Administrator is authenticated via AD with HR role |
| Postcondition | News item is published and visible to employees; audit trail entry created |
| Priority | Must |
| Stability | Medium |

**Main Flow:**
1. HR Administrator navigates to news management panel
2. System authenticates via AD (`<<include>>`)
3. HR Administrator enters news title, body, selects category (General, HR, IT, Events), and optionally marks as featured
4. HR Administrator clicks "Publish"
5. System saves news item with current date
6. System creates audit trail entry (who, what, when)
7. System confirms publication

**Alternative Flows:**
- **AF-1: Edit existing news:** HR selects existing item, modifies fields, saves. Audit trail updated.
- **AF-2: Delete news:** HR selects item, confirms deletion. Audit trail updated.

**Activity Diagram (UC-004 Flow):**

```plantuml
@startuml
title UC-004: Publish News — Activity Diagram

start
:HR Admin navigates to news management panel;
:Portal authenticates via AD (<<include>>);

if (Create new?) then (yes)
  :HR enters title, body, selects category;
  :HR optionally marks as Featured;
  :HR clicks Publish;
  :System saves news item with current date;
  :System creates audit trail entry;
  :Display publication confirmation;
else (no — edit existing)
  :HR selects existing news item;
  :HR modifies fields;
  :HR clicks Save;
  :System updates news item;
  :System creates audit trail entry;
  :Display update confirmation;
endif

stop
@enduml
```

### UC-005: Read News

| Field | Value |
|---|---|
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee opens portal main page |
| Precondition | Employee is authenticated via AD |
| Postcondition | News list displayed with filtering and featured banner |
| Priority | Must |
| Stability | High |

**Main Flow:**
1. Employee navigates to portal home page
2. System authenticates via AD (`<<include>>`)
3. System displays featured news banner at top (if any featured items exist)
4. System displays news list sorted by date (most recent first)
5. Employee optionally selects a category filter (General, HR, IT, Events)
6. System filters news list by selected category

**Alternative Flows:**
- **AF-1: No news items:** System displays empty state message.
- **AF-2: No featured news:** Banner section is hidden; news list starts at top.

**Activity Diagram (UC-005 Flow):**

```plantuml
@startuml
title UC-005: Read News — Activity Diagram

start
:Employee navigates to portal home page;
:Portal authenticates via AD (<<include>>);
if (Featured news exists?) then (yes)
  :Display featured news banner at top;
else (no)
  :Hide banner section;
endif
:System displays news list sorted by date (most recent first);
if (Employee selects category filter?) then (yes)
  :System filters news by selected category;
  :Display filtered results;
else (no)
  :Display all news;
endif
if (No news items?) then (yes)
  :Display empty state message;
else (no)
endif
stop
@enduml
```

### UC-006: Search Directory

| Field | Value |
|---|---|
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee needs to find a colleague's contact information |
| Precondition | Employee is authenticated via AD |
| Postcondition | Matching directory entries displayed with corporate contact data |
| Priority | Must |
| Stability | High |

**Main Flow:**
1. Employee navigates to directory page
2. System authenticates via AD (`<<include>>`)
3. Employee enters search criteria (name, department, or office)
4. System displays matching entries showing: name, job title, department, office, email, extension phone number
5. Employee reviews results

**Alternative Flows:**
- **AF-1: No matches:** System displays "No results found" message.
- **AF-2: Browse all:** Employee leaves search empty; system shows all entries (paginated).

**Activity Diagram (UC-006 Flow):**

```plantuml
@startuml
title UC-006: Search Directory — Activity Diagram

start
:Employee navigates to directory page;
:Portal authenticates via AD (<<include>>);
if (Employee enters search criteria?) then (yes)
  :Employee enters name, department, or office;
  :System searches matching entries;
else (no — browse all)
  :System loads all entries (paginated);
endif
if (Matches found?) then (yes)
  :Display results: name, title, department, office, email, extension;
else (no)
  :Display "No results found" message;
endif
stop
@enduml
```

### UC-007: Manage Directory

| Field | Value |
|---|---|
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR needs to update employee directory data |
| Precondition | HR Administrator is authenticated via AD with HR role |
| Postcondition | Directory entry created/updated/deactivated; audit trail entry created |
| Priority | Must |
| Stability | Medium |

**Main Flow:**
1. HR Administrator navigates to directory admin panel
2. System authenticates via AD (`<<include>>`)
3. HR Administrator selects an employee entry to edit (or creates new)
4. HR Administrator updates fields: name, job title, department, office, email, extension phone number
5. HR Administrator saves changes
6. System creates audit trail entry (who, what, when)
7. System confirms update

**Alternative Flows:**
- **AF-1: Deactivate entry:** HR marks entry as inactive; entry hidden from directory search but retained in database.
- **AF-2: AD sync conflict:** If AD-synchronized data conflicts with manual edit, system flags conflict for HR resolution.

**Activity Diagram (UC-007 Flow):**

```plantuml
@startuml
title UC-007: Manage Directory — Activity Diagram

start
:HR Admin navigates to directory admin panel;
:Portal authenticates via AD (<<include>>);

if (Create new entry?) then (yes)
  :HR enters name, title, department, office, email, extension;
  :HR clicks Save;
  :System creates directory entry;
  :System creates audit trail entry;
  :Display creation confirmation;
else (no — edit existing)
  :HR selects employee entry;
  :HR updates fields;
  :HR clicks Save;
  if (AD-synced field changed?) then (yes)
    :System flags AD sync conflict;
    if (HR overrides?) then (yes)
      :System saves manual override;
      :System creates audit trail entry;
    else (no)
      :System reverts to AD value;
    endif
  else (no)
    :System saves changes;
    :System creates audit trail entry;
  endif
  :Display update confirmation;
endif

stop
@enduml
```

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| UC-001 | FEAT-001, FEAT-010 | Refines | ACL-001 (future), TC-001 (future) |
| UC-002 | FEAT-002 | Refines | ACL-001 (future), TC-002 (future) |
| UC-003 | FEAT-003 | Refines | ACL-002 (future), TC-003 (future) |
| UC-004 | FEAT-004, FEAT-006 | Refines | ACL-003 (future), TC-004 (future) |
| UC-005 | FEAT-005 | Refines | ACL-003 (future), TC-005 (future) |
| UC-006 | FEAT-007 | Refines | ACL-004 (future), TC-006 (future) |
| UC-007 | FEAT-008 | Refines | ACL-004 (future), TC-007 (future) |
| ACT-001 | STK-003 | — | UC-001, UC-002, UC-005, UC-006 |
| ACT-002 | STK-001 | — | UC-003, UC-004, UC-007 |
| ACT-003 | STK-002, CON-004 | — | <<include>> from all UCs |