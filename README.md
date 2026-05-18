# HR PeopleOS Backend

.NET 9 Web API for the first PeopleOS lifecycle-management build.

## Scope

Included:

- Demo login
- Employee directory and profile data
- Hire-to-retire lifecycle tracker
- Attendance records and correction queue
- Leave balances and leave requests
- Benefits, mobility, and expense categories
- Expense claims including Medical OPD
- Resignation requests and offboarding queue
- Profile language and photo update support
- Policies and downloads
- Line-manager approval queue

Excluded by product decision:

- Payroll
- Tax
- Travel management
- Help desk tickets

## Run

```powershell
dotnet restore
dotnet ef database update
dotnet run --urls http://localhost:5265
```

The API uses SQL Server LocalDB by default:

`Server=(localdb)\MSSQLLocalDB;Database=PeopleOS_HR;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`

On startup it applies pending migrations and inserts demo data when the database is empty.

## QA

```powershell
dotnet test PeopleOS.Backend.sln
```

## Demo Accounts

- `admin@peopleos.dev` / `Admin@123`
- `hr@peopleos.dev` / `Hr@123`
- `employee@peopleos.dev` / `Employee@123`
