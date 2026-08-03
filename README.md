# Hospital-Clinic-Management-System-Team-3

Welcome to the **Hospital / Clinic Management System**, our capstone project for the Spark to Code 2026 bootcamp. This platform bridges the gap between healthcare providers and patients, allowing clinic administrators and doctors to manage departments, schedules, medical records, and prescriptions while enabling patients to book appointments, view lab results, and receive billing invoices.

This project represents the accumulation of everything covered throughout the bootcamp, alongside key self-study architectural milestones (JWT Auth and Email Services) required for production backend deployment.

---

## 🚀 Business Core & Features

The platform serves as a multi-tier clinical management ecosystem built around **12 core entities** split across three functional domains:

* **Clinical & Appointment Operations:** Patients can browse doctor profiles, view available clinic rooms, and book appointments, while doctors manage consultation schedules and availability.


* **Electronic Health Records (EHR) & Diagnostics:** Doctors record diagnostic findings, issue digital prescriptions linked to medication catalogs, and order lab tests with trackable results.


* **Administration & Financial Billing:** System admins structure medical departments and specializations, while the billing module automatically generates invoices upon appointment completion.



---

## 🛠️ Tech Stack & Architecture

This application utilizes a decoupled, modern multi-tier architecture focusing heavily on a robust, highly optimized relational database backend.

* **Database Engine:** Microsoft SQL Server
* **Data Access Tier:** Entity Framework Core (EF Core) via standard Code-First Migration patterns.


* **Backend REST Framework:** ASP.NET Core Web API written in C#


* **Data Querying Engine:** Language Integrated Query (LINQ) optimized for relational joins (`Include`), filtering, sorting, and aggregate operations.


* **Frontend Client:** Responsive HTML5, JavaScript (ES6+), and Bootstrap CSS.



---

## 🗺️ Project Execution Roadmap

The repository is organized following a strict, clean-code lifecycle structure:

* **ERD & Relational Mapping:** Crafting complex relationships across 12 core models, including 1:1 (User to Profile), 1:N (Department to Doctors), and M:N (Doctor to Specializations, Prescription to Medications).


* **C# Model Generation:** Building deterministic Domain Entities with strong data types, constraints, and navigational properties.


* **EF Core DbContext:** Managing the DbContext lifecycle, mapping configurations using Fluent API or Data Annotations, and maintaining clean local SQL Server connection behaviors.


* **Web API Controllers (8-Case Standard):** Building secure Web API controllers for each entity, implementing at least 8 distinct usage cases per controller (CRUD, filtering, sorting, aggregation, and relational projection).


* **Frontend UI Integration:** Constructing Bootstrap-based frontend screens that consume all API endpoints, handle JWT storage, and support user interactions.



---

## 🧪 Advanced Implementation & Self-Study Requirements

Beyond standard CRUD logic, this backend implements two production-grade micro-features researched independently using official Microsoft Documentation:

### 🔑 1. Stateless Authentication (JSON Web Tokens - JWT)

* Replaced vulnerable session states with secure, stateless, signed JWTs.


* Implements role-based access control (RBAC) to cleanly partition endpoints so only authorized roles (`Patient`, `Doctor`, `Admin`) can access sensitive clinical operations.


* **Resources Used:** `Microsoft.AspNetCore.Authentication.JwtBearer`


### 📧 2. Asynchronous Email Notification Service

* Integrates a reliable backend notification system to automatically send an appointment-confirmation email when a patient books a visit, along with a reminder email 24 hours before the scheduled appointment.


* Abstracted via a dependency-injected messaging interface utilizing standard SMTP transport parameters (`MailKit` / `MimeKit`).



---

## 👥 Team Distribution & Model Ownership

The project development is divided equally among **6 developers**, with each developer owning 2 models end-to-end (ERD, C# models, DbContext, 8-case API controller, and frontend UI):

| Developer | Assigned Entities & Models | Primary Responsibilities |
| --- | --- | --- |
| **Developer 1** | `User` & `PatientProfile` | Authentication Flow (JWT Register/Login), Patient Profile Management

 |
| **Developer 2** | `DoctorProfile` & `Department` | Doctor Directory & Qualifications, Department Administration

 |
| **Developer 3** | `Specialization` & `Appointment` | Medical Specializations Catalog, Appointment Scheduling & Notifications

 |
| **Developer 4** | `MedicalRecord` & `Prescription` | Clinical Diagnostics, Treatment Plans & Prescription Management

 |
| **Developer 5** | `Medication` & `LabTest` | Pharmacy Inventory Catalog, Lab Test Orders & Diagnostics

 |
| **Developer 6** | `Invoice` & `Room` | Patient Billing & Payment Tracking, Room Assignment & Availability

 |

---

## 💻 Local Quickstart

### Prerequisites

* **.NET 8.0 SDK** (or higher)
* **SQL Server Express** (`.\SQLEXPRESS`) or SQL Server LocalDB
* **Visual Studio 2022** / **VS Code**

### 1. Database Provisioning

Ensure your local connection string inside `appsettings.json` correctly points to your local SQL Server instance. Open the **Package Manager Console** in Visual Studio (`Tools ➔ NuGet Package Manager ➔ Package Manager Console`) and run the following commands to generate and apply your database schema instantly:

```bash
# Create the initial schema snapshot mapping your 12 entities
Add-Migration InitialCreate

# Execute the migration script to build the database inside .\SQLEXPRESS
Update-Database

```

---
