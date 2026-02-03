## Quick context

This repository contains a small ASP.NET Core minimal API service that exposes fingerprint reader functionality using the DPUruNet SDK (native DLLs in `Libs/`). The service is implemented in `fingerprintMiddleware/` and targets .NET 9.0.

Key files/directories
- `fingerprintMiddleware/Program.cs` — API surface (minimal APIs). See endpoints: `/api/fingerprint/device-info`, `/capture`, `/verify`, `/enroll`, `/status`, `/reinitialize`, `/settings`.
- `fingerprintMiddleware/Services/FingerprintService.cs` — singletons that manage the hardware reader via DPUruNet. Important: this class opens and holds the reader connection.
- `fingerprintMiddleware/Models/FingerprintModels.cs` — data shapes: `FingerprintData`, `FingerprintResponse`, `DeviceInfo`.
- `Libs/` — contains native/third-party DLLs referenced by the project (`DPUruNet.dll`, `dpfpdd.dll`, `dpfpdd_ptapi.dll`).
- `fingerprintMiddleware/Tests.http` — example HTTP requests useful for manual testing.

## Big-picture architecture notes for agents
- The API is a thin middleware around the DPUruNet fingerprint SDK. The app uses minimal APIs in `Program.cs` but still registers controllers (`AddControllers`) — the primary endpoints live in the top-level file.
- `FingerprintService` is registered as a singleton (see `Program.cs`), because it opens and holds a hardware connection; creating multiple instances will result in concurrent device access issues. Prefer editing behavior inside `FingerprintService` rather than creating additional reader objects.
- Image/Template flow: `CaptureFingerprint()` returns a base64 PNG preview in `FingerprintResponse.ImageData` and a template serialized as XML via `Fmd.SerializeXml(...)`. `VerifyFingerprint()` expects the stored XML template string and compares a live capture to it.

## Platform / runtime constraints
- The capture/verify/enroll image-path methods are decorated with `[SupportedOSPlatform("windows")]`. This service only fully functions on Windows because the native fingerprint drivers / DLLs are Windows-only.
- Native DLLs in `Libs/` must be available to the runtime (copy to output folder or ensure HintPath references work). If you see DllNotFoundException at runtime, confirm the DLLs are present under `bin/Debug/net9.0/` or placed alongside the executable.

## Build / run / debug (concrete commands)
- From repository root (Windows/bash), run the service:

```bash
cd fingerprintMiddleware
dotnet run
```

- Alternatively open `AttendanceSystem.sln` in Visual Studio and run the `fingerprintMiddleware` project.
- Use `fingerprintMiddleware/Tests.http` or the OpenAPI UI (enabled in Development) to exercise endpoints. The project defines a host and sample requests in `Tests.http`.

## Patterns & conventions agents should follow in this repo
- Minimal API + small service class pattern: keep public API wiring in `Program.cs` and implementation details in `Services/`.
- Avoid creating additional `FingerprintService` instances; follow the singleton pattern used by `Program.cs`.
- Configuration is read from `appsettings.json` / `appsettings.Development.json` (look for `Fingerprint` and `Logging` sections). Prefer using existing config keys (`ReaderTimeout`, `MinAcceptableQuality`) when adding configurable behavior.
- Error handling convention: methods return `FingerprintResponse` with Success/Message/Error; follow this shape when adding endpoints or internal flows.

## Integration points & tests
- Native integration: `DPUruNet` interop — changes that touch capture/compare must be tested on Windows hardware or a suitable test harness; CI cannot validate hardware-specific paths unless device/DLLs are available.
- Manual integration tests: `Tests.http` contains example POST bodies for `enroll` and `verify` and GETs for status/device-info.

## Helpful examples extracted from code
- To get device info: inspect `Program.cs` mapping to `/api/fingerprint/device-info` which calls `FingerprintService.GetDeviceInfo()`.
- Template serialization: `CaptureFingerprint()` returns `Template = Fmd.SerializeXml(fmdResult.Data)`; verification uses `Fmd.DeserializeXml(storedTemplateXml)`.

## When editing the project, watch out for
- Moving or renaming `Libs/` DLLs requires updating `fingerprintMiddleware.csproj` HintPath entries or copying the native DLLs to output.
- Any refactor that instantiates `FingerprintService` more than once may break exclusive device access — keep it singleton or centralize access.
- `System.Drawing.Common` and image conversions use Windows graphics APIs — tests that exercise `GetBitmapFromCaptureResult` must run on Windows.

---
If anything above is unclear or you'd like me to include additional examples (e.g., a small snippet on how templates are compared, or a checklist for reproducing DllNotFoundException), tell me which areas to expand and I'll iterate.
