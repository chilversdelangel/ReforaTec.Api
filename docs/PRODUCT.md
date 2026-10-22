# ReforaTec — Product Domain & Business Specification

## 1. Vision & Institutional Purpose
**ReforaTec** is a multi-tenant software platform engineered to digitize, monitor, and audit environmental reforestation and tree care campaigns across educational campuses of the **Tecnológico Nacional de México (TecNM / ITCM)**.

### Academic & Formative Objectives
* **Complementary Credit Accreditation (`Crédito Complementario`):** Students actively participate in environmental care campaigns to fulfill their mandatory *Complementary Credits* (an extracurricular graduation requirement in the TecNM curriculum), instilling environmental stewardship and civic responsibility.
* **Permanent Patrimonial Asset:** Planted trees are not transient or disposable; they represent enduring institutional assets. An individual tree remains registered in the system for decades and is transferred across successive student cohorts and semesters.
* **Institutional & Civic Values:** Every cataloged tree is christened with an institutional core value from TecNM's educational model (e.g. *Responsibility*, *Environmental Care*, *Respect*, *Perseverance*, *Loyalty*), embodying the student's personal commitment to their campus.

---

## 2. The 4 System Actors & Core Responsibilities

```text
┌────────────────────────────────────────────────────────────────────────┐
│                        COORDINATOR (Web Portal)                        │
│ • Creates semester campaigns and provisions enrollment codes.          │
│ • Assigns trees to individual students or collaborative care teams.    │
│ • Evaluates final survival audits and approves Complementary Credits.  │
└───────────────────┬────────────────────────────────┬───────────────────┘
                    │                                │
                    ▼                                ▼
┌──────────────────────────────────────┐ ┌───────────────────────────────┐
│         STUDENT (Mobile App)         │ │     INSPECTOR (Mobile App)    │
│ • Enrolls via InscriptionCode.       │ │ • Conducts weekly audits.     │
│ • Cares for assigned tree cohort.    │ │ • Performs TreeInspections.   │
│ • Logs daily services (water, prune).│ │ • Evaluates health & photos.  │
│ • Receives alerts if health drops.   │ │ • Triggers automated alerts.  │
└──────────────────────────────────────┘ └───────────────────────────────┘
```

| Role | Client Interface | Domain Responsibility |
| :--- | :--- | :--- |
| **Student (`Student`)** | Android Mobile App | Primary caretaker. Performs routine tree maintenance (watering, pruning, fertilizing) and responds promptly to health alerts. |
| **Inspector (`Inspector`)** | Android Mobile App | Objective technical auditor. Conducts weekly campus surveys, records phytosanitary health, captures metric growth during key milestones, and triggers alerts. |
| **Coordinator (`Coordinator`)** | Web Portal | Campus environmental administrator. Manages campaigns, oversees campus-wide tree inventory, assigns tree cohorts, and evaluates final credit accreditation. |
| **System Admin (`SystemAdmin`)** | Web Portal | Multi-tenant infrastructure operator, managing institution records, botanical catalogs, and global security policies. |

---

## 3. Campaign Dynamics, Enrollment & Tree Assignment

### 3.1. Student Enrollment (Hybrid Flow)
1. The Coordinator creates a semester campaign (e.g. `ReforaTec 2026-A`) and the system provisions an alphanumeric `InscriptionCode` (e.g. `ITCM-2026A`).
2. The Student logs into the mobile app using their institutional email (`@cdmadero.tecnm.mx`) and enrolls in the campaign by submitting the `InscriptionCode`.
3. The Coordinator assigns one or more cataloged trees to the student via the web dashboard.

### 3.2. Collaborative Care (Team Assignment per Tree)
* A student may care for **one or multiple trees** (typically a cohort of 1 to 5 trees).
* **Shared Caretaking:** An individual tree may have **two or more students actively assigned simultaneously** within the same campaign (`UserCaresForTree` records with `EndDate == null`).
* **Shared Authorization:** Any student actively assigned to a tree holds legitimate authorization to log maintenance services on behalf of the care team.

---

## 4. Care Services vs. Technical Inspections (Ubiquitous Language)

The domain strictly delineates between **student routine care logs** (`Service`) and **impartial inspector audits** (`TreeInspection`):

