using FingerprintMiddleware.Models;
using FingerprintMiddleware.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the FingerprintService as a singleton so it maintains the reader connection
builder.Services.AddSingleton<FingerprintService>();

// Configure CORS for cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Enable controller support
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

// Use CORS with the "AllowAll" policy
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseRouting();

// Fingerprint API endpoints
app.MapGet("/api/fingerprint/settings", (FingerprintService svc) =>
{
    var s = svc.GetSettings();
    return Results.Ok(new { farDivisor = s.FarDivisor, minAcceptableQuality = s.MinAcceptableQuality });
});

app.MapPost("/api/fingerprint/settings", (FingerprintService svc, dynamic body) =>
{
    try
    {
        int? farDivisor = null;
        int? minQuality = null;
        if (body != null)
        {
            try { farDivisor = (int?)body.farDivisor; } catch { }
            try { minQuality = (int?)body.minAcceptableQuality; } catch { }
        }
        svc.UpdateSettings(farDivisor, minQuality);
        var s = svc.GetSettings();
        return Results.Ok(new { farDivisor = s.FarDivisor, minAcceptableQuality = s.MinAcceptableQuality });
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid settings payload" });
    }
});
app.MapGet("/api/fingerprint/device-info", async (FingerprintService fingerprintService) =>
{
    var response = await fingerprintService.GetDeviceInfo();
    return Results.Ok(response);
})
.WithName("GetDeviceInfo")
.WithOpenApi();

app.MapPost("/api/fingerprint/capture", async (FingerprintService fingerprintService) =>
{
    var response = await fingerprintService.CaptureFingerprint();
    return Results.Ok(response);
})
.WithName("CaptureFingerprint")
.WithOpenApi();

app.MapPost("/api/fingerprint/verify", async (FingerprintService fingerprintService, FingerprintData data) =>
{
    if (string.IsNullOrEmpty(data.Template))
    {
        return Results.BadRequest(new FingerprintResponse 
        { 
            Success = false,
            Message = "Template is required for verification",
            Error = "Missing template data"
        });
    }
    
    var response = await fingerprintService.VerifyFingerprint(data.Template);
    return Results.Ok(response);
})
.WithName("VerifyFingerprint")
.WithOpenApi();

app.MapPost("/api/fingerprint/compare", async (FingerprintService fingerprintService, HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        
        if (data == null || !data.ContainsKey("Template1") || !data.ContainsKey("Template2"))
        {
            return Results.BadRequest(new FingerprintResponse 
            { 
                Success = false,
                Message = "Both Template1 and Template2 are required for comparison",
                Error = "Missing template data"
            });
        }
        
        var response = await fingerprintService.CompareTemplates(data["Template1"], data["Template2"]);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Ok(new FingerprintResponse
        {
            Success = false,
            Message = "Failed to compare templates",
            Error = ex.Message
        });
    }
})
.WithName("CompareTemplates")
.WithOpenApi();

app.MapPost("/api/fingerprint/enroll", async (FingerprintService fingerprintService, FingerprintData data) =>
{
    if (string.IsNullOrEmpty(data.UserId))
    {
        return Results.BadRequest(new FingerprintResponse 
        { 
            Success = false,
            Message = "User ID is required for enrollment",
            Error = "Missing user ID"
        });
    }
    
    var response = await fingerprintService.EnrollFingerprint(data.UserId);
    return Results.Ok(response);
})
.WithName("EnrollFingerprint")
.WithOpenApi();

app.MapGet("/api/fingerprint/status", (FingerprintService fingerprintService) =>
{
    bool isConnected = fingerprintService.IsReaderConnected();
    return Results.Ok(new { IsConnected = isConnected });
})
.WithName("GetReaderStatus")
.WithOpenApi();

app.MapPost("/api/fingerprint/reinitialize", async (FingerprintService fingerprintService) =>
{
    await fingerprintService.ReinitializeReaderAsync();
    return Results.Ok(new { Success = true, Message = "Reader reinitialization requested" });
})
.WithName("ReinitializeReader")
.WithOpenApi();

app.MapPost("/api/fingerprint/reset", (FingerprintService fingerprintService) =>
{
    fingerprintService.ResetReader();
    return Results.Ok(new { Success = true, Message = "Fingerprint reader reset successfully." });
})
.WithName("ResetReader")
.WithOpenApi();

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
