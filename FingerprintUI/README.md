# Fingerprint Biometric System - GUI Application

A Windows Forms GUI application for fingerprint enrollment, verification, and identification using biometric fingerprint scanning.

## Features

### 📝 Enrollment Tab
- Enroll new users with User ID and full name
- Capture fingerprint with real-time quality feedback
- Store fingerprint templates securely in local database
- Display captured fingerprint image preview
- Automatic validation of user inputs

### ✅ Verification Tab
- Select enrolled user from dropdown
- Verify identity by scanning fingerprint
- 1:1 matching against stored template
- Visual and audio feedback for match/no-match
- Display verification confidence and match score

### 🔍 Identification Tab  
- Identify user from fingerprint scan (1:N matching)
- Search through all enrolled users automatically
- Display list of all enrolled users
- Highlight matched user when identified
- Real-time search progress indication

## Prerequisites

- ✅ Windows OS (required for fingerprint hardware)
- ✅ .NET 9.0 SDK
- ✅ Fingerprint scanner hardware connected
- ✅ **Fingerprint Middleware service running** on `http://localhost:5000`

## Running the Application

### Step 1: Start the Fingerprint Middleware

Before running the GUI, you **must** start the fingerprint middleware service:

```bash
cd fingerprintMiddleware
dotnet run
```

The middleware should show:
```
Now listening on: http://localhost:5000
```

### Step 2: Run the GUI Application

In a new terminal:

```bash
cd FingerprintUI
dotnet run
```

Or double-click the executable:
```
FingerprintUI\bin\Debug\net9.0-windows\FingerprintUI.exe
```

## User Workflows

### Enrolling a New User

1. Click the **Enroll** tab
2. Enter a unique **User ID** (e.g., "EMP001", "user123")
3. Enter the user's **Full Name**
4. Click **"Capture & Enroll Fingerprint"**
5. Place finger on the scanner when prompted
6. Wait for success confirmation
7. Fingerprint template is saved automatically

### Verifying a User

1. Click the **Verify** tab
2. Select a user from the dropdown list
3. Click **"Scan & Verify Fingerprint"**
4. Place finger on the scanner
5. System will confirm if fingerprint matches the selected user
   - ✓ Green = Match (identity confirmed)
   - ✗ Red = No match

### Identifying an Unknown User

1. Click the **Identify** tab
2. Click **"Scan & Identify Fingerprint"**
3. Place finger on the scanner
4. System searches all enrolled users automatically
5. If found, displays user's name and ID
6. If not found, shows "Not Identified"

## Data Storage

User data is stored locally in JSON format:
- Location: `%AppData%\FingerprintUI\users.json`
- Contains: User ID, Name, Fingerprint template (XML), Enrollment date
- **Note**: Templates are encrypted XML data, not raw images

## Architecture

```
┌─────────────────┐
│  FingerprintUI  │  (Windows Forms GUI)
│    (Port N/A)   │
└────────┬────────┘
         │ HTTP REST API
         ▼
┌─────────────────┐
│ Fingerprint     │  (ASP.NET Core Minimal API)
│ Middleware      │  (Port 5000)
└────────┬────────┘
         │ Native DLL calls
         ▼
┌─────────────────┐
│  DPUruNet SDK   │  (Native Windows DLLs)
│  Hardware Layer │
└────────┬────────┘
         │ USB
         ▼
┌─────────────────┐
│  Fingerprint    │
│    Scanner      │
└─────────────────┘
```

## Troubleshooting

### "Cannot connect to fingerprint service"
- **Cause**: Middleware is not running
- **Solution**: Start the middleware first: `cd fingerprintMiddleware && dotnet run`

### "No reader connected" or "Enrollment failed"
- **Cause**: Scanner hardware not detected
- **Solution**: 
  1. Check USB connection
  2. Verify scanner drivers are installed
  3. Check Device Manager for fingerprint device
  4. Restart middleware service

### Fingerprint quality too low
- **Cause**: Poor fingerprint placement
- **Solution**:
  - Clean finger and scanner surface
  - Press finger firmly and evenly
  - Try a different finger
  - Adjust `MinAcceptableQuality` in middleware `appsettings.json`

### User not found in identify mode
- **Cause**: 
  - User not enrolled
  - Different finger used
  - Low match threshold
- **Solution**:
  - Ensure user is enrolled with the same finger
  - Check enrolled users list in Identify tab
  - Adjust `farDivisor` setting (higher = stricter matching)

## Configuration

The GUI connects to the middleware at `http://localhost:5000` by default.

To change the API URL, edit `FingerprintUI/Services/FingerprintApiClient.cs`:

```csharp
public FingerprintApiClient(string baseUrl = "http://localhost:5000")
```

## Development

### Project Structure
```
FingerprintUI/
├── Models/
│   └── FingerprintModels.cs    # Data models
├── Services/
│   ├── FingerprintApiClient.cs # HTTP API client
│   └── UserDatabase.cs         # Local JSON storage
├── MainForm.cs                 # UI logic
├── MainForm.Designer.cs        # UI layout
└── Program.cs                  # Entry point
```

### Adding Features

To add new features:
1. Add API methods to `FingerprintApiClient.cs`
2. Update UI in `MainForm.Designer.cs`
3. Add event handlers in `MainForm.cs`
4. Update `UserDatabase.cs` if new data storage needed

## Security Notes

⚠️ **Important**: This is a demo application. For production use:
- Add encryption for stored templates
- Implement user authentication
- Use HTTPS for API communication
- Add audit logging
- Implement proper error handling and validation
- Secure the user database with access controls

## License

Part of the AttendanceSystem project.
