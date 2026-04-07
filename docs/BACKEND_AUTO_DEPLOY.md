# Backend Auto Deploy

This file explains how the VPS auto-deploy flow works for the backend.

## What It Does

When code is pushed to `main` and the push includes files under `backend/`, GitHub Actions will:

1. build and publish the ASP.NET Core backend
2. upload the published files to `/opt/construction-management`
3. restart the Linux service `construction-management`

Workflow file:

- `.github/workflows/deploy-backend.yml`

## GitHub Secrets You Must Add

Open GitHub repository settings:

- `Settings -> Secrets and variables -> Actions`

Add these repository secrets:

- `VPS_HOST`
  - value: `46.250.243.76`
- `VPS_USERNAME`
  - value: `root`
- `VPS_PASSWORD`
  - value: your current VPS root password
- `VPS_PORT`
  - value: `22`

## How To Use It

1. Commit your backend changes.
2. Push to `main`.
3. Open GitHub:
   - `Actions -> Deploy Backend To VPS`
4. Wait for the workflow to finish.
5. Verify:
   - `https://api.bulid.org/health`

## Important Notes

- Frontend is still deployed by Vercel.
- Backend auto-deploy only runs when files under `backend/` change.
- Changing VPS environment variables still requires editing:
  - `/etc/construction-management/app.env`
- If backend deploy succeeds but startup fails, inspect VPS logs:

```bash
journalctl -u construction-management -n 100 --no-pager
```

## Recommended Security Upgrade Later

This first version uses a VPS password stored in GitHub Secrets because it is the fastest path.

Later, upgrade this to:

- a dedicated deploy user
- SSH key authentication
- optional health-check rollback

