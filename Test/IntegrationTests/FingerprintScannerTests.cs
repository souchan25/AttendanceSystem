using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FingerprintMiddleware.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FingerprintMiddlewareTests.IntegrationTests;

/// <summary>
/// Tests for scanner-specific operations: capture, enroll, verify, identify.
/// IMPORTANT: These tests require physical user interaction with the scanner.
/// Run these tests manually when hardware is available.
/// </summary>
[Collection("Scanner Hardware Tests")]
public class FingerprintScannerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public FingerprintScannerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CaptureFingerprint_ReturnsTemplateAndImage()
    {
        // Arrange
        Console.WriteLine("Please place finger on scanner...");
        await Task.Delay(2000); // Give user time to place finger

        // Act
        var response = await _client.PostAsync("/api/fingerprint/capture", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success, $"Capture failed: {result.Error}");
        Assert.NotNull(result.Template);
        Assert.NotEmpty(result.Template);
        Assert.NotNull(result.ImageData);
        Assert.NotEmpty(result.ImageData);
        Assert.True(result.Quality > 0, "Quality should be greater than 0");
    }

    [Fact]
    public async Task EnrollFingerprint_CreatesTemplateForUser()
    {
        // Arrange
        var enrollData = new FingerprintData
        {
            UserId = $"test-user-{Guid.NewGuid()}"
        };
        
        Console.WriteLine($"Enrolling user: {enrollData.UserId}");
        Console.WriteLine("Please place finger on scanner...");
        await Task.Delay(2000);

        // Act
        var response = await _client.PostAsJsonAsync("/api/fingerprint/enroll", enrollData);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success, $"Enrollment failed: {result.Error}");
        Assert.Contains(enrollData.UserId, result.Message);
        Assert.NotNull(result.Template);
        Assert.NotEmpty(result.Template);
        
        // Output template for manual verification tests
        Console.WriteLine($"Enrolled template (first 100 chars): {result.Template[..Math.Min(100, result.Template.Length)]}...");
    }

    [Fact]
    public async Task VerifyFingerprint_MatchesEnrolledTemplate()
    {
        // Arrange - First enroll
        var enrollData = new FingerprintData
        {
            UserId = $"verify-test-{Guid.NewGuid()}"
        };
        
        Console.WriteLine("STEP 1: Enrolling fingerprint...");
        Console.WriteLine("Please place finger on scanner...");
        await Task.Delay(2000);
        
        var enrollResponse = await _client.PostAsJsonAsync("/api/fingerprint/enroll", enrollData);
        var enrollResult = await enrollResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        
        Assert.NotNull(enrollResult);
        Assert.True(enrollResult.Success, $"Enrollment failed: {enrollResult.Error}");
        Assert.NotNull(enrollResult.Template);
        
        // Act - Now verify with the same finger
        Console.WriteLine("\nSTEP 2: Verifying fingerprint...");
        Console.WriteLine("Please place the SAME finger on scanner again...");
        await Task.Delay(3000);
        
        var verifyData = new FingerprintData
        {
            Template = enrollResult.Template
        };
        
        var verifyResponse = await _client.PostAsJsonAsync("/api/fingerprint/verify", verifyData);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(verifyResult);
        Assert.True(verifyResult.Success, $"Verification should succeed with matching finger. Error: {verifyResult.Error}, Message: {verifyResult.Message}");
        Assert.Contains("Match", verifyResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyFingerprint_RejectsDifferentFinger()
    {
        // Arrange - Enroll with one finger
        var enrollData = new FingerprintData
        {
            UserId = $"reject-test-{Guid.NewGuid()}"
        };
        
        Console.WriteLine("STEP 1: Enrolling with first finger...");
        Console.WriteLine("Please place finger #1 on scanner...");
        await Task.Delay(2000);
        
        var enrollResponse = await _client.PostAsJsonAsync("/api/fingerprint/enroll", enrollData);
        var enrollResult = await enrollResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        
        Assert.NotNull(enrollResult);
        Assert.True(enrollResult.Success);
        Assert.NotNull(enrollResult.Template);
        
        // Act - Verify with a different finger
        Console.WriteLine("\nSTEP 2: Verifying with DIFFERENT finger...");
        Console.WriteLine("Please place a DIFFERENT finger on scanner...");
        await Task.Delay(3000);
        
        var verifyData = new FingerprintData
        {
            Template = enrollResult.Template
        };
        
        var verifyResponse = await _client.PostAsJsonAsync("/api/fingerprint/verify", verifyData);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(verifyResult);
        Assert.False(verifyResult.Success, "Verification should fail with different finger");
        Assert.Contains("No match", verifyResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IdentifyFingerprint_FromMultipleEnrolledUsers()
    {
        // Arrange - Enroll 3 different users
        var enrolledUsers = new List<(string UserId, string Template)>();
        
        for (int i = 1; i <= 3; i++)
        {
            var userId = $"user-{i}-{Guid.NewGuid().ToString()[..8]}";
            Console.WriteLine($"\nEnrolling User {i}: {userId}");
            Console.WriteLine($"Please place finger #{i} on scanner...");
            await Task.Delay(3000);
            
            var enrollData = new FingerprintData { UserId = userId };
            var enrollResponse = await _client.PostAsJsonAsync("/api/fingerprint/enroll", enrollData);
            var enrollResult = await enrollResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
            
            Assert.NotNull(enrollResult);
            Assert.True(enrollResult.Success, $"Failed to enroll user {i}");
            Assert.NotNull(enrollResult.Template);
            
            enrolledUsers.Add((userId, enrollResult.Template));
        }
        
        // Act - Capture a live fingerprint and identify which user it matches
        Console.WriteLine("\n\n=== IDENTIFICATION TEST ===");
        Console.WriteLine("Please place one of the enrolled fingers on scanner...");
        await Task.Delay(3000);
        
        var captureResponse = await _client.PostAsync("/api/fingerprint/capture", null);
        var captureResult = await captureResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        
        Assert.NotNull(captureResult);
        Assert.True(captureResult.Success, "Failed to capture fingerprint for identification");
        Assert.NotNull(captureResult.Template);
        
        // Compare against all enrolled templates to identify the user
        string? identifiedUser = null;
        foreach (var (userId, template) in enrolledUsers)
        {
            var verifyData = new FingerprintData { Template = template };
            var verifyResponse = await _client.PostAsJsonAsync("/api/fingerprint/verify", verifyData);
            var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
            
            if (verifyResult != null && verifyResult.Success)
            {
                identifiedUser = userId;
                break;
            }
        }
        
        // Assert
        Assert.NotNull(identifiedUser);
        Console.WriteLine($"\n✓ Successfully identified user: {identifiedUser}");
    }

    [Fact]
    public async Task VerifyFingerprint_WithMissingTemplate_ReturnsBadRequest()
    {
        // Arrange
        var verifyData = new FingerprintData
        {
            Template = "" // Missing template
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/fingerprint/verify", verifyData);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrollFingerprint_WithMissingUserId_ReturnsBadRequest()
    {
        // Arrange
        var enrollData = new FingerprintData
        {
            UserId = "" // Missing user ID
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/fingerprint/enroll", enrollData);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<FingerprintResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
