# 🔐 Fingerprint Biometric Attendance System

A complete biometric fingerprint attendance system powered by **Blazor Server** (Web UI) and **ASP.NET Core Minimal API** (Hardware Middleware).

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────┐
│                   AttendanceWeb                      │
│            (Blazor Server Web Application)           │
│  • Admin Dashboard (Manage Students, Events)        │
│  • Attendance Records & Reporting                   │
│  • Student Portal (View personal records)           │
└──────────────────┬───────────────────────────────────┘
                   │ HTTP REST API (Port 5000)
                   ▼
┌──────────────────────────────────────────────────────┐
│             fingerprintMiddleware                    │
│         (ASP.NET Core Minimal API)                   │
│  • Hardware Abstraction Layer                        │
│  • /api/fingerprint/capture                          │
│  • /api/fingerprint/enroll                           │
│  • /api/fingerprint/verify                           │
└──────────────────┬───────────────────────────────────┘
                   │ DPUruNet SDK (Native DLLs)
                   ▼
┌──────────────────────────────────────────────────────┐
│           Fingerprint Scanner Hardware               │
│        (DigitalPersona or compatible)                │
└──────────────────────────────────────────────────────┘
```

## 📦 Project Structure

```
AttendanceSystem/
├── AttendanceWeb/             # Main Blazor Server Application
│   ├── Components/Pages/      # Razor Pages (Admin, Student Portal)
│   ├── Services/              # Business Logic & DB Access
│   └── wwwroot/               # Static Assets (CSS, JS)
│
├── fingerprintMiddleware/     # Hardware Interface Service
│   ├── Program.cs             # API Endpoints
│   └── Services/              # SDK Integration
│
└── Libs/                      # Native DLLs
    ├── DPUruNet.dll           # DigitalPersona SDK
    ├── dpfpdd.dll
    └── dpfpdd_ptapi.dll
```

## 🚀 Quick Start

### Prerequisites

- **Windows OS** (required for fingerprint hardware drivers)
- **.NET 9.0 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Fingerprint Scanner** (DigitalPersona or compatible USB device)

### Step 1: Clone & Build

```bash
git clone <repository-url>
cd AttendanceSystem
dotnet build
```

### Step 2: Start the Middleware Service (Hardware Layer)

Open a terminal and run the middleware. This service must stay running to communicate with the fingerprint reader.

```bash
cd fingerprintMiddleware
dotnet run
```
*Listens on: http://localhost:5000*

### Step 3: Start the Web Application

Open a **new terminal** window:

```bash
cd AttendanceWeb
dotnet run
```
*Listens on: http://localhost:5243*

The browser should open automatically to the application home page.

## 💡 Usage

### 👥 For Admin
Access the Admin Dashboard to manage students, events, and view improved analytics.
- **URL**: `http://localhost:5243/admin/login`
- **Default Credentials**: See [ADMIN_GUIDE.md](ADMIN_GUIDE.md)

### 🎓 For Students
Students can view their own attendance history and status.
- **URL**: `http://localhost:5243/student-portal`
- **Lookup**: Enter your Student ID (e.g., `2023-0001`)

### 📝 Attendance Taking
1. Go to the **Attendance Page** (`/attendance`)
2. Select the current **Event** (e.g., "Morning Assembly")
3. Students scan their finger to mark **Time In** or **Time Out**
4. System validates enrollment and records the timestamp

## 🔧 Configuration

### Middleware Settings
Edit `fingerprintMiddleware/appsettings.json`:
- **ReaderTimeout**: Duration to wait for a finger scan (ms).
- **MinAcceptableQuality**: Quality threshold for valid scans.

### Web App Settings
Edit `AttendanceWeb/appsettings.json`:
- **Database**: Connection string for SQLite.
- **Logging**: Log levels for debugging.

## 🐛 Troubleshooting

### "No reader connected"
Ensure the middleware is running (`dotnet run` in `fingerprintMiddleware`) and the device is physically connected.

### "Connection Refused"
Ensure the middleware is running on port 5000. Check `fingerprintMiddleware/Properties/launchSettings.json` if needed.

## 📄 License
[Your License Here]
