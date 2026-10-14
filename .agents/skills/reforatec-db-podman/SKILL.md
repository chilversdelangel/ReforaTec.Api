---
name: reforatec-db-podman
description: >-
  Guides managing the PostgreSQL database container in Podman and executing SQL queries via psql.
  Use this skill when asked to inspect the database, check tables, query rows, or verify Podman database container status.
---

# ReforaTec Database Management with Podman & psql

This skill provides procedures for managing the local PostgreSQL container via Podman and executing queries against the database without requiring external tools.

---

## 1. Container Configuration

| Property | Value |
| :--- | :--- |
| **Container Name** | `reforatec-db` |
| **Database Engine** | PostgreSQL 18 |
| **Database Name** | `reforatec` |
| **Username** | `postgres` |
| **Port Mapping** | `5432:5432` |

---

## 2. Container Lifecycle

```bash
# Check if container is running
podman ps --filter "name=reforatec-db"

# Check container status (including exited/stopped)
podman ps -a --filter "name=reforatec-db"

# Start the database container
podman start reforatec-db

# Stop the database container
podman stop reforatec-db

# View container logs
podman logs --tail 50 reforatec-db
```

---

## 3. Executing SQL via psql

Execute queries non-interactively using `podman exec`:

```bash
# Syntax template
podman exec -i reforatec-db psql -U postgres -d reforatec -c "<SQL_QUERY>"

# Example: List all tables
podman exec -i reforatec-db psql -U postgres -d reforatec -c "\dt"

# Example: Describe a specific table schema
podman exec -i reforatec-db psql -U postgres -d reforatec -c "\d users"
```

---

## 4. Key Verification Queries

### Verify Seeded Tenants
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT id, acronym, institutional_email_domain, is_deleted FROM tenants;"
```

### Verify Users & Roles
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT id, email, tenant_id, \"current_role\", is_email_verified, is_deleted FROM users;"
```

### Verify Trees by Tenant
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT id, tenant_id, species_id, health_state, planting_date FROM trees WHERE tenant_id = 1;"
```

### Verify Active Student Tree Assignments
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT id, user_id, tree_id, campaign_id, start_date, end_date FROM user_cares_for_trees WHERE end_date IS NULL;"
```

### Inspect Active OTP Codes
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT user_id, verification_code, failed_attempts, expires_at FROM auth_otp_codes;"
```

### Inspect Active Sessions & Refresh Tokens
```bash
podman exec -i reforatec-db psql -U postgres -d reforatec -c \
  "SELECT id, user_id, audience, expires_at, is_revoked FROM refresh_tokens WHERE NOT is_revoked;"
```