```text
┌────────────────────────────────────────────────────────────────────────┐
│                      STUDENT ROUTINE SERVICES                          │
│ Service Catalog: Watering, Pruning, Fertilizing, Fumigation, Weeding   │
│ • Focus: Encourage daily care routines with zero unnecessary friction. │
│ • Logical Anti-Spam: The API enforces logical cooldown limits to       │
│   prevent batch gaming (e.g., max 1 watering service per tree/day).    │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼
┌────────────────────────────────────────────────────────────────────────┐
│                     INSPECTOR TREE INSPECTIONS                         │
│ Performed weekly by assigned inspectors (`UserInspectsCampaign`):      │
│                                                                        │
│ 1. WeeklyRoutine (90% of visits):                                      │
│    • Qualitative Health Check: TreeHealthState (Healthy,               │
│      NeedsAttention, Critical, Dead) + ObservationNotes.               │
│    • Height, diameter, and photo remain null.                          │
│                                                                        │
│ 2. Milestone Inspections (Baseline & FinalDelivery):                   │
│    • Baseline: Captured once for newly planted trees.                  │
│    • FinalDelivery: Captured at semester conclusion with coordinator.  │
│    • Mandatory Metrics: Height (cm), Diameter (mm), EvidencePhotoUrl.  │
│                                                                        │
│ 3. Automated Health Alerts:                                            │
│    • If health drops to 'NeedsAttention' or 'Critical', the tree's     │
│      status updates immediately and an automated Push Alert (FCM) is   │
│      dispatched to all assigned student caretakers.                    │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Milestone Inheritance & Campaign Transition

To prevent redundant field measurements and support campus tree longevity across academic semesters:

```text
[ CAMPAIGN 2026-A ]                                        [ CAMPAIGN 2026-B ]
┌──────────────────────────────────────┐                   ┌──────────────────────────────────────┐
│ FinalDelivery Inspection             │                   │ Inherited Baseline                   │
│ • Height: 155 cm                     │ ────────────────► │ • Height: 155 cm (Inherited)         │
│ • Diameter: 16.2 mm                  │   (Auto-Link)     │ • Diameter: 16.2 mm (Inherited)      │
│ • Photo: latest_evidence.jpg         │                   │ • Photo: latest_evidence.jpg         │
└──────────────────────────────────────┘                   └──────────────────────────────────────┘
(No physical re-measurement needed at launch of new semester for existing campus trees)
```

1. **Veteran Trees:** When a new campaign launches, existing campus trees inherit the `FinalDelivery` metrics of the preceding campaign as their starting baseline.
2. **Newly Planted Trees:** Newly introduced trees undergo an initial `Baseline` inspection with tape measure and photographic capture.
3. **Growth Computation:** Absolute semester growth is calculated cleanly:
   $$\Delta \text{Growth} = \text{FinalDelivery.Height} - \text{Baseline.Height}$$

---

## 6. Campaign Closure & Credit Accreditation

Upon reaching the campaign's scheduled conclusion (`Campaign.Period.EndDate`):
1. **Final Delivery Inspection (`FinalDelivery`):**
   * The inspection team, accompanied by the Coordinator, conducts an on-site audit to record final height/diameter dimensions and verify tree survival.
2. **Evaluation & Approval Criteria:**
   * **Tree Survival Priority:** The fundamental requirement for credit approval is that the assigned tree has survived (`TreeHealthState != Dead`).
   * **App Care History as Supporting Evidence:** The longitudinal service log in the app acts as verifiable proof of student diligence in case of disputes or weather-related tree stress.
   * **Liberation:** The Coordinator grants the *Complementary Credit* approval in the web portal, and the system formalizes assignment closure (`EndDate = DateOnly.FromDateTime(DateTime.UtcNow)`).

---

## 7. Physical Identification & Campus Digitalization

### Current Physical Reality
* Historically, every tree on campus is tagged with an embossed metal plaque or identification number (`CampaignFolio`).
* Campus locations were traditionally maintained on hand-drawn physical maps.

### ReforaTec Digital Innovation
* **Satellite / GPS Mapping:** ReforaTec enriches the physical `CampaignFolio` with high-precision **GPS coordinates (Latitude and Longitude)** and campus zone descriptors.
* **Interactive Map:** Both Students and Inspectors view a live digital map within the mobile app, navigating directly to individual trees across academic buildings, plazas, and green spaces.

---

## 8. Mobile Offline-First Architecture

Given intermittent Wi-Fi and cellular coverage across expansive campus grounds and perimeter gardens:
1. **Dual Timestamping:**
   * `DeviceCapturedAt`: Local timestamp recorded by the mobile device when the service or inspection is captured (used for chronological business logic).
   * `ServerSyncedAt`: Exact UTC timestamp when the record is persisted into PostgreSQL.
2. **Batch Synchronization:**
   * Mobile clients persist actions locally in SQLite/Room and upload queued records in batches once connectivity is restored.
   * Endpoints guarantee idempotent processing and graceful ingestion of burst synchronizations.
