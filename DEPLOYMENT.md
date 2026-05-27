# ByteBrigade Pharmacy — Free Deployment Guide

> **Backend** → Render (Docker, free)
> **Frontend** → Vercel (Static SPA, free)
> **Database** → Neon Postgres (free, no credit card)

Total cost: **$0**. Total time end-to-end: **~30 minutes**.

---

## 0. Before you start

### Prereqs

- A GitHub repo containing `backend/` and `frontend/`.
- Free accounts on:
  - <https://neon.tech> (no credit card)
  - <https://render.com> (no credit card)
  - <https://vercel.com> (no credit card)
- Locally:
  - .NET 10 SDK
  - Node LTS + Angular CLI: `npm i -g @angular/cli`
  - EF Core tools: `dotnet tool install --global dotnet-ef --version 10.0.8` (or `dotnet tool update --global dotnet-ef --version 10.0.8`)

### One-time prep: regenerate migrations for Postgres

The old migrations were SQL-Server-specific (`nvarchar(max)`, `bit`, `DBCC CHECKIDENT`). They've been deleted. You need to generate one fresh Postgres migration **once** before pushing:

```bash
cd backend/HackathonBackend

# Point at any local Postgres. Easiest: use the Neon connection string you'll
# create in step 1. Or run Docker locally:
#   docker run -d --name pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16

dotnet ef migrations add InitialCreate
```

You should see `Migrations/<timestamp>_InitialCreate.cs` appear. Commit it.

That's all the local setup. Now the cloud.

---

## 1. Database — Neon Postgres (free)

1. Sign in to <https://neon.tech>.
2. Create a new project → **Project name**: `bytebrigade-pharmacy`. Region: pick the one nearest your Render region (e.g. US East / EU West).
3. Neon shows a **Connection string** — copy it. It looks like:
   ```
   postgresql://USER:PASSWORD@ep-something.eu-west-1.aws.neon.tech/neondb?sslmode=require
   ```
4. **Important:** Neon's connection string format works as-is with Npgsql in .NET. You don't have to convert it.

That's it for the database. You don't need to create any tables — the backend runs `db.Database.Migrate()` on startup and creates them all on first cold boot.

---

## 2. Backend — Render (free Docker dyno)

1. Push your repo to GitHub (make sure `backend/HackathonBackend/Dockerfile`, `render.yaml`, and the new `Migrations/<timestamp>_InitialCreate.cs` are committed).
2. Sign in to <https://render.com>.
3. **New** → **Blueprint** → connect your GitHub repo. Render reads `backend/HackathonBackend/render.yaml` and creates a free Docker web service called `bytebrigade-api`.

   **Manual fallback** if you prefer:
   - **New** → **Web Service** → **Docker** → connect repo.
   - Root directory: `backend/HackathonBackend`.
   - Health check path: `/openapi/v1.json`.

4. **Environment** tab → add three secrets:

   | Key | Value |
   |---|---|
   | `JWT__KEY` | A long random string, 32+ chars. Generate with `openssl rand -base64 48` or use a password manager. |
   | `DEFAULT_CONNECTION` | The Neon connection string from step 1, with `?sslmode=require` kept. |
   | `CORS__ALLOWEDORIGINS` | Leave empty for the first deploy — you'll set it after Vercel gives you a URL. |

5. **Deploy**. The first build takes ~5 minutes (downloads .NET SDK + builds image).
6. Open the live URL Render gives you, e.g. `https://bytebrigade-api.onrender.com`.
7. Test:
   - `https://bytebrigade-api.onrender.com/openapi/v1.json` → returns OpenAPI JSON
   - `https://bytebrigade-api.onrender.com/swagger` → Scalar UI
   - `https://bytebrigade-api.onrender.com/api/Medicine` → returns 50 seeded medicines

> **What happens on first boot:**
> 1. Container starts and reads `DEFAULT_CONNECTION` from env.
> 2. `db.Database.Migrate()` runs your `InitialCreate` migration against the empty Neon DB.
> 3. The seed data in `AppDbContext.OnModelCreating` is applied: 1 admin user, 2 promo codes, 50 medicines.

> **Free tier caveat:** Render's free Docker dyno sleeps after 15 min idle. First request after sleep takes ~30 s. That's normal.

---

## 3. Frontend — Vercel (free SPA)

1. Open `frontend/hackathon-frontend/src/environments/environment.prod.ts` and replace `YOUR-RENDER-SERVICE.onrender.com` with your actual Render URL. Commit + push.
2. Sign in to <https://vercel.com>.
3. **Add New** → **Project** → import your GitHub repo.
4. Configure:
   - Root directory: `frontend/hackathon-frontend`
   - Build / output settings are auto-detected from `vercel.json`:
     - Build command: `ng build --configuration production`
     - Output directory: `dist/hackathon-frontend/browser`
   - Install command: `npm install`
