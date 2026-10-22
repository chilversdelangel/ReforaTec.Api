# ReforaTec — Technical Architecture & Ingress Guidelines

## 1. Architectural Philosophy
ReforaTec adopts **Vertical Slice Architecture** combined with **Domain-Driven Design (DDD)** bounded aggregates. 

Instead of traditional horizontal layers (Controllers $\to$ Services $\to$ Repositories) that fragment use cases across multiple directories, every use case is packaged into an isolated, self-contained directory containing its endpoint mapping, input/output contracts, business logic, validation, and domain errors.

---

## 2. Vertical Slice Anatomy

Each use case resides under:
```text
src/ReforaTec.Api/Features/{Domain}/{UseCase}/
  ├── {UseCase}.cs       # IEndpoint implementation: route mapping + OpenAPI documentation
  ├── Request.cs         # Input DTO record (internal sealed)
  ├── Response.cs        # Output DTO record (internal sealed)
  ├── Handler.cs         # Pure business logic returning ErrorOr<T> (internal static)
  ├── Validator.cs       # FluentValidation AbstractValidator<Request> (internal sealed)
  └── Errors.cs          # Domain-specific errors using kebab-case codes (internal static)
```

### Slice Rules:
* **No MediatR / No CQRS:** Commands and queries are invoked directly through `Handler.Handle(...)` for zero overhead and immediate traceability.
* **Minimal APIs:** Endpoints implement the custom `IEndpoint` interface and are auto-discovered at startup via assembly scanning.
* **Encapsulation:** DTOs (`Request`, `Response`), `Validator`, and `Handler` classes MUST be `internal sealed` (or `internal static`).
* **Domain Aggregate Cohesion:** Operations on child entities or subresources strictly scoped to an Aggregate Root (e.g., tree maintenance services, measurement photos) belong inside the Aggregate's feature folder (`Features/Trees/`), preserving domain cohesion and avoiding premature top-level folder fragmentation.

---

## 3. Multi-Tenant Security & Identity Model

### Mandatory Invariants:
1. **Tenant Isolation:** Every query and mutation on tenant-scoped entities MUST enforce `TenantId == user.TenantId`. Never trust client-provided tenant identifiers.
2. **Identity Resolution:** Resolved strictly through `ICurrentUserService` and `UserIdentity` from JWT claims:
   ```csharp
   var identityResult = currentUser.GetIdentity();
   if (identityResult.IsError)
   {
       return identityResult.Errors.ToProblem();
   }
   ```
3. **JWT Claim Standard:** Custom claims use standard snake_case (e.g. `tenant_id`, `sub`, `role`). Never use Microsoft-reserved short claims (`tid`, `oid`) to prevent unwanted ASP.NET Core inbound claim mapping.
4. **Resource Authorization:** Attempting to query, mutate, or associate resources not actively assigned or owned within the tenant MUST return `Error.Forbidden` (`StatusCodes.Status403Forbidden`).
5. **User Enumeration Prevention (OWASP):** Authentication flows (e.g. `RequestOtp`) MUST return `Result.Success` even if the user does not exist, preventing attackers from probing valid campus emails.

---

## 4. Ingress, Remote Management & Communication Stack

```text
                            STUDENT / INSPECTOR MOBILE APP
                                          │
                                          ▼
                        https://api.reforatec.org (Cloudflare)
                         (TLS 1.3, DDoS Shield, DNS Anycast)
                                          │
                                          ▼ (Zero-Port Outbound Tunnel)
               ┌────────────────────────────────────────────────────────┐
               │ ON-PREMISES VM AT ITCM CAMPUS (Ubuntu Server)          │
               │                                                        │
               │  [ Tailscale SSH ] ◄── Remote admin from developer PC  │
               │                                                        │
               │  [ Docker Compose Stack (reforatec_net) ]              │
               │   ├── cloudflared (Tunnel edge connector)              │
               │   ├── reforatec-api (.NET 10 Chiseled)                 │
               │   │    └── OTP Delivery via Resend (no-reply@reforatec)│
               │   └── postgres:18 (PostgreSQL persistent volume)       │
               │                                                        │
               │  [ GitHub Actions Self-Hosted Runner ]                 │
               │   └── Automated CD: pull & rolling restart on main push│
               └────────────────────────────────────────────────────────┘
```

* **Production Ingress:** Cloudflare Tunnel (`cloudflared`) connects the internal API container to `api.reforatec.org` with zero open ports on the institutional firewall.
* **Remote Administration:** Tailscale provides encrypted mesh VPN access directly to the VM via `ssh user@reforatec-vm` without exposing port 22 to the public internet.
* **Transactional Email Delivery:** Resend delivers OTP verification codes from `no-reply@reforatec.org`, verified via SPF, DKIM, and DMARC DNS records in Cloudflare.
