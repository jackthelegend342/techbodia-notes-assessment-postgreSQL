# Techbodia Notes (PostgreSQL edition)

A full-stack notes application: Vue 3 + TypeScript + Tailwind CSS frontend,
ASP.NET Core 8 Web API backend (Dapper + PostgreSQL), with JWT-based
authentication and per-user data isolation.

> This is the PostgreSQL variant of the project. A SQL Server variant
> (matching the assessment's stated tech stack) lives in a separate repo.

## Features

- Register / sign in with email + password (BCrypt-hashed, JWT sessions)
- Create, view, edit, and delete notes — click anywhere on a note card to
  open it for editing
- Pin notes to keep them at the top of the list
- Live search across title and content
- Sort by newest, oldest, or alphabetical
- Each note shows its created date and last-edited date
- Users can only ever see and modify their own notes — enforced at the
  database query level, not just in the UI
- Responsive, mobile-first layout

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | Vue 3 (`<script setup>`), TypeScript, Tailwind CSS, Axios |
| Backend | C# / .NET 8, ASP.NET Core Web API |
| Data access | Dapper (raw parameterized SQL, no ORM) |
| Database | PostgreSQL |
| Auth | JWT Bearer tokens, BCrypt password hashing |

## Structure

```
├── backend/
│   ├── init.sql                       # PostgreSQL schema
│   └── NotesApp.Api/
│       ├── Controllers/               # AuthController, NotesController
│       ├── Data/                      # DbConnectionFactory (Npgsql)
│       ├── Middleware/                # Global exception handling
│       ├── Models/                    # User, Note, DTOs/requests
│       ├── Repositories/              # Dapper raw-SQL data access
│       ├── Services/                  # AuthService (JWT + BCrypt)
│       ├── Program.cs
│       ├── appsettings.json
│       └── NotesApp.Api.csproj
└── frontend/
    ├── src/
    │   ├── api/                       # axiosInstance, auth.ts, notes.ts
    │   ├── components/                # LoginRegister.vue, NotesDashboard.vue
    │   ├── types/                     # Shared TS interfaces
    │   ├── App.vue
    │   ├── main.ts
    │   └── style.css
    ├── index.html
    ├── package.json
    ├── vite.config.ts
    ├── tailwind.config.js
    ├── postcss.config.js
    └── tsconfig.json
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (pgAdmin works well for managing it)
- [Node.js](https://nodejs.org/) (includes npm)

### 1. Create the database and run the schema

Using `psql`:
```bash
createdb techbodia
psql -d techbodia -f backend/init.sql
```

Or in pgAdmin: create a `techbodia` database, open the Query Tool against
it, paste in `backend/init.sql`, and execute.

### 2. Configure the backend

Edit `backend/NotesApp.Api/appsettings.json` directly for local testing, or
use `dotnet user-secrets` (recommended, keeps secrets out of source control):

```bash
cd backend/NotesApp.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=techbodia;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Secret" "some-long-random-32-plus-byte-string"
```

### 3. Run the backend

```bash
dotnet restore
dotnet run
```

The API listens on the URL printed in the console (typically
`http://localhost:5000`). Swagger UI is available at `/swagger` in
Development.

### 4. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173` in your browser. Optionally point the frontend
at a non-default API URL:
```bash
echo "VITE_API_BASE_URL=http://localhost:5000/api" > .env.local
```

## Security notes

- Passwords are hashed with BCrypt (work factor 12) — never stored or logged
  in plaintext.
- Every note query in `NoteRepository` filters explicitly by
  `user_id = @UserId`, where `@UserId` is derived only from the validated
  JWT `sub` claim in `NotesController` — never from client input — so one
  user can never read or modify another user's notes.
- JWTs are signed with HMAC-SHA256; issuer, audience, lifetime, and signing
  key are all validated on every request.
- Unhandled exceptions are caught by `ExceptionHandlingMiddleware` and
  returned as `ProblemDetails` JSON — stack traces are logged server-side
  only, never sent to the client.
