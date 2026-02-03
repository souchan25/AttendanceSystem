# 🔐 Fingerprint Biometric Attendance System

A complete biometric fingerprint authentication system with a minimal API middleware and Windows Forms GUI for enrollment, verification, and identification.

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────┐
│                   FingerprintUI                      │
│            (Windows Forms GUI Client)                │
│  • Enroll users with fingerprints                   │
│  • Verify user identity (1:1 match)                 │
│  • Identify users from scan (1:N search)            │
└──────────────────┬───────────────────────────────────┘
                   │ HTTP REST API (Port 5000)
                   ▼
┌──────────────────────────────────────────────────────┐
│             fingerprintMiddleware                    │
│         (ASP.NET Core Minimal API)                   │
│  • /api/fingerprint/capture                          │
│  • /api/fingerprint/enroll                           │
│  • /api/fingerprint/verify                           │
│  • /api/fingerprint/device-info                      │
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
├── fingerprintMiddleware/     # ASP.NET Core API service
│   ├── Program.cs             # API endpoints
│   ├── Services/
│   │   └── FingerprintService.cs   # Hardware integration
│   └── Models/
│       └── FingerprintModels.cs    # API models
│
├── FingerprintUI/             # Windows Forms GUI
│   ├── MainForm.cs            # UI logic
│   ├── Services/
│   │   ├── FingerprintApiClient.cs  # HTTP client
│   │   └── UserDatabase.cs          # Local storage
│   └── Models/
│       └── FingerprintModels.cs     # Data models
│
├── Test/                      # Integration tests
│   ├── IntegrationTests/
│   │   ├── FingerprintApiTests.cs      # API tests
│   │   └── FingerprintScannerTests.cs  # Hardware tests
│   └── README.md
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

### Step 2: Start the Middleware Service

```bash
cd fingerprintMiddleware
dotnet run
```

Output should show:
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

### Step 3: Launch the GUI Application

In a **new terminal**:

```bash
# Quick launcher (recommended)
./run-gui.bat   # Windows
./run-gui.sh    # Linux/macOS

# OR run directly
cd FingerprintUI
dotnet run
```

## 💡 Usage

### 📝 Enrolling Users

1. Open the **Enroll** tab
2. Enter User ID (e.g., `EMP001`)
3. Enter Full Name (e.g., `John Doe`)
4. Click **"Capture & Enroll Fingerprint"**
5. Place finger on scanner
6. Wait for success confirmation

The fingerprint template is saved locally for future verification.

### ✅ Verifying Users

1. Open the **Verify** tab
2. Select a user from the dropdown
3. Click **"Scan & Verify Fingerprint"**
4. Place finger on scanner
5. System confirms match or rejection

**Use case**: Access control, attendance check-in

### 🔍 Identifying Users

1. Open the **Identify** tab
2. Click **"Scan & Identify Fingerprint"**
3. Place finger on scanner
4. System searches all enrolled users
5. Displays matched user or "Not Identified"

**Use case**: Unknown user lookup, attendance without pre-selection

## 🧪 Testing

The project includes comprehensive integration tests:

```bash
cd Test

# Run API tests (no hardware needed)
dotnet test --filter "FullyQualifiedName~FingerprintApiTests"

# Run hardware tests (requires scanner)
dotnet test --filter "FullyQualifiedName~FingerprintScannerTests"

# Quick test script
./run-tests.bat   # Windows
./run-tests.sh    # Linux
```

See [`Test/README.md`](Test/README.md) for detailed testing documentation.

## 🔧 Configuration

### Middleware Settings

Edit `fingerprintMiddleware/appsettings.json`:

```json
{
  "Fingerprint": {
    "ReaderTimeout": 10000,
    "MinAcceptableQuality": 60
  }
}
```

- **ReaderTimeout**: Milliseconds to wait for scan
- **MinAcceptableQuality**: 0-100, higher = stricter quality check

### Match Sensitivity

Adjust in `FingerprintService.cs`:

```csharp
private int _farDivisor = 100000;  // Higher = stricter matching
```

Or use the `/api/fingerprint/settings` endpoint at runtime.

## 📡 API Reference

### Endpoints

```http
GET  /api/fingerprint/device-info     # Get scanner info
GET  /api/fingerprint/status          # Check connection
POST /api/fingerprint/capture         # Capture fingerprint
POST /api/fingerprint/enroll          # Enroll new user
POST /api/fingerprint/verify          # Verify fingerprint
GET  /api/fingerprint/settings        # Get current settings
POST /api/fingerprint/settings        # Update settings
POST /api/fingerprint/reinitialize    # Restart scanner
```

### Example: Enroll a User

```bash
curl -X POST http://localhost:5000/api/fingerprint/enroll \
  -H "Content-Type: application/json" \
  -d '{"userId": "user123"}'
```

Response:
```json
{
  "success": true,
  "message": "Fingerprint enrolled successfully for user user123.",
  "template": "<FMD>...</FMD>",
  "imageData": "iVBORw0KGgoAAAANS...",
  "quality": 85
}
```

See [`fingerprintMiddleware/Tests.http`](fingerprintMiddleware/Tests.http) for more examples.

## 🐛 Troubleshooting

### "No reader connected"

**Cause**: Scanner not detected by middleware

**Solutions**:
1. Check USB connection
2. Verify scanner appears in Device Manager
3. Restart middleware: `POST /api/fingerprint/reinitialize`
4. Check native DLLs are in `bin/Debug/net9.0/`

### "Cannot connect to fingerprint service"

**Cause**: Middleware not running

**Solution**: Start middleware first:
```bash
cd fingerprintMiddleware && dotnet run
```

### Low fingerprint quality

**Solutions**:
- Clean finger and scanner surface
- Press finger firmly and evenly
- Lower `MinAcceptableQuality` in settings
- Use a different finger

### Verification always fails

**Solutions**:
- Ensure using the same finger for enroll and verify
- Increase `farDivisor` (makes matching more lenient)
- Re-enroll user with better quality scan

## 🔒 Security Considerations

⚠️ **This is a demo/development application**

For production deployment:

- [ ] Enable HTTPS for API communication
- [ ] Encrypt fingerprint templates at rest
- [ ] Add user authentication/authorization
- [ ] Implement audit logging
- [ ] Secure the user database with access controls
- [ ] Add rate limiting on API endpoints
- [ ] Validate and sanitize all inputs
- [ ] Use secure session management
- [ ] Implement proper error handling (don't leak details)

## 📝 AI Agent Instructions

See [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for detailed guidance on:
- Project architecture and patterns
- Build/test/debug workflows
- Integration points and dependencies
- Common gotchas and troubleshooting

## 📄 License

[Your License Here]

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `cd Test && dotnet test`
5. Submit a pull request

## 📞 Support

For issues or questions:
- Check [`FingerprintUI/README.md`](FingerprintUI/README.md) for GUI documentation
- Check [`Test/README.md`](Test/README.md) for testing guide
- Review [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for architecture details

---

**Built with**: .NET 9.0 | ASP.NET Core | Windows Forms | DigitalPersona SDK
