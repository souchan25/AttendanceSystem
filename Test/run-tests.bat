@echo off
REM Quick test runner script for fingerprint middleware tests

echo ===================================
echo Fingerprint Middleware Test Runner
echo ===================================
echo.

REM Run API tests (no hardware required)
echo Running API Integration Tests (no scanner needed)...
dotnet test --filter "FullyQualifiedName~FingerprintApiTests" --logger "console;verbosity=normal"

set API_RESULT=%ERRORLEVEL%

echo.
echo ===================================
echo Test Summary
echo ===================================
if %API_RESULT%==0 (
    echo API Tests: PASSED
) else (
    echo API Tests: FAILED
)
echo.
echo To run hardware tests (requires fingerprint scanner):
echo   1. Remove [Fact(Skip = "...")] from tests in FingerprintScannerTests.cs
echo   2. Run: dotnet test --filter "FullyQualifiedName~FingerprintScannerTests"
echo.

exit /b %API_RESULT%
