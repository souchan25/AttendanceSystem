#!/bin/bash
# Quick test runner script for fingerprint middleware tests

echo "==================================="
echo "Fingerprint Middleware Test Runner"
echo "==================================="
echo ""

# Run API tests (no hardware required)
echo "Running API Integration Tests (no scanner needed)..."
dotnet test --filter "FullyQualifiedName~FingerprintApiTests" --logger "console;verbosity=normal"

API_RESULT=$?

echo ""
echo "==================================="
echo "Test Summary"
echo "==================================="
echo "API Tests: $([ $API_RESULT -eq 0 ] && echo '✓ PASSED' || echo '✗ FAILED')"
echo ""
echo "To run hardware tests (requires fingerprint scanner):"
echo "  1. Remove [Fact(Skip = \"...\")] from tests in FingerprintScannerTests.cs"
echo "  2. Run: dotnet test --filter 'FullyQualifiedName~FingerprintScannerTests'"
echo ""

exit $API_RESULT
