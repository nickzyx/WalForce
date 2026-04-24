# WalForce

## Backend Overview

The backend lives in `Backend/WebServer`. It is an ASP.NET Core minimal API server with:

- PostgreSQL-backed repositories in Development
- JSON-backed fallback repositories when no database connection string is configured
- Employee endpoints for profile, schedule, and weekly availability
- Manager endpoints for roster and team schedule
- Swagger UI at `/swagger`

## Current Database

Local Development uses PostgreSQL through `Backend/WebServer/appsettings.Development.json`:

```json
"Database": {
  "ConnectionString": "Host=localhost;Port=5432;Database=walforce;Username=postgres;Password=postgrespwd"
}
```

The active schema is:

- `public.employees`
- `public.logins`
- `public.availability`
- `public.shifts`

Schedule endpoints read from `public.shifts`. Availability endpoints read and write `public.availability`.

Database files are under `Backend/Database`:

- `dump-walforce-202604241135.sql`: original backup
- `dump-walforce-202604241247.sql`: current backup with the `shifts` table
- `20260424-create-and-seed-shifts.sql`: script that creates and seeds `public.shifts`

## Setup

### Prerequisites

- .NET 10 SDK
- PostgreSQL running on `localhost:5432`

### Restore and Build

From the repo root:

```powershell
dotnet restore Backend/WebServer/WebServer.csproj
dotnet build Backend/WebServer/WebServer.csproj
```

### Run the Backend

```powershell
dotnet run --project Backend/WebServer --launch-profile http
```

The default local URL is:

```text
http://localhost:5041
```

The `http` launch profile sets `ASPNETCORE_ENVIRONMENT=Development`, so the server automatically reads `appsettings.Development.json` and connects to PostgreSQL.

## Swagger

With the backend running, open:

```text
http://localhost:5041/swagger
```

### Login

Expand `POST /api/auth/login`, click `Try it out`, and use one of these request bodies.

Manager:

```json
{
  "email": "ava.diaz@walforce.local",
  "password": "WalForce!123"
}
```

Employee:

```json
{
  "email": "liam.nguyen@walforce.local",
  "password": "WalForce!123"
}
```

Copy the `accessToken` value from the response.

### Authorize Swagger

1. Click `Authorize`.
2. Paste only the raw token into the `Bearer` field.
3. Do not include the `Bearer ` prefix.
4. Click `Authorize`.
5. Close the dialog.

### Employee Flow

Log in as `liam.nguyen@walforce.local`, authorize Swagger, then call:

- `GET /api/me/profile`
- `GET /api/me/schedule?from=2026-04-13&to=2026-04-26`
- `GET /api/me/availability`
- `PUT /api/me/availability`

Expected schedule result: Liam has 6 seeded shifts from `public.shifts`.

### Manager Flow

Log in as `ava.diaz@walforce.local`, authorize Swagger, then call:

- `GET /api/manager/roster`
- `GET /api/manager/schedule?from=2026-04-13&to=2026-04-26`

Expected manager data:

- Roster returns Liam Nguyen and Zoe Patel.
- Manager schedule returns 12 employee-role shifts from `public.shifts`.

## Seed Credentials

All seeded PostgreSQL users use the password `WalForce!123`.

- `ava.diaz@walforce.local`: Manager
- `liam.nguyen@walforce.local`: Employee
- `zoe.patel@walforce.local`: Employee

The JSON fallback data also includes `manager@walforce.local`, but Development should use PostgreSQL.

## Notes

- CORS is configured for `http://localhost:3000`.
- `appsettings.json` has an empty database connection string for fallback behavior.
- `appsettings.Development.json` is configured for the local PostgreSQL server.
