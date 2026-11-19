# SupplierReturns API

A REST API for managing suppliers and the goods you send back to them, from a draft return through to a supplier credit note. It is modelled on a real-world "return awaiting credit" process and built as an ASP.NET Core Web API with Entity Framework Core and SQLite.

## Features

- Supplier management (create, read, update, delete)
- Supplier returns with one or more lines (stock code, description, quantity, unit cost, reason)
- A three-step workflow: **Draft → AwaitingCredit → Credited**, with rules that block invalid changes
- Automatic return numbers (`RET-00001`, `RET-00002`, ...)
- Line totals and return totals calculated by the API
- Filtering returns by status and supplier
- Input validation and clear error responses (400, 404, 409)
- Interactive API documentation through Swagger UI
- Business rules covered by automated xUnit tests (in-memory SQLite)

## Tech stack

- C# / ASP.NET Core 10 (controller-based Web API)
- Entity Framework Core 10 with SQLite
- Code-first migrations
- OpenAPI + Swagger UI
- xUnit for unit tests

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The EF Core command-line tool:

```
dotnet tool install --global dotnet-ef
```

### Run it

```
git clone https://github.com/CraigWentzel/SupplierReturns.git
cd SupplierReturns/SupplierReturns.Api
dotnet ef database update
dotnet run
```

`dotnet ef database update` creates the SQLite database and tables. When the app starts, the terminal prints the address it is listening on (for example `http://localhost:5276`). Open that address and add `/swagger` to try the endpoints.

### Run the tests

From the repository root:

```
dotnet test
```

Stop the API first if it is running, because a running `SupplierReturns.Api.exe` locks the file and the build fails. The tests use an in-memory SQLite database, so they need no setup and leave no data behind.

## Endpoints

### Suppliers

| Method | Route | Description | Success | Errors |
|--------|-------|-------------|---------|--------|
| GET | `/api/Suppliers` | List all suppliers | 200 | |
| GET | `/api/Suppliers/{id}` | Get one supplier | 200 | 404 |
| POST | `/api/Suppliers` | Create a supplier | 201 | |
| PUT | `/api/Suppliers/{id}` | Update a supplier | 204 | 404 |
| DELETE | `/api/Suppliers/{id}` | Delete a supplier | 204 | 404 |

### Returns

| Method | Route | Description | Success | Errors |
|--------|-------|-------------|---------|--------|
| GET | `/api/Returns?status=&supplierId=` | List returns, optionally filtered | 200 | |
| GET | `/api/Returns/{id}` | Get one return with its lines | 200 | 404 |
| POST | `/api/Returns` | Create a return (starts as Draft) | 201 | 400 |
| POST | `/api/Returns/{id}/submit` | Move a Draft to AwaitingCredit | 200 | 404, 409 |
| POST | `/api/Returns/{id}/credit` | Record the credit note and move to Credited | 200 | 404, 409 |
| DELETE | `/api/Returns/{id}` | Delete a Draft return | 204 | 404, 409 |

## Return workflow

```
Draft  ──submit──▶  AwaitingCredit  ──credit──▶  Credited
```

- Only a **Draft** can be submitted or deleted.
- Only a return that is **AwaitingCredit** can be credited, and it needs a credit note number.
- Anything else returns **409 Conflict** with an explanation.

## Example

Create a return:

```
POST /api/Returns
```

```json
{
  "supplierId": 1,
  "lines": [
    {
      "stockCode": "A100",
      "description": "Widget",
      "quantity": 5,
      "unitCost": 12.50,
      "reason": "Damaged on delivery"
    }
  ]
}
```

Response (201):

```json
{
  "id": 1,
  "returnNumber": "RET-00001",
  "supplierId": 1,
  "supplierName": "Acme Supplies",
  "status": "Draft",
  "creditNoteNumber": null,
  "createdUtc": "2026-11-17T08:04:18Z",
  "total": 62.5,
  "lines": [
    {
      "id": 1,
      "stockCode": "A100",
      "description": "Widget",
      "quantity": 5,
      "unitCost": 12.5,
      "lineTotal": 62.5,
      "reason": "Damaged on delivery"
    }
  ]
}
```

Credit it once it has been submitted:

```
POST /api/Returns/1/credit
```

```json
{
  "creditNoteNumber": "CN-1001"
}
```

## Project structure

```
SupplierReturns/
├── SupplierReturns.slnx
├── SupplierReturns.Api/
│   ├── Controllers/   HTTP endpoints; thin, they only translate results to status codes
│   ├── Data/          AppDbContext and the data-layer classes (SupplierData, ReturnData)
│   ├── Dtos/          Request and response shapes, with validation attributes
│   ├── Models/        Database entities (Supplier, SupplierReturn, SupplierReturnLine)
│   └── Migrations/    EF Core migrations
└── SupplierReturns.Tests/   xUnit tests for the return workflow rules
```

## Design notes

- **Layering:** controllers call data-layer classes, and only those classes talk to EF Core. This keeps the controllers short and the business rules (the status workflow) in one place.
- **DTOs:** the API never exposes the entities directly. Requests are validated with data annotations, and responses are shaped for the client (for example `total` and `lineTotal` are calculated rather than stored).
- **Status as text:** the status is stored as a string in the database, so rows stay readable and reordering the enum later can't corrupt data.
- **Money and SQLite:** the SQLite provider stores `decimal` values as text, so it cannot sum or sort them inside the database query. The API loads the rows first and calculates totals in memory, which is fine at this scale. A move to SQL Server would remove the limitation.
- **Swagger and enums:** the status filter appears in Swagger UI as a number (0 = Draft, 1 = AwaitingCredit, 2 = Credited). The API also accepts the status names.
- **Dependency warning:** building shows NU1903 for the SQLite native library bundled with EF Core's SQLite provider. The fix depends on an upstream release, and the newer provider targets .NET 11. The API takes no raw SQL from users, and moving to SQL Server (see Roadmap) removes the dependency.

## Roadmap

Ideas for the next phases:

- Integration tests for the endpoints (the workflow rules already have unit tests)
- Authentication and authorisation
- Pagination and sorting on the list endpoints
- Editing the lines of a Draft return
- Switching to SQL Server

## Author

Craig Wentzel. Built while learning ASP.NET Core as a path from Web Forms.