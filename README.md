# PPSR Registration Upload System

This project implements a full-stack solution for batch uploading and processing of motor vehicle PPSR registrations via CSV files.

## Features

### Backend (.NET 8, EF Core, PostgreSQL)
- RESTful API for uploading CSV files
- Stream-based ingestion for memory efficiency
- Data validation and normalization:
  - Flexible date parsing (e.g., `24/02/2025`, `2025-02-24`)
  - VIN normalization (trimmed, uppercase)
  - ACN normalization (whitespace removed)
- Updates existing records if VIN + Grantor + ACN already exists
- Fully containerized using Docker Compose
- Unit tests using xUnit and Moq

### Frontend (Vite + React + TypeScript + TailwindCSS)
- CSV file upload form with validations
- Displays full upload summary: total, added, updated, invalid
- Shows warnings (e.g., normalized values, missing data)
- Smooth scroll and UI transitions

## Architecture
The backend adopts a **microservice-inspired architecture**, even though currently implemented as a single service. It follows a clean, modular project layout:
```
PPSR.Registration/
└── services/                  
    ├── PPSR.Registration.Domain      
    ├── PPSR.Registration.Application
    ├── PPSR.Registration.Infrastructure
    └── PPSR.Registration.WebApi.Ingestor
```
This separation of concerns makes it **highly extensible**, allowing new services (e.g., audit logging, analytics) to be added easily in the future.

> Example: You can introduce a dedicated microservice like `PPSR.Registration.Audit` to asynchronously track changes via a message queue.
---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [Node.js](https://nodejs.org/) with `yarn` installed

### Folder Structure
```
PPSR.Registration/
├── client/                     # Frontend (React + Vite)
└── services/                   # Backend services
    ├── PPSR.Registration.*     # Clean architecture structure
    └── docker-compose.yml      # PostgreSQL container
```

### 1. Setup PostgreSQL with Docker
```bash
cd PPSR.Registration
docker compose up -d
```

### 2. Run Migrations
```bash
cd services
# Apply EF Core migration to PostgreSQL
dotnet ef database update -s PPSR.Registration.WebApi.Ingestor
```

### 3. Run Backend
```bash
cd services
dotnet run --project PPSR.Registration.WebApi.Ingestor
# Runs at https://localhost:7066
```

### 4. Run Frontend
```bash
cd client
yarn install
yarn dev
# Opens at http://localhost:5173
```

> Ensure CORS is enabled and `POSTGRES_*` env vars are set in `.env`.

---

## Highlights
- Graceful handling of malformed CSV
- Smart defaults and auto-corrections (e.g., comma in name → space)
- Shows timestamp and line-level warning messages
- Easily extendable (e.g., batch id tracking, audit logging)

---

## Tech Stack
- Backend: .NET 8, EF Core, PostgreSQL
- Frontend: React 19, TypeScript, TailwindCSS, Vite
- DevOps: Docker, Yarn, Git, Swagger

---

## Project Status
This implementation meets **all functional and technical requirements** outlined in the InfoTrack exercise, including smart normalization, user feedback, and full containerization.

Ready for review or further extension as a production-grade system.

---

## Demo Preview
![LG](https://github.com/user-attachments/assets/a1b2513a-fd12-43c8-bc0e-a644773e2429)

<sub>This GIF demonstrates selecting a CSV file, uploading it, and viewing the result summary.</sub>

![Screenshot 2025-03-29 183447](https://github.com/user-attachments/assets/5165be17-26be-4689-a091-07b2d494a9e1)

<sub>After a successful upload, normalized records are stored in the PostgreSQL table.</sub>

---

## Notes
- To reset database: `docker volume rm ppsrregistration_pgdata`
- To run tests: `dotnet test` inside `/Tests` folder

---

## License and Usage

This project is provided as-is **for InfoTrack interview demonstration purposes only**.  
It is not intended for production use without proper validation, security hardening, and authorization layers.

Commercial or production deployment should be done with caution and proper review.

---

## Author

Developed by **Xinping Liu (Jack)**  
📧 Email: [jacklau9515@gmail.com](mailto:jacklau9515@gmail.com)

All rights reserved © 2025 Xinping Liu.
