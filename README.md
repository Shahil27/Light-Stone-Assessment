Orders & Inventory Service (Light Stone Assessment)

Local run instructions

Prerequisites:
- .NET 10 SDK
- SQL Server LocalDB (installed with Visual Studio) or any SQL Server; update connection string in appsettings.json

Run locally:

1. Restore and build

   dotnet restore
   dotnet build

2. Run

   dotnet run --project "Light Stone Assessment/Light Stone Assessment.csproj"

The app will create a local database (EnsureCreated) using the connection string in appsettings.json and seed sample products.

Endpoints (examples):

- Health: GET /health
- Create product: POST /api/products
  Body: { "sku": "SKU-010", "name": "Item", "price": 12.50, "initialStock": 10 }
- Adjust stock: PATCH /api/products/{sku}/stock
  Body: { "delta": -2 }
- Submit order: POST /api/orders
  Body: { "externalOrderId": "ext-10001", "placedAt": "2025-05-15T12:34:56Z", "items": [ { "sku": "SKU-001", "qty": 2, "unitPrice": 24.99 } ] }
- Sales summary: GET /api/sales?start=2025-05-01&end=2025-05-07
- In Development the app registers OpenAPI and Swagger UI. Try:
	- OpenAPI JSON (common paths): https://localhost:7041/openapi  (also checks /openapi?format=json, /openapi/v1, /openapi.json)
  - Swagger UI: https://localhost:7041/swagger

Notes
- Logging and connection string are configured via appsettings.json. Change levels and connection string without code changes.
- For demo simplicity the project uses EnsureCreated (no EF migrations). For production, use migrations and robust deployment patterns.
