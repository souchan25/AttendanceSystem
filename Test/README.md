# Fingerprint Middleware Integration Tests

This test project contains comprehensive integration tests for the fingerprint middleware API.

## Test Categories

### 1. API Integration Tests (`FingerprintApiTests.cs`)
These tests verify the API endpoints work correctly and can run **without** physical scanner hardware:
- ✅ Device info endpoint
- ✅ Status endpoint  
- ✅ Settings get/update endpoints
- ✅ Reader reinitialization

**Run these tests anytime:**
```bash
dotnet test --filter "FullyQualifiedName~FingerprintApiTests"
```

### 2. Scanner Hardware Tests (`FingerprintScannerTests.cs`)
These tests require a **physical fingerprint scanner** connected to Windows and **manual user interaction**:
- 🔒 Fingerprint capture with image and template extraction
- 🔒 User enrollment workflow
- 🔒 Fingerprint verification (match/reject)
- 🔒 Multi-user identification (1:N matching)
- 🔒 Input validation (missing template/userId)

**These tests are SKIPPED by default.** To run them:

1. Ensure fingerprint scanner is connected
2. Remove the `Skip` attribute from the tests you want to run
3. Run tests one at a time for manual interaction:

```bash
# Run a specific test
dotnet test --filter "FullyQualifiedName~CaptureFingerprint_ReturnsTemplateAndImage"

# Or run all scanner tests (after removing Skip attributes)
dotnet test --filter "FullyQualifiedName~FingerprintScannerTests"
```

## Prerequisites

- ✅ .NET 9.0 SDK
- ✅ Windows OS (required for DPUruNet native DLLs)
- ✅ Fingerprint scanner hardware (for scanner tests only)
- ✅ Fingerprint middleware service running (tests use `WebApplicationFactory` for in-memory hosting)

## Running Tests

### Run all non-skipped tests:
```bash
cd Test
dotnet test
```

### Run only API tests (no hardware needed):
```bash
dotnet test --filter "FullyQualifiedName~FingerprintApiTests"
```

### Run with verbose output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Generate coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Workflow Examples

### Enrollment Test Flow
1. Test calls `/api/fingerprint/enroll` with a userId
2. Console prompts: "Please place finger on scanner..."
3. User places finger on scanner
4. API captures fingerprint, extracts template, returns success
5. Test validates template is present and non-empty

### Verification Test Flow
1. Test enrolls a fingerprint (step 1)
2. Console prompts: "Please place the SAME finger on scanner again..."
3. User places same finger
4. API compares live capture against stored template
5. Test validates Success=true and message contains "Match"

### Identification Test Flow (1:N Matching)
1. Test enrolls 3 different users with different fingers
2. Console prompts: "Please place one of the enrolled fingers..."
3. User places enrolled finger
4. Test captures live fingerprint and compares against all 3 enrolled templates
5. Test validates correct user is identified

## Important Notes

⚠️ **Hardware Tests Require Manual Interaction**  
Scanner tests are marked with `[Fact(Skip = "...")]` because they require:
- Physical hardware to be connected
- User to place/remove fingers at specific times
- Adequate time delays for user interaction (2-3 seconds)

⚠️ **Windows-Only**  
Native fingerprint SDK DLLs only work on Windows. Tests will fail on Linux/macOS.

⚠️ **Test Isolation**  
Scanner tests are independent and can run in any order. Each test uses unique user IDs (GUIDs) to avoid conflicts.

## Troubleshooting

### "Fingerprint reader not initialized"
- Ensure scanner is connected via USB
- Check Windows Device Manager for driver issues
- Run the middleware service manually first to verify scanner works
- Try the `/api/fingerprint/reinitialize` endpoint

### Tests hang waiting for capture
- Check that native DLLs (`DPUruNet.dll`, `dpfpdd.dll`, `dpfpdd_ptapi.dll`) are in the output folder
- Verify scanner LED is on/blinking (indicates ready state)
- Try restarting the test or reseating the USB cable

### "No match" when fingers should match
- Ensure you're using the exact same finger for enroll and verify
- Place finger firmly and consistently on scanner
- Check `minAcceptableQuality` setting (lower = more lenient)
- Verify `farDivisor` setting (higher = stricter matching)

## Extending Tests

To add new test cases:

1. Add test method to appropriate test class
2. Mark hardware-dependent tests with `[Fact(Skip = "...")]`
3. Use `Console.WriteLine()` to guide user interaction
4. Add `await Task.Delay()` to give users time to interact
5. Validate both success and error scenarios

Example:
```csharp
[Fact(Skip = "Requires manual scanning")]
public async Task YourNewTest()
{
    Console.WriteLine("Please place finger...");
    await Task.Delay(2000);
    
    var response = await _client.PostAsync("/api/fingerprint/capture", null);
    var result = await response.Content.ReadFromJsonAsync<FingerprintResponse>();
    
    Assert.NotNull(result);
    Assert.True(result.Success);
}
```
