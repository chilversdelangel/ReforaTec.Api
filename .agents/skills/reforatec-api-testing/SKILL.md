---
name: reforatec-api-testing
description: >-
  Guides testing and validating ReforaTec.Api endpoints using curl, OpenAPI specification,
  the development OTP backdoor, and JWT token inspection.
  Use this skill when asked to test, curl, verify API responses, authenticate, or validate tokens.
---

# ReforaTec API Testing Runbook (curl & OpenAPI)

This skill provides step-by-step procedures to launch the API, inspect routes via OpenAPI, authenticate via the QA backdoor, and test endpoints using `curl`.

---

## 1. Starting the API

Ensure the PostgreSQL container is running before launching the API:

```bash
# Verify database container is running
podman ps --filter "name=reforatec-db"

# Start the API in Development mode (port 5210)
dotnet run --project src/ReforaTec.Api/ReforaTec.Api.csproj --launch-profile http
```

The server listens on `http://localhost:5210`.

---

## 2. Discovering Endpoints via OpenAPI

Inspect registered endpoints and schemas directly from the OpenAPI 3.0 document:

```bash
# List all route paths and HTTP methods
curl -s http://localhost:5210/openapi/v1.json | jq '.paths | keys'

# Inspect a specific endpoint definition
curl -s http://localhost:5210/openapi/v1.json | jq '.paths["/api/v1/trees"]'
```

---

## 3. Authentication & Token Extraction

ReforaTec uses passwordless email + OTP authentication. In non-production environments, the master OTP code `000000` is active.

### Step A: Request OTP Code
```bash
curl -i -X POST http://localhost:5210/api/v1/otp-codes \
  -H "Content-Type: application/json" \
  -d '{"email":"student@cdmadero.tecnm.mx"}'
```
Expected response: `HTTP/1.1 204 No Content`.

### Step B: Verify OTP & Obtain Tokens
```bash
RESPONSE=$(curl -s -X POST http://localhost:5210/api/v1/sessions \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@cdmadero.tecnm.mx",
    "otpCode": "000000",
    "audience": "student-mobile-app"
  }')

ACCESS_TOKEN=$(echo "$RESPONSE" | jq -r .accessToken)
echo "Access Token: $ACCESS_TOKEN"
```

### Step C: Inspect JWT Payload & Claims
Decode the Base64Url payload to verify that identity claims are present:
```bash
echo "$ACCESS_TOKEN" | cut -d. -f2 | base64 -d 2>/dev/null | jq
```
Verify the payload contains:
- `sub`: User ID (e.g., `"1"`)
- `tid`: Tenant ID (e.g., `"1"`)
- `role`: User role (e.g., `"Student"`)
- `aud`: `"student-mobile-app"`

---

## 4. Consuming Protected Endpoints

Pass the access token in the `Authorization: Bearer` header:

```bash
# Example: Get assigned trees for authenticated user
curl -i -X GET http://localhost:5210/api/v1/users/me/trees \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Example: Create a new resource
curl -i -X POST http://localhost:5210/api/v1/trees/1/services \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "serviceTypeId": 1,
    "deviceCapturedAt": "2026-10-14T15:30:00-06:00",
    "comment": "Regular maintenance"
  }'
```

---

## 5. Validating Error Responses (ProblemDetails)

Verify that business and validation errors follow RFC 9457 (`application/problem+json`):

- **400 Bad Request / Validation**: Must include `"type"`, `"title"`, and an `"errors"` dictionary.
- **401 Unauthorized**: Missing or malformed token (`"auth/unauthorized"`).
- **403 Forbidden**: Insufficient permissions or tree care assignment mismatch (`"user/not-assigned-to-tree"`).
- **404 Not Found**: Resource missing in the current tenant (`"tree/not-found"`).
- **409 Conflict**: Duplicate record in the tenant.
