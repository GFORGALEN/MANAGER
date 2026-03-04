# Variation State Machine

This document defines the allowed state transitions for variations.

The system must strictly enforce these rules.

---

# States

Draft  
Submitted  
Approved  
Rejected  
NeedInfo

---

# Allowed Transitions

Draft → Submitted

Submitted → Approved  
Submitted → Rejected  
Submitted → NeedInfo

NeedInfo → Submitted

---

# Forbidden Transitions

Draft → Approved  
Draft → Rejected  

Approved → Draft  
Approved → Submitted  

Rejected → Approved

---

# Error Handling

If a forbidden transition is attempted:

Return HTTP status:

409 Conflict

Example response:

{
code: "INVALID_STATE_TRANSITION",
message: "Cannot change variation status from Draft to Approved"
}

---

# Business Logic Notes

Only Admin can:

- Approve variations
- Reject variations
- Request more information

PM can:

- Create Draft
- Submit variations
- Update Draft or NeedInfo variations