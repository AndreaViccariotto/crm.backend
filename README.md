CRM API

Backend API for a personal CRM (Customer Relationship Management) project, built for learning and experimenting with modern .NET architecture.

Tech Stack

* .NET 8.0
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server (or specify your DB)
* JWT Authentication 

---

Getting Started

Prerequisites

* .NET SDK 8.0+
* SQL Server (or your configured database)

Check installation:

dotnet --version

---
Installation

Clone the repository:

git clone <your-repo-url>
cd <api-project-folder>

Restore dependencies:

dotnet restore

---
Configuration

Update your `appsettings.json`:

{
  "ConnectionStrings": {
    "DefaultConnection": "YourDatabaseConnectionString"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "your-app",
    "Audience": "your-app"
  }
}

---
Database Setup

Run migrations:

dotnet ef database update

If you need to create a migration:

dotnet ef migrations add InitialCreate

---

Run the API

Start the application:

dotnet run

Default endpoints:

https://localhost:5001
http://localhost:5000

Swagger UI (if enabled):

https://localhost:5001/swagger

---

Features

* RESTful API architecture
* CRUD operations for CRM entities
* Authentication & Authorization (JWT)
* Database persistence with EF Core
* Swagger documentation

---

Architecture

/CRM.Api/Controllers
/CRM.Application/Services
/CRM.Domain/Entities
/CRM.Infrastructure/Data

---

Status

Work in progress — features may change frequently.
