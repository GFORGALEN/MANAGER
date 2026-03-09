# Phase 2 Backend Specification
Project: Construction Management API
Stack:
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger

Current status:
The backend MVP CRUD skeleton is already implemented and working.

Existing entities:
- Project
- TaskItem
- Variation
- Attachment

Existing controllers:
- ProjectController
- TaskItemsController
- VariationsController
- AttachmentsController
- TestController

Existing capabilities:
- SQL Server + EF Core integration is working
- Migrations are working
- Swagger is enabled
- Basic CRUD endpoints already exist
- XML comments already enabled in Swagger
- Entity relationships are already configured

This phase should upgrade the current MVP backend into a cleaner, more production-ready structure.

---

# Phase 2 Goals

Implement the following improvements in order:

1. Add full update/edit endpoints (PUT or PATCH where appropriate)
2. Introduce DTO layering to avoid returning EF entities directly
3. Add service layer to separate controller logic from business logic
4. Add global exception handling middleware
5. Add structured logging
6. Add pagination, filtering and sorting for list endpoints
7. Add simple authentication and authorization
8. Add real file upload support for attachments
9. Keep Swagger documentation updated
10. Do not break existing working endpoints unless replacing them with improved versions

---

# Required Folder Structure

Backend should follow this structure:

- Controllers
- Entities
- DTOs
- Services
- Data
- Middleware
- Helpers (optional)
- Configurations (optional)

If needed, add subfolders under DTOs:
- DTOs/Projects
- DTOs/Tasks
- DTOs/Variations
- DTOs/Attachments

---

# 1. DTO Layer

## Rule
Controllers must no longer return EF Core entity models directly.

## Required DTOs

### Project DTOs
- ProjectListDto
- ProjectDetailDto
- CreateProjectDto
- UpdateProjectDto

### TaskItem DTOs
- TaskItemListDto
- TaskItemDetailDto
- CreateTaskItemDto
- UpdateTaskStatusDto
- UpdateTaskItemDto

### Variation DTOs
- VariationListDto
- VariationDetailDto
- CreateVariationDto
- UpdateVariationStatusDto
- UpdateVariationDto

### Attachment DTOs
- AttachmentListDto
- AttachmentDetailDto
- CreateAttachmentDto
- UpdateAttachmentDto (optional if needed)

## DTO Rules
- DTOs should only expose fields needed by API consumers
- Entity navigation objects must not be returned directly
- Keep request DTO validation using DataAnnotations

---

# 2. Full Update Endpoints

Add or improve these endpoints:

## Projects
- PUT /api/projects/{id}
Update project name, address and editable fields

## TaskItems
- PUT /api/tasks/{id}
Update full task editable fields
- PATCH /api/tasks/{id}/status
Keep existing status-only endpoint

## Variations
- PUT /api/variations/{id}
Update variation editable fields
- PATCH /api/variations/{id}/status
Keep existing status-only endpoint

## Attachments
- PUT /api/attachments/{id}
Optional metadata update if meaningful

## Response rules
- 200 OK for successful update
- 404 NotFound if entity does not exist
- 400 BadRequest for validation failures

---

# 3. Service Layer

Move business logic out of controllers.

Create services:

- IProjectService / ProjectService
- ITaskItemService / TaskItemService
- IVariationService / VariationService
- IAttachmentService / AttachmentService

## Rules
- Controllers should remain thin
- Services should contain EF Core query/update logic
- Use async methods
- Use dependency injection

---

# 4. Global Exception Handling

Add a global exception handling middleware.

## Requirements
- Catch unhandled exceptions
- Return consistent JSON format

Error response format:
{
  "code": "INTERNAL_SERVER_ERROR",
  "message": "An unexpected error occurred."
}

## Also handle business validation exceptions where useful

Examples:
- Entity not found
- Invalid task status transition
- Invalid variation status transition

---

# 5. Logging

Add structured logging.

## Requirements
- Log warnings when entities are not found
- Log errors in exception middleware
- Log important business actions such as:
  - project created
  - task created
  - variation status changed
  - attachment created/deleted

Use built-in ASP.NET Core logging abstraction.

---

# 6. Pagination, Filtering, Sorting

Improve list endpoints.

## Project list
GET /api/projects
Support:
- pageNumber
- pageSize
- keyword
- sortBy
- sortOrder

