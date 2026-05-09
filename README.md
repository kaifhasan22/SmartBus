# SmartBus API — Setup & Reference Guide

## Tech Stack
- ASP.NET Core 8 Web API
- Entity Framework Core 8 (ORM)
- SQL Server (database)
- JWT Bearer Authentication
- BCrypt password hashing
- Swagger UI (auto-generated docs)

---

## Step 1 — Prerequisites

Install these before starting:
- Visual Studio 2022 (with ASP.NET workload)
- SQL Server (LocalDB included with VS is fine)
- .NET 8 SDK

---

## Step 2 — Open the Project

1. Open Visual Studio 2022
2. File → Open → Folder → select the `SmartBusAPI` folder
3. VS will detect the `.csproj` automatically

---

## Step 3 — Configure the Connection String

Open `appsettings.json` and update if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SmartBusDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Common server names to try:
- `localhost`
- `.\SQLEXPRESS`
- `(localdb)\MSSQLLocalDB`

---

## Step 4 — Run EF Core Migrations

Open Package Manager Console (Tools → NuGet → PMC) and run:

```
Add-Migration InitialCreate
Update-Database
```

This creates all 7 tables in SQL Server automatically.

---

## Step 5 — Run the Project

Press F5. The browser opens at `http://localhost:5000` showing Swagger UI.

The DataSeeder runs automatically on first launch and creates:
- admin@college.edu   / Admin@123
- mohan@college.edu   / Driver@123
- suresh@college.edu  / Driver@123
- ramesh@college.edu  / Driver@123
- priya@student.edu   / Student@123
- arjun@student.edu   / Student@123

---

## Full API Reference

### AUTH
| Method | Endpoint              | Auth     | Body                        |
|--------|-----------------------|----------|-----------------------------|
| POST   | /api/auth/login       | None     | { Email, Password }         |
| POST   | /api/auth/register    | Admin    | { Name, Email, Password, Role } |
| POST   | /api/auth/fcm-token   | Any      | { FCMToken }                |
| GET    | /api/auth/me          | Any      | —                           |

### BUSES
| Method | Endpoint                    | Auth      |
|--------|-----------------------------|-----------|
| GET    | /api/buses                  | Any       |
| GET    | /api/buses/{id}             | Any       |
| GET    | /api/buses/{id}/location    | Any       |  ← Student polls this every 5s
| GET    | /api/buses/locations/all    | Any       |  ← Admin live map
| POST   | /api/buses                  | Admin     |
| PUT    | /api/buses/{id}             | Admin     |
| DELETE | /api/buses/{id}             | Admin     |

### ROUTES
| Method | Endpoint                    | Auth  |
|--------|-----------------------------|-------|
| GET    | /api/routes                 | Any   |
| GET    | /api/routes/{id}            | Any   |
| GET    | /api/routes/{id}/stops      | Any   |
| POST   | /api/routes                 | Admin |
| POST   | /api/routes/{id}/stops      | Admin |
| DELETE | /api/routes/{id}            | Admin |
| DELETE | /api/routes/stops/{stopId}  | Admin |

### LOCATION (Driver App → every 5 seconds)
| Method | Endpoint              | Auth   | Body                                          |
|--------|-----------------------|--------|-----------------------------------------------|
| POST   | /api/location/update  | Driver | { BusId, TripId, Latitude, Longitude, SpeedKmph } |

### TRIPS
| Method | Endpoint            | Auth   | Body                  |
|--------|---------------------|--------|-----------------------|
| GET    | /api/trips          | Admin  | —                     |
| GET    | /api/trips/active   | Any    | —                     |
| GET    | /api/trips/my       | Driver | —                     |
| GET    | /api/trips/{id}     | Any    | —                     |
| POST   | /api/trips/start    | Driver | { BusId, RouteId }    |
| POST   | /api/trips/{id}/end | Driver | { Notes? }            |

### NOTIFICATIONS
| Method | Endpoint                        | Auth   |
|--------|---------------------------------|--------|
| GET    | /api/notifications/my           | Any    |
| POST   | /api/notifications/send         | Admin  |
| POST   | /api/notifications/broadcast    | Admin  |
| PUT    | /api/notifications/{id}/read    | Any    |
| PUT    | /api/notifications/read-all     | Any    |

### REPORTS (Data Mining — Admin only)
| Method | Endpoint                       | Query Params  |
|--------|--------------------------------|---------------|
| GET    | /api/reports/dashboard         | —             |
| GET    | /api/reports/delays            | ?days=7       |
| GET    | /api/reports/peak-hours        | ?days=30      |
| GET    | /api/reports/route-efficiency  | —             |
| GET    | /api/reports/weekly-trend      | —             |

### USERS
| Method | Endpoint                    | Auth  |
|--------|-----------------------------|-------|
| GET    | /api/users                  | Admin |
| GET    | /api/users/drivers          | Admin |
| GET    | /api/users/me               | Any   |
| PUT    | /api/users/{id}/deactivate  | Admin |
| PUT    | /api/users/{id}/activate    | Admin |

---

## How Android App Connects

In Android Studio, set the base URL in your Retrofit client:

```java
// For emulator connecting to your PC's localhost:
private static final String BASE_URL = "http://10.0.2.2:5000/api/";

// For real device on same WiFi:
private static final String BASE_URL = "http://YOUR_PC_IP:5000/api/";
```

Find your PC IP: open CMD → type `ipconfig` → use IPv4 Address.

---

## Workflow Summary

1. Driver logs in → POST /api/auth/login → gets JWT token
2. Driver taps "Start Trip" → POST /api/trips/start → gets tripId
3. Driver app starts LocationService → POST /api/location/update every 5s
4. Student logs in → GET /api/trips/active → sees which bus is running
5. Student taps bus → GET /api/buses/{id}/location every 5s → shows moving pin on map
6. Driver taps "End Trip" → POST /api/trips/{id}/end → trip recorded with delay
7. Admin opens dashboard → GET /api/reports/dashboard → sees KPIs
8. Admin views mining → GET /api/reports/delays, /peak-hours, /route-efficiency
