# Employee Portal — Deployment Strategy

## Deployment Mode

**Custom-Built** — internal intranet application deployed on a single Windows Server within the corporate network.

- No cloud hosting
- No external access
- No shrink-wrap or downloadable distribution
- Installation performed by IT staff (Miguel Torres)

## Target Environment

| Attribute | Value |
|---|---|
| Server | Single Windows Server (internal corporate network) |
| Application Host | .NET 10 Kestrel (no reverse proxy for 200 users) |
| Database | PostgreSQL (primary), SQLite (offline buffer) |
| Authentication | Active Directory via LDAP/OAuth2 (decision pending) |
| Client Browsers | Chrome, Edge (current versions only) |
| Network | Corporate intranet — 3 offices (HQ + 2 branches) |
| Availability Window | Monday–Friday 07:00–19:00 |

## Rollout Approach

1. **Elaboration:** Deploy executable prototype to dev server
2. **Construction:** Iterative builds deployed to dev server; each iteration's UCs pass acceptance
3. **Transition — Beta:** Deploy to production Windows Server; pilot with HR staff (5 users)
4. **Transition — Full Rollout:** All 200 employees gain access; target 80% adoption in 3 months

## Rollback Criteria

- AD integration failure (auth unavailable >15 min) → roll back; enable local auth fallback
- Offline sync data loss detected → halt deployment; investigate SQLite sync queue
- Page load >5s for >10% of users → roll back; investigate performance regression
- Portal unavailable during 07:00–19:00 for >30 min → roll back; engage IT for diagnostics

## Two-Gate Acceptance

| Gate | Location | Criteria |
|---|---|---|
| Gate 1 — Dev Site | Dev Windows Server | All UCs pass; performance met; offline sync validated; AD functional |
| Gate 2 — Production | Corporate Windows Server | Beta pilot: HR completes all 3 UCs without assistance; 5-min offline test passes; CSV export verified |

## Bill of Materials (Planned)

- Employee Portal application (.NET 10 self-contained)
- PostgreSQL database schema scripts
- AD connection configuration
- Release Notes (evolved each phase)
- Installation Guide (Transition)
- HR Admin Quick Reference (Transition)
- SQLite offline buffer

## Notes

- Deployment topology is defined in the SAD Deployment View (single-node, intranet-only)
- Deployment Model artifact [OMITTED — trigger not fired: single-node, not distributed/multi-env]
- CI/CD pipeline skeleton bootstrapped in `.github/workflows/deploy.yml`
- IaC base bootstrapped in `infra/Chart.yaml` (placeholder for future Helm chart if containerization is adopted)
