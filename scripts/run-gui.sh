#!/bin/bash

echo "================================================"
echo "Fingerprint Biometric System Launcher"
echo "================================================"
echo ""

# Check if middleware is running
echo "[1/3] Checking if middleware is running..."
if ! curl -s http://localhost:5000/api/fingerprint/status > /dev/null 2>&1; then
    echo ""
    echo "WARNING: Fingerprint middleware is not running!"
    echo ""
    echo "Please start the middleware first in a separate terminal:"
    echo "  cd fingerprintMiddleware"
    echo "  dotnet run"
    echo ""
    exit 1
fi

echo "✓ Middleware is running"
echo ""

echo "[2/3] Building GUI application..."
cd FingerprintUI
dotnet build --nologo -v quiet
if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi

echo "✓ Build successful"
echo ""

echo "[3/3] Launching Fingerprint GUI..."
echo ""
dotnet run

echo "✓ Application closed"
