# AI Development Rules

This project is developed with AI assistance.

The following rules must be followed when generating code.

---

# 1. Architecture Rules

Backend must follow a clean layered structure.

Required folders:

Controllers  
Entities  
DTOs  
Services  
Data  
Middleware

Example:
backend/
Controllers
Entities
DTOs
Services
Data
Middleware
---

# 2. Entity Design Rules

All entities must include:

- Id (GUID)
- CreatedAt
- UpdatedAt

Use EF Core annotations or Fluent API.

---

# 3. Database Rules

Use Entity Framework Core.

Migrations must be used.

Never manually modify database schema.

Commands:
dotnet ef migrations add MigrationName
dotnet ef database update

---

# 4. API Design Rules

Use REST conventions.

Examples:

GET /api/projects  
GET /api/projects/{id}  
POST /api/projects  
PUT /api/projects/{id}  
DELETE /api/projects/{id}

---

# 5. Error Handling

Use global exception middleware.

Error response format:
{
code: "ERROR_CODE",
message: "error description"
}

---

# 6. Security Rules

Never store secrets in code.

Use:

- environment variables
- Azure configuration

Secrets include:

- database connection strings
- storage keys
- JWT secrets

---

# 7. Code Quality

Code must:

- compile successfully
- follow C# naming conventions
- include comments for complex logic

---

# 8. Task Size Rule

AI should only implement **one module at a time**.

Example modules:

- Projects
- Variations
- Tasks
- Attachments

Do NOT generate the entire system in one step.