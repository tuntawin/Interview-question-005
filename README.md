# Queue Management System

A full-stack Queue Management Application built with **.NET 10 Web API** and **Angular**.

## Tech Stack

- **Frontend**: Angular 22
- **Backend**: .NET 10 Web API (C#)
- **Database**: SQLite

---

## Prerequisites

- .NET 10 SDK or higher
- Node.js (v18+) and `npm`

---

## Getting Started

### 1. Running Backend (API)

1. Open a terminal and navigate to the `backend` directory:

   ```bash
   cd backend
   ```

2. Run the API project:

   ```bash
   dotnet run --project src/QueueApp.Api
   ```

3. Once started, the backend services are available at:
   - **HTTP Endpoint**: `http://localhost:9292`
   - **HTTPS Endpoint**: `https://localhost:9295`

---

### 2. Running Frontend (Web)

1. Open a new terminal and navigate to the `frontend` directory:

   ```bash
   cd frontend
   ```

2. Install dependencies (if running for the first time):

   ```bash
   npm install
   ```

3. Start the Angular dev server:

   ```bash
   npm start
   ```

4. Once compiled, access the Web application at:
   - **Web App**: `http://localhost:4502`

---

## Running Tests

- **Backend Tests**:

  ```bash
  cd backend
  dotnet test
  ```

- **Frontend Tests**:
  ```bash
  cd frontend
  npm test
  ```
