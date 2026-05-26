# BierDex

Quickstart — First-time setup

Prerequisites
- .NET 10 SDK installed (verify with `dotnet --version`).
- PostgreSQL server accessible (local or hosted). Ensure a database and user exist.
- (Optional) Visual Studio 2022/2026 or VS Code for development.

1) Clone the repository

	git clone https://github.com/Magierige/BierDex.git
	cd BierDex

2) Configure secrets / connection strings

The API project BierDex uses a PostgreSQL connection string named `localhost` by default (see BierDex/appsettings.json).

You can either edit BierDex/appsettings.json directly or store secrets with the user-secrets tool (recommended for local development):

	cd BierDex
	dotnet user-secrets init
	dotnet user-secrets set "ConnectionStrings:localhost" "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=yourpassword"
	dotnet user-secrets set "smtpCredentials:host" "smtp.example.com"
	dotnet user-secrets set "smtpCredentials:username" "smtp-user"
	dotnet user-secrets set "smtpCredentials:password" "smtp-pass"
	dotnet user-secrets set "smtpCredentials:port" "587"
	dotnet user-secrets set "smtpCredentials:linkSpa" "https://localhost:5001"

If you prefer, replace the values in BierDex/appsettings.json instead.

3) Apply Entity Framework migrations and create the database

Ensure the `dotnet-ef` tool is available:

	dotnet tool install --global dotnet-ef

From the repository root run:

	dotnet ef database update --project BierDex --startup-project BierDex

This will apply the existing migrations and create tables. The project includes seeders that create initial roles and users.

4) Run the API and the SPA

API (backend):

	dotnet run --project BierDex

SPA (frontend static server / reverse proxy):

	dotnet run --project BierDexSpa

You may also open the solution in Visual Studio and run the projects.

5) Seeded accounts (for local testing)

IdentitySeeder creates these accounts (passwords shown for development only):

- Admin:  email: admin@test.com    password: Password123!
- Supplier: email: supplier@test.com password: Password123!
- Supplier2: email: supplier2@test.com password: Password123!
- User: email: user@test.com      password: Password123!

6) SMTP

Set `smtpCredentials` entries (host, port, username, password, linkSpa) either in appsettings.json or in user-secrets. Without valid SMTP settings some email flows (verification, password reset) will not work.

Troubleshooting
- If you get database errors about duplicate primary keys after reseeding, check that migrations were applied and that the database sequences are aligned with the table data. You can reset migrations or recreate the database for a clean start.
- Verify the connection string and that PostgreSQL accepts connections from your host/port.
- Check the application logs for detailed errors.

Additional notes
- The API project is BierDex and the SPA proxy project is BierDexSpa. The SPA serves static files and proxies API calls; you can host them together or separately during development.
- Keep secrets out of source control.

If you need help with a specific error, open an issue or provide the error details and I will assist further.
