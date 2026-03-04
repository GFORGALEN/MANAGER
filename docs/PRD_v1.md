# Construction Project Management System (MVP)

## 1. Project Goal

Build a lightweight project management system for small construction or property development companies.

The system focuses on:
- Tracking project variations
- Managing tasks related to project execution
- Recording evidence (attachments)
- Providing a simple project dashboard

This is an MVP designed for internal use by a small construction company.

---

# 2. Technology Stack

Backend:
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server (local dev) / Azure SQL (production)

Frontend:
- Vue 3
- TypeScript
- Ant Design Vue
- Axios

Cloud (future):
- Azure App Service
- Azure SQL Database
- Azure Blob Storage

---

# 3. User Roles

### Admin (Boss / Owner)

Permissions:
- Approve or reject variations
- View all projects
- View dashboard summary

### PM (Project Manager)

Permissions:
- Create and edit variations
- Submit variations for approval
- Create tasks
- Upload attachments

### Contractor (optional)

Permissions:
- View assigned tasks
- Update task status
- Upload evidence

---

# 4. Core Entities

## Project

Fields:

- ProjectId (GUID)
- Name
- Address
- Stage (Consenting / Procurement / Construction / Handover)
- Status (Active / OnHold / Completed)
- CreatedAt
- UpdatedAt

---

## Variation

Represents a change request.

Fields:

- VariationId
- ProjectId
- Title
- Description
- EstimatedAmount
- FinalAmount
- TimeImpactDays
- Status
- SubmittedByUserId
- SubmittedAt
- ApprovedByUserId
- ApprovedAt
- DecisionNote
- CreatedAt
- UpdatedAt

---

## Task

Represents execution work.

Fields:

- TaskId
- ProjectId
- VariationId (optional)
- Title
- Description
- AssigneeUserId
- DueDate
- Status
- CreatedAt
- UpdatedAt

Task Status:

- Todo
- Doing
- Done
- Closed

---

## Attachment

Evidence file.

Fields:

- AttachmentId
- ProjectId
- VariationId (optional)
- TaskId (optional)
- FileName
- BlobUrl
- ContentType
- SizeBytes
- UploadedByUserId
- UploadedAt

---

# 5. Key Features

### Project Dashboard

Shows:

- total variations
- approved variations
- rejected variations
- overdue tasks

---

### Variation Workflow

1. PM creates Draft
2. PM submits variation
3. Admin approves / rejects / asks for more info
4. Approved variations may generate tasks

---

### Task Board

Kanban style:

Todo → Doing → Done

---

### Attachments

Files can be uploaded to:

- project
- variation
- task

Stored in Azure Blob Storage.

---

# 6. MVP Scope

Include:

- Projects CRUD
- Variations workflow
- Tasks board
- Attachments upload
- Basic dashboard summary

Exclude (future):

- Notifications
- AI summarization
- Calendar planning
- Advanced analytics