5. **Deploy**. First build ~3 minutes.
6. Vercel gives you a URL, e.g. `https://bytebrigade-pharmacy.vercel.app`.

### Wire CORS back

1. Render dashboard → `bytebrigade-api` → **Environment**.
2. Set `CORS__ALLOWEDORIGINS` to your Vercel URL (no trailing slash). Add multiple comma-separated if you have preview URLs too:
   ```
   https://bytebrigade-pharmacy.vercel.app
   ```
3. Render redeploys automatically.

---

## 4. Test the full flow

1. Open your Vercel URL.
2. Click **Register**, create a new customer account. The strong-password validator should accept e.g. `Demo@1234`.
3. Login. You'll be sent to `/medicines` and see all 50 seeded items.
4. Add a few medicines to cart. Try one with an Rx badge — the cart will demand a JPG/PNG upload.
5. Purchase from cart → toast `Order confirmed — waiting for validation` → routes to `/orders`.
6. Logout, then login as `admin` / `admin123` (seeded). You'll land on `/dashboard`. Visit `/admin-orders` → approve or reject the order via the modal.

---

## 5. Common pitfalls

| Symptom | Cause | Fix |
|---|---|---|
| Frontend shows `ERR_CONNECTION_REFUSED` on every API call | `environment.prod.ts` still has the `YOUR-RENDER-SERVICE` placeholder | Edit it to your Render URL, commit, redeploy on Vercel |
| Frontend shows `CORS error` | `CORS__ALLOWEDORIGINS` not set on Render or wrong URL | Add the exact Vercel origin (with `https://`, no trailing slash) |
| `dotnet ef migrations add` fails: `Unable to resolve service for type 'AppDbContext'` | EF tools can't connect to Postgres yet | Either spin up local Postgres via Docker (`docker run -d -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16`) or temporarily paste the Neon connection string into `appsettings.json` before running the command |
| Render build fails at `dotnet publish` | EF Core tool version mismatch | Locally run `dotnet tool update --global dotnet-ef --version 10.0.8`, regenerate the migration, push |
| Render container restarts in a loop | Connection string wrong, or Neon DB sleeping | Check Render logs → look for `Npgsql` errors. Verify `DEFAULT_CONNECTION` value. Visit your Neon project once to wake it. |
| Login fails with 500 | `JWT__KEY` env var missing | Set it on Render → redeploy |
| `Could not load your cart` toast | Backend container is asleep (free dyno) | Wait 30 s for cold start, retry |
| Prescriptions disappear between deploys | Render free dyno filesystem is ephemeral | Acceptable for demos. For real persistence, attach a Render Disk on a paid plan or move to Cloudflare R2 / Supabase Storage |

---

## 6. Local development still works

```bash
# Backend (port 5020)
cd backend/HackathonBackend
dotnet run

# Frontend (port 4200)
cd frontend/hackathon-frontend
ng serve
```

For local dev you have two options:

- **Use Neon for local too** — simplest. Just keep the Neon connection string in `appsettings.json`. Free tier allows many concurrent connections.
- **Run Postgres locally** — `docker run -d --name pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16`. The default `appsettings.json` already points at this.

When `ng build --configuration production` runs, Angular swaps `environment.ts` for `environment.prod.ts` (via `fileReplacements` in `angular.json`), so local dev always hits `localhost:5020` and production always hits your Render URL.

---

## 7. URLs to bookmark

| What | URL |
|---|---|
| Frontend (Vercel) | `https://<your-project>.vercel.app` |
| Backend root | `https://bytebrigade-api.onrender.com` |
| OpenAPI JSON | `https://bytebrigade-api.onrender.com/openapi/v1.json` |
| Scalar UI | `https://bytebrigade-api.onrender.com/swagger` |
| Neon dashboard | <https://console.neon.tech> |
| Render dashboard | <https://dashboard.render.com> |
| Vercel dashboard | <https://vercel.com/dashboard> |

---

## 8. What changed in the codebase for Postgres

| File | Change |
|---|---|
| `backend/HackathonBackend/HackathonBackend.csproj` | Removed `Microsoft.EntityFrameworkCore.SqlServer`. Added `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4. |
| `backend/HackathonBackend/Program.cs` | `options.UseSqlServer(...)` → `options.UseNpgsql(...)`. |
| `backend/HackathonBackend/appsettings.json` | Default `DefaultConnection` now points at a local Postgres on port 5432. |
| `backend/HackathonBackend/Migrations/` | **Deleted.** SQL-Server-specific. Regenerate once with `dotnet ef migrations add InitialCreate` (see step 0). |

Everything else — controllers, models, services, frontend code, env config — is provider-agnostic and unchanged.
