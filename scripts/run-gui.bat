@echo off
echo ================================================
echo Fingerprint Biometric System Launcher
echo ================================================
echo.

REM Check if middleware is running
echo [1/3] Checking if middleware is running...
curl -s http://localhost:5000/api/fingerprint/status >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Fingerprint middleware is not running!
    echo.
    echo Please start the middleware first in a separate terminal:
    echo   cd fingerprintMiddleware
    echo   dotnet run
    echo.
    pause
    exit /b 1
)

echo ✓ Middleware is running
echo.

echo [2/3] Building GUI application...
cd FingerprintUI
dotnet build --nologo -v quiet
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Build failed
    pause
    exit /b 1
)

echo ✓ Build successful
echo.

echo [3/3] Launching Fingerprint GUI...
echo.
start bin\Debug\net9.0-windows\FingerprintUI.exe

echo ✓ Application launched!
echo.
echo The Fingerprint Biometric System window should now be open.
echo.
