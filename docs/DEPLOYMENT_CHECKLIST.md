# Deployment Checklist

## Replace Before Launch

- `backend/ConstructionManagement/appsettings.Development.json`
  - Do not keep real production keys here.
  - Use this file only for local development.

- `ConnectionStrings:Default`
  - Replace local SQL Server credentials with the real deployment database connection string.

- `Jwt:SecretKey`
  - Replace with a long random secret.
  - Do not reuse the placeholder value.

- `Email:Enabled`
  - Set to `true` only when email is fully configured.

- `Email:Provider`
  - Keep `Resend` only if you are actually using Resend.
  - Otherwise switch to the SMTP provider path and fill host credentials.

- `Email:ApiKey`
  - Required for Resend.
  - Must be stored in deployment environment variables or a secure secret store.

- `Email:FromAddress`
  - Must be a verified sender address for your email provider.

- `Sms:Enabled`
  - Keep `false` unless Twilio is fully configured.

- `Sms:AccountSid`
  - Required only if SMS is enabled.

- `Sms:AuthToken`
  - Required only if SMS is enabled.

- `Sms:FromNumber`
  - Required only if SMS is enabled.

## Recommended Deployment Split

- Frontend
  - Only configure `VITE_API_BASE_URL` if the frontend needs a different API address in production.
  - Do not put backend secrets in frontend `.env` files.

- Backend
  - Store database, JWT, email, and SMS secrets in deployment platform environment variables.
  - Environment variable naming should follow ASP.NET Core conventions, for example:
    - `ConnectionStrings__Default`
    - `Jwt__SecretKey`
    - `Email__ApiKey`
    - `Email__FromAddress`
    - `Sms__AuthToken`

## Before You Go Live

- Confirm the backend starts successfully with production configuration.
- Confirm login works for:
  - Admin
  - PM
  - Contractor
- Confirm file upload works.
- Confirm worker task status updates work.
- Confirm email notifications work with a real recipient.
- Confirm delete-all dangerous actions are admin-only.
- Confirm CORS allows the real frontend domain.
- Confirm HTTPS is enabled on the deployed site.

## Immediate Security Follow-Up

- Rotate any key that was previously committed to source control.
- Remove old leaked Resend or other provider keys from your provider dashboard.
