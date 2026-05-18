# HR PeopleOS Backend

.NET 9 Web API for the first PeopleOS lifecycle-management build.

## Scope

Included:

- Demo login
- Employee directory and profile data
- Hire-to-retire lifecycle tracker
- Attendance records and correction queue
- Leave balances and leave requests
- Benefits administration
- Policies and downloads
- Approval queue

Excluded by product decision:

- Payroll
- Tax
- Travel management
- Help desk tickets

## Run

```powershell
dotnet restore
dotnet run --urls http://localhost:5265
```

## Demo Accounts

- `admin@peopleos.dev` / `Admin@123`
- `hr@peopleos.dev` / `Hr@123`
- `employee@peopleos.dev` / `Employee@123`

The current build uses Entity Framework Core with an in-memory database and seeded dummy data. It is intentionally fast for product iteration and can be moved to SQL Server when the domain model settles.
