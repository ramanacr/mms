# Microsoft Meeting Scheduler (mms)

Multi-tenant meeting room scheduler with:
- **Backend**: ASP.NET Core Web API (.NET 10, EF Core, Microsoft Identity Web, Microsoft Graph)
- **Frontend**: Angular 21 + MSAL + PrimeNG + FullCalendar
- **Tests**: xUnit for core booking and recurrence logic

## Repository Structure

- `MeetingScheduler.Api/` - Backend API
- `MeetingScheduler.Client/` - Angular frontend
- `MeetingScheduler.Tests/` - Unit tests
- `MeetingScheduler.slnx` - Solution (API + Tests)

## Tech Stack

### Backend
- .NET `net10.0`
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- Microsoft Identity Web (JWT bearer auth)
- Microsoft Graph SDK
- Swagger/OpenAPI (Development)

### Frontend
- Angular `21.x`
- MSAL Angular / MSAL Browser
- PrimeNG
- FullCalendar

## Local Development Setup

## Prerequisites
- .NET SDK 10
- SQL Server LocalDB (or SQL Server)
- Node.js + npm (repo uses npm lockfile)
- Azure AD / Microsoft Entra app registration for API + SPA

## 1) Backend (API) - Development

From repo root:

```powershell
dotnet restore .\MeetingScheduler.slnx
dotnet build .\MeetingScheduler.slnx
```

Apply database migrations:

```powershell
dotnet ef database update --project .\MeetingScheduler.Api\MeetingScheduler.Api.csproj
```

Run API:

```powershell
dotnet run --project .\MeetingScheduler.Api\MeetingScheduler.Api.csproj
```

Default local URLs (`launchSettings.json`):
- `https://localhost:7087`
- `http://localhost:5182`

Swagger is enabled in `Development`.

## 2) Frontend (Client) - Development

```powershell
cd .\MeetingScheduler.Client
npm install
npm start
```

Default Angular dev URL:
- `http://localhost:4200`

## Backend Configuration

Backend configuration files:
- `MeetingScheduler.Api/appsettings.json` (base)
- `MeetingScheduler.Api/appsettings.Development.json` (development overrides)
- Optional: `appsettings.Production.json` for production deployment

Required sections:

### `ConnectionStrings`
- `DefaultConnection`: SQL Server connection string.

### `AzureAd`
- `Instance`
- `Domain`
- `TenantId`
- `ClientId`
- `Audience`

### `Graph`
- `BaseUrl`
- `Scopes` (currently uses `User.Read`, `Calendars.ReadWrite`, `People.Read`)

### `Cors`
- `AllowedOrigins`: frontend origins allowed to call API.

## Frontend Configuration

Frontend environment files:
- `MeetingScheduler.Client/src/environments/environment.ts` (development/base)
- `MeetingScheduler.Client/src/environments/environment.prod.ts` (production values)

Keys used by client:

### `apiBaseUrl`
- API base URL (for example, local dev uses `https://localhost:7087/api`).

### `auth`
- `clientId`
- `authority`
- `redirectUri`
- `postLogoutRedirectUri`
- `scopes`
- `apiScopes`

`environment.ts` is currently configured for local development.
`environment.prod.ts` contains production placeholders and should be updated before production builds.

## Development vs Production Configuration

## Backend

### Development
- Environment name: `ASPNETCORE_ENVIRONMENT=Development`
- Swagger UI enabled
- LocalDB default connection string
- CORS default includes `http://localhost:4200`

### Production
- Environment name: `ASPNETCORE_ENVIRONMENT=Production`
- Configure production SQL connection string
- Configure real Entra tenant/app values in `AzureAd`
- Configure production Microsoft Graph scopes and app permissions
- Set restrictive `Cors:AllowedOrigins` to your deployed frontend domain(s)
- Provide production settings via `appsettings.Production.json` and/or environment variables/secret store

## Frontend

### Development
- Start with `npm start` (`ng serve`, development configuration)
- Use dev redirect URIs and local API URL in `environment.ts`

### Production
- Build with:

```powershell
cd .\MeetingScheduler.Client
npm run build
```

- `ng build` defaults to production (`angular.json`)
- Set real production values in `environment.prod.ts`:
  - deployed API URL
  - production Entra client ID/authority
  - production redirect/logout URLs
  - production API scopes

## API Overview

Base route: `/api`

- `GET /api/profile/me`
- `GET /api/dashboard/stats`
- `GET /api/rooms`
- `POST /api/rooms` (OrgAdmin)
- `PUT /api/rooms/{id}` (OrgAdmin)
- `DELETE /api/rooms/{id}` (OrgAdmin)
- `GET /api/bookings?start=...&end=...`
- `POST /api/bookings`
- `DELETE /api/bookings/{id}?deleteSeries={bool}`
- `POST /api/tenants/admin-consent` (anonymous onboarding callback)

## Room Mailbox Booking

For Microsoft-native room booking, configure each room with an Exchange room mailbox in `ExchangeEmail`.
When a user schedules a meeting, MMS sends the selected room mailbox as a Microsoft Graph `resource` attendee and also sets the event location to the room name.

Rooms without `ExchangeEmail` can still be reserved locally in MMS, but they will not receive or process Microsoft room resource invites.

Authorization policies:
- `OrgAdmin`
- `OrgUser`

## Testing

Run backend tests:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj
```

Current test coverage includes:
- Booking conflict/adjacency behavior
- Recurrence expansion behavior

## Notes

- Frontend project exists in repository but is not included in `MeetingScheduler.slnx`.
- Backend tenant isolation is enforced through tenant claims + EF query filters.
