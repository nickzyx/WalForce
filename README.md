# WalForce

## Backend Overview

The backend lives in `Backend/WebServer`. It is a minimal API server with:

- JSON-backed repositories under `Backend/WebServer/Data`
- Bearer-token auth with seeded employee and manager users
- Employee endpoints for login, profile, schedule, and weekly availability
- Manager endpoints for login, roster, and team schedule
- Swagger UI at `/swagger`

## Developer Setup

### Prerequisites

- .NET 10 SDK

### Setup

From the repo root:

```powershell
dotnet restore Backend/WebServer/WebServer.csproj
```

### Configure local development settings

`Backend/WebServer/appsettings.json` is the checked-in example file. Put real local values in `Backend/WebServer/appsettings.Development.json`, which is ignored by git.

Use this shape:

```json
{
  "Auth": {
    "SigningKey": "replace-with-a-long-random-development-key"
  }
}
```

### Run the backend

```powershell
dotnet run --project Backend/WebServer --launch-profile http
```

The default local URL is `http://localhost:5041`.

### How to use Swagger

With the backend running, open:

- `http://localhost:5041/swagger/index.html`

Swagger is a browser UI for testing API endpoints without writing code. You can expand an endpoint, fill in inputs, send the request, and see the response.

#### What you will see

- A list of endpoint groups such as `Auth`, `Me`, and `Manager`
- A small arrow next to each endpoint that expands or collapses it
- A `Try it out` button on each endpoint
- An `Authorize` button near the top-right of the page

#### First step: log in and get a token

1. Expand `POST /api/auth/login`
2. Click `Try it out`
3. Replace the request body with one of the seed users below
4. Click `Execute`
5. In the response body, copy the value of `accessToken`

Example employee login body:

```json
{
  "email": "ava.diaz@walforce.local",
  "password": "WalForce!123"
}
```

Example manager login body:

```json
{
  "email": "manager@walforce.local",
  "password": "WalForce!123"
}
```

#### Second step: authorize Swagger with the token

1. Click `Authorize` near the top-right of the page
2. Paste only the raw token into the `Bearer` field
3. Do not type `Bearer ` before the token
4. Click `Authorize`
5. Close the dialog

After this, Swagger will send your token automatically on protected requests.

#### Third step: call protected endpoints

For employee testing:

1. Expand `GET /api/me/profile`
2. Click `Try it out`
3. Click `Execute`
4. Confirm the response shows the logged-in employee

Then try:

- `GET /api/me/schedule`
- `GET /api/me/availability`
- `PUT /api/me/availability`

For the schedule endpoints, enter dates like:

- `from`: `2026-04-13`
- `to`: `2026-04-20`

For manager testing:

1. Log in as `manager@walforce.local`
2. Authorize Swagger with the manager token
3. Try:
   - `GET /api/manager/roster`
   - `GET /api/manager/schedule`

#### Common mistakes

- `401 Unauthorized` on `POST /api/auth/login`:
  The email or password in the request body is wrong.
- `401 Unauthorized` on employee or manager endpoints:
  You are not authorized in Swagger yet, or the token was pasted incorrectly.
- `403 Forbidden` on employee or manager endpoints:
  The token is valid, but you are using the wrong role. For example, an employee token cannot call manager endpoints.
- Nothing changes after a new login:
  Click `Authorize` again and replace the old token with the new one.

#### Good beginner test flow

1. Log in as `ava.diaz@walforce.local`
2. Authorize Swagger with the returned token
3. Call `GET /api/me/profile`
4. Call `GET /api/me/schedule`
5. Call `GET /api/me/availability`
6. Log in as `manager@walforce.local`
7. Authorize Swagger with the new manager token
8. Call `GET /api/manager/roster`
9. Call `GET /api/manager/schedule`

### Seed credentials

All seeded users use the password `WalForce!123`.

- `manager@walforce.local`
- `ava.diaz@walforce.local`
- `liam.nguyen@walforce.local`
- `zoe.patel@walforce.local`

## Notes

- CORS is configured for `http://localhost:3000` in development settings.
- The current prototype persists availability changes back to the JSON data files under `Backend/WebServer/Data`.