## Task list
GET /api/projects/{projectId}/tasks
Support:
- status
- pageNumber
- pageSize
- sortBy
- sortOrder

## Variation list
GET /api/projects/{projectId}/variations
Support:
- status
- pageNumber
- pageSize
- sortBy
- sortOrder

## Attachment list
GET /api/projects/{projectId}/attachments
Support:
- pageNumber
- pageSize
- sortBy
- sortOrder

## Rules
- Default page size should be reasonable (e.g. 10 or 20)
- Prevent unbounded large queries
- Return paged result shape like:

{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 35
}

---

# 7. Authentication and Authorization

Add simple JWT-based authentication.

## Scope for this phase
Do not build a full identity platform.
Implement a simple minimal auth system with seeded users.

## Add entity
- User

Suggested fields:
- UserId
- Username
- PasswordHash (or simplified placeholder if needed for MVP)
- Role

## Roles
- Admin
- PM
- Contractor

## Endpoints
- POST /api/auth/login

Return JWT token.

## Authorization rules
- Admin:
  full access
- PM:
  create and manage projects/tasks/variations/attachments
- Contractor:
  limited task viewing and status update only (basic implementation is enough)

## Minimal acceptable implementation
If role-based restrictions are too large for one step, first implement:
- login endpoint
- JWT token generation
- [Authorize] on controllers
- role claims scaffold

---

# 8. Real File Upload Support

Current attachment API only handles metadata.
Upgrade it to support real file upload.

## Requirements
Use multipart/form-data.

## Endpoints
- POST /api/projects/{projectId}/attachments/upload

## Behavior
- Accept one file upload
- Save file to local folder for now:
  /Uploads
- Save metadata to Attachment table:
  - original file name
  - stored path
  - content type
  - size
  - projectId

## Validation
- Max file size limit
- Allow only safe common file types if easy
- Return 400 for invalid uploads

## Note
Do NOT implement Azure Blob yet in this phase.
Use local storage first.

---

# 9. Status Validation Rules

## TaskItem allowed statuses
- Todo
- Doing
- Done

Reject any other value.

## Variation allowed statuses
- Draft
- Submitted
- Approved
- Rejected
- NeedInfo

Reject any other value.

If possible, add helper methods or enums to centralize status validation.

---

# 10. Swagger Requirements

Keep Swagger fully usable.

## Requirements
- Every endpoint should have summary comments
- DTO request body fields should remain documented
- Auth endpoints and protected endpoints should be testable
- If JWT auth is added, configure Swagger bearer token support

---

# 11. Implementation Order (Important)

Codex must implement Phase 2 in this order:

Step 1:
Introduce DTOs and refactor controllers to use DTOs

Step 2:
Add full update endpoints (PUT)

Step 3:
Introduce service layer and move business logic out of controllers

Step 4:
Add global exception middleware and logging

Step 5:
Add pagination/filtering/sorting

Step 6:
Add simple auth + JWT + basic authorization

Step 7:
Add real local file upload support for attachments

Do NOT attempt to generate the entire phase in one massive step.
Implement one step at a time, keeping the project buildable after each step.

---

# 12. Non-Goals for This Phase

Do NOT implement yet:
- frontend
- Azure deployment
- Blob Storage
- email notifications
- advanced approval workflows
- audit log
- unit test suite if time is limited
- CQRS or MediatR unless necessary

Keep the implementation practical and incremental.

---

# 13. Acceptance Criteria

The phase is complete when:

1. Controllers use DTOs instead of returning entities directly
2. Full update endpoints exist for Project/TaskItem/Variation
3. Business logic is moved into services
4. Global exception middleware is working
5. Logging is added
6. List endpoints support pagination/filtering/sorting
7. JWT login works and protected endpoints are secured
8. Attachment upload saves real local files and metadata
9. Swagger continues to work for all endpoints
10. dotnet build succeeds and app runs without breaking current features

---

# 14. First Task for Codex

Start with Step 1 only:

Refactor the existing controllers to use DTOs instead of returning EF entities directly.

Requirements:
- create DTO classes
- update ProjectController, TaskItemsController, VariationsController, AttachmentsController
- preserve current endpoint behavior
- keep build passing
- do not implement later steps yet