using DPUruNet;
using FingerprintMiddleware.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Threading;

namespace FingerprintMiddleware.Services
{
    public class FingerprintService
    {
        private ReaderCollection? _readers;
        private Reader? _selectedReader;
        private bool _isInitialized = false;
        private volatile bool _cancellationRequested = false;

        // Runtime settings (can be adjusted via API)
        private int _farDivisor = 100000; // probability-one / farDivisor (default ~1e-5)
        private int _minAcceptableQuality = 60; // default minimum acceptable quality

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FingerprintService()
        {
            InitializeReader();
        }

        private void InitializeReader()
        {
            try
            {
                _readers = ReaderCollection.GetReaders();
                if (_readers != null && _readers.Count > 0)
                {
                    _selectedReader = _readers[0];
                    Console.WriteLine($"[FingerprintService] Detected reader: {_selectedReader.Description.Name}");
                    Console.WriteLine($"[FingerprintService] Attempting to open reader with priority COOPERATIVE...");
                    
                    var result = _selectedReader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
                    
                    if (result != Constants.ResultCode.DP_SUCCESS)
                    {
                        Console.WriteLine($"[FingerprintService] Failed to open reader. Error Code: {result}");
                    }
                    else
                    {
                        Console.WriteLine($"[FingerprintService] Successfully opened reader.");
                    }
                    
                    _isInitialized = result == Constants.ResultCode.DP_SUCCESS;
                }
                else
                {
                    Console.WriteLine("[FingerprintService] No readers found (ReaderCollection empty).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing fingerprint reader: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        public (int FarDivisor, int MinAcceptableQuality) GetSettings()
        {
            return (_farDivisor, _minAcceptableQuality);
        }

        public void UpdateSettings(int? farDivisor, int? minAcceptableQuality)
        {
            if (farDivisor.HasValue && farDivisor.Value > 0)
            {
                _farDivisor = farDivisor.Value;
            }
            if (minAcceptableQuality.HasValue && minAcceptableQuality.Value >= 0 && minAcceptableQuality.Value <= 100)
            {
                _minAcceptableQuality = minAcceptableQuality.Value;
            }
        }

        public Task<FingerprintResponse> GetDeviceInfo()
        {
            if (!_isInitialized || _selectedReader == null)
            {
                return Task.FromResult(new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized.",
                    Error = "No reader connected."
                });
            }

            try
            {
                // Get reader status instead of capabilities
                var status = _selectedReader.Status;
                
                return Task.FromResult(new FingerprintResponse
                {
                    Success = true,
                    Message = "Device info retrieved successfully.",
                    Template = $"Name: {_selectedReader.Description.Name}, Status: {status}"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new FingerprintResponse
                {
                    Success = false,
                    Message = "Error retrieving device info.",
                    Error = ex.Message
                });
            }
        }

        [SupportedOSPlatform("windows")]
        public async Task<FingerprintResponse> CaptureFingerprint()
        {
            // If not initialized, try to reinitialize once
            if (!_isInitialized || _selectedReader == null)
            {
                Console.WriteLine("[FingerprintService] Reader not initialized. Attempting reinitialization...");
                await ReinitializeReaderAsync();
            }

            if (!_isInitialized || _selectedReader == null)
            {
                string deviceList = "(none)";
                try
                {
                    var readers = ReaderCollection.GetReaders();
                    if (readers != null && readers.Count > 0)
                        deviceList = string.Join(", ", readers.Select(r => r.Description.Name));
                }
                catch (Exception ex)
                {
                    deviceList = $"Error listing devices: {ex.Message}";
                }
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized after reattempt.",
                    Error = $"No reader connected. Devices found: {deviceList}"
                };
            }

            // Lock the semaphore to ensure only one operation at a time
            await _semaphore.WaitAsync();
            try
            {
                try
                {
                    // Get reader capabilities
                    var caps = _selectedReader.Capabilities;
                    // Set default resolution - first one is typically the lowest which is fine for our needs
                    int resolution = 500; // Default to 500 DPI if no resolutions available
                    
                    if (caps.Resolutions != null && caps.Resolutions.Length > 0)
                    {
                        resolution = caps.Resolutions[0];
                    }

                    CaptureResult? captureResult = null;
                    var startTime = DateTime.Now;
                    var timeout = TimeSpan.FromSeconds(30);

                    // Wait loop for finger placement
                    while (DateTime.Now - startTime < timeout)
                    {
                        if (_cancellationRequested)
                        {
                            return new FingerprintResponse
                            {
                                Success = false,
                                Message = "Operation cancelled by system.",
                                Error = "Cancelled"
                            };
                        }

                        captureResult = await Task.Run(() => _selectedReader.Capture(
                            Constants.Formats.Fid.ANSI, 
                            Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 
                            resolution, 
                            resolution));
                        
                        if (captureResult.ResultCode == Constants.ResultCode.DP_SUCCESS && captureResult.Data != null)
                        {
                            break; // Successfully captured
                        }
                        
                        // If not successful, wait a bit before retrying
                        await Task.Delay(200);
                    }
                    
                    if (captureResult == null || captureResult.Data == null || captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = "Timeout or failed to capture fingerprint.",
                            Error = captureResult != null ? $"Capture error: {captureResult.ResultCode}" : "Timeout"
                        };
                    }

                    // Get fingerprint quality - using heuristic for now
                    int quality = 0;
                    if (captureResult.Data.Views.Count > 0)
                    {
                        // Set to a default quality based on image clarity - actual value would require a deeper analysis
                        quality = 70; // Default medium-high quality
                    }

                    if (quality < _minAcceptableQuality)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = $"Low quality capture ({quality} < {_minAcceptableQuality}).",
                            Error = "LowQuality",
                            ImageData = await GetBitmapFromCaptureResult(captureResult),
                            Quality = quality
                        };
                    }

                    // Create template from captured data
                    var fmdResult = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                    if (fmdResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = "Failed to extract features from fingerprint.",
                            Error = $"Feature extraction error: {fmdResult.ResultCode}"
                        };
                    }

                    // Get image data for preview
                    string imageData = await GetBitmapFromCaptureResult(captureResult);

                    // Serialize template as XML (string) for consistent storage/verification
                    string templateXml = Fmd.SerializeXml(fmdResult.Data);

                    return new FingerprintResponse
                    {
                        Success = true,
                        Message = "Fingerprint captured successfully.",
                        Template = templateXml,
                        ImageData = imageData,
                        Quality = quality
                    };
                }
                catch (Exception ex)
                {
                    return new FingerprintResponse
                    {
                        Success = false,
                        Message = "Error capturing fingerprint.",
                        Error = ex.Message
                    };
                }
            }
            finally
            {
                // Ensure proper release of the reader after operation
                // DisposeReader(); // REMOVED: Keep reader open for singleton
                // Ensure semaphore is released even if an exception occurs
                _semaphore.Release();
            }
        }

        [SupportedOSPlatform("windows")]
        private async Task<string> GetBitmapFromCaptureResult(CaptureResult captureResult)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (captureResult?.Data?.Views?.Count > 0)
                    {
                        // Convert captured fingerprint to bitmap
                        var bitmap = new Bitmap(captureResult.Data.Views[0].Width, captureResult.Data.Views[0].Height);
                        var bmpData = bitmap.LockBits(
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.WriteOnly,
                            PixelFormat.Format8bppIndexed);

                        // Copy fingerprint image data to bitmap
                        System.Runtime.InteropServices.Marshal.Copy(captureResult.Data.Views[0].RawImage, 0, bmpData.Scan0, captureResult.Data.Views[0].RawImage.Length);
                        bitmap.UnlockBits(bmpData);

                        // Set up the color palette for the 8bpp bitmap
                        ColorPalette colorPalette = bitmap.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bitmap.Palette = colorPalette;

                        // Convert bitmap to base64 string
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting fingerprint to image: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        [SupportedOSPlatform("windows")]
        public async Task<FingerprintResponse> VerifyFingerprint(string storedTemplateXml)
        {
            if (!_isInitialized || _selectedReader == null)
            {
                Console.WriteLine("[FingerprintService] Reader not initialized. Attempting reinitialization...");
                await ReinitializeReaderAsync();
            }
            if (!_isInitialized || _selectedReader == null)
            {
                string deviceList = "(none)";
                try
                {
                    var readers = ReaderCollection.GetReaders();
                    if (readers != null && readers.Count > 0)
                        deviceList = string.Join(", ", readers.Select(r => r.Description.Name));
                }
                catch (Exception ex)
                {
                    deviceList = $"Error listing devices: {ex.Message}";
                }
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized after reattempt.",
                    Error = $"No reader connected. Devices found: {deviceList}"
                };
            }

            // Lock the semaphore to ensure only one operation at a time
            await _semaphore.WaitAsync();
            try
            {
                try
                {
                    // Get reader capabilities
                    var caps = _selectedReader.Capabilities;
                    // Set default resolution
                    int resolution = 500; // Default to 500 DPI if no resolutions available
                    
                    if (caps.Resolutions != null && caps.Resolutions.Length > 0)
                    {
                        resolution = caps.Resolutions[0];
                    }

                    // Capture live fingerprint
                    var captureResult = await Task.Run(() => _selectedReader.Capture(
                        Constants.Formats.Fid.ANSI, 
                        Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 
                        resolution, 
                        resolution));
                        
                    if (captureResult.Data == null || captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = "Failed to capture fingerprint.",
                            Error = $"Capture error: {captureResult.ResultCode}"
                        };
                    }

                    // Extract features from the captured fingerprint
                    var liveTemplate = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                    if (liveTemplate.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = "Failed to extract features from fingerprint.",
                            Error = $"Feature extraction error: {liveTemplate.ResultCode}"
                        };
                    }

                    // Create Fmd object from stored XML template
                    Fmd? storedTemplate;
                    try
                    {
                        storedTemplate = Fmd.DeserializeXml(storedTemplateXml);
                    }
                    catch (Exception ex)
                    {
                        return new FingerprintResponse
                        {
                            Success = false,
                            Message = "Failed to deserialize stored template.",
                            Error = ex.Message
                        };
                    }

                    // Compare templates using a proper False Accept Rate (FAR) threshold
                    var compareResult = Comparison.Compare(storedTemplate, 0, liveTemplate.Data, 0);
                    // Lower score means better match. DigitalPersona probability-one is 0x7FFFFFFF
                    const int PROBABILITY_ONE = 0x7FFFFFFF;
                    int divisor = _farDivisor <= 0 ? 100000 : _farDivisor;
                    int targetFAR = PROBABILITY_ONE / divisor; // configurable FAR
                    bool isMatch = compareResult.ResultCode == Constants.ResultCode.DP_SUCCESS && compareResult.Score < targetFAR;

                    // Get image for UI feedback
                    string imageData = await GetBitmapFromCaptureResult(captureResult);

                    // Set quality as a calculated value
                    int quality = 70; // Default value

                    return new FingerprintResponse
                    {
                        Success = isMatch,
                        Message = isMatch ? $"Match (score {compareResult.Score}, thr {targetFAR})" : $"No match (score {compareResult.Score}, thr {targetFAR})",
                        ImageData = imageData,
                        Quality = quality
                    };
                }
                catch (Exception ex)
                {
                    return new FingerprintResponse
                    {
                        Success = false,
                        Message = "Error verifying fingerprint.",
                        Error = ex.Message
                    };
                }
            }
            finally
            {
                // Ensure proper release of the reader after operation
                // DisposeReader(); // REMOVED: Keep reader open for singleton
                // Ensure semaphore is released even if an exception occurs
                _semaphore.Release();
            }
        }

        [SupportedOSPlatform("windows")]
        public async Task<FingerprintResponse> EnrollFingerprint(string userId)
        {
            if (!_isInitialized || _selectedReader == null)
            {
                Console.WriteLine("[FingerprintService] Reader not initialized. Attempting reinitialization...");
                await ReinitializeReaderAsync();
            }
            if (!_isInitialized || _selectedReader == null)
            {
                string deviceList = "(none)";
                try
                {
                    var readers = ReaderCollection.GetReaders();
                    if (readers != null && readers.Count > 0)
                        deviceList = string.Join(", ", readers.Select(r => r.Description.Name));
                }
                catch (Exception ex)
                {
                    deviceList = $"Error listing devices: {ex.Message}";
                }
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized after reattempt.",
                    Error = $"No reader connected. Devices found: {deviceList}"
                };
            }

            // Lock the semaphore to ensure only one operation at a time
            await _semaphore.WaitAsync();
            try
            {
                // Capture fingerprint
                var captureResponse = await CaptureFingerprint();
                if (!captureResponse.Success)
                {
                    return captureResponse;
                }

                // Template is already included in the capture response
                // Add user ID to the response for identification
                captureResponse.Message = $"Fingerprint enrolled successfully for user {userId}.";
                
                return captureResponse;
            }
            finally
            {
                // Ensure proper release of the reader after operation
                // DisposeReader(); // REMOVED: Keep reader open for singleton
                // Ensure semaphore is released even if an exception occurs
                _semaphore.Release();
            }
        }

        [SupportedOSPlatform("windows")]
        public async Task<FingerprintResponse> CompareTemplates(string template1Xml, string template2Xml)
        {
            try
            {
                // Deserialize both templates
                Fmd? fmd1, fmd2;
                try
                {
                    fmd1 = Fmd.DeserializeXml(template1Xml);
                    fmd2 = Fmd.DeserializeXml(template2Xml);
                }
                catch (Exception ex)
                {
                    return new FingerprintResponse
                    {
                        Success = false,
                        Message = "Invalid template format.",
                        Error = $"Deserialization error: {ex.Message}"
                    };
                }

                // Compare the two templates
                var compareResult = await Task.Run(() => Comparison.Compare(fmd1, 0, fmd2, 0));
                
                if (compareResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    return new FingerprintResponse
                    {
                        Success = false,
                        Message = "Comparison failed.",
                        Error = $"Comparison error: {compareResult.ResultCode}"
                    };
                }

                // Check if fingerprints match (using FAR - False Accept Rate)
                const int PROBABILITY_ONE = 0x7FFFFFFF;
                int divisor = _farDivisor <= 0 ? 100000 : _farDivisor;
                int targetFAR = PROBABILITY_ONE / divisor;
                bool isMatch = compareResult.Score < targetFAR;

                return new FingerprintResponse
                {
                    Success = isMatch,
                    Message = isMatch ? $"Templates match (score: {compareResult.Score}/{targetFAR})." : $"Templates do not match (score: {compareResult.Score}/{targetFAR}).",
                    Quality = 0 // No quality for comparison only
                };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "An error occurred during template comparison.",
                    Error = ex.Message
                };
            }
        }

        public bool IsReaderConnected()
        {
            // Check if reader is initialized and not null
            return _isInitialized && _selectedReader != null;
        }

        public async Task ReinitializeReaderAsync()
        {
            try
            {
                Console.WriteLine("[FingerprintService] Requesting reinitialization...");
                
                // Signal cancellation to any running capture loops
                _cancellationRequested = true;
                
                if (_selectedReader != null)
                {
                    try { _selectedReader.CancelCapture(); } catch { }
                }

                // Wait for the semaphore to ensure no other operation is running
                // We'll wait up to 5 seconds for the current operation to finish/cancel
                if (await _semaphore.WaitAsync(5000))
                {
                    try
                    {
                        Console.WriteLine("[FingerprintService] Reinitializing fingerprint reader (Safe Mode)...");

                        // Close existing reader if any
                        if (_selectedReader != null)
                        {
                            Console.WriteLine("[FingerprintService] Disposing existing reader...");
                            DisposeReader();
                            _selectedReader = null;
                        }

                        _isInitialized = false;
                        
                        // Give the driver a moment to release the device
                        await Task.Delay(1500);

                        // Retry logic for reinitialization
                        for (int attempt = 1; attempt <= 3; attempt++)
                        {
                            Console.WriteLine($"[FingerprintService] Reinitialization attempt {attempt}...");

                            try
                            {
                                InitializeReader();

                                if (_isInitialized)
                                {
                                    Console.WriteLine("[FingerprintService] Fingerprint reader reinitialized successfully.");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[FingerprintService] Error during reinitialization attempt {attempt}: {ex.Message}");
                            }

                            // Wait before retrying
                            await Task.Delay(1000);
                        }

                        if (!_isInitialized)
                        {
                            // Log detected devices if reinitialization fails
                            string deviceList = "(none)";
                            try
                            {
                                var readers = ReaderCollection.GetReaders();
                                if (readers != null && readers.Count > 0)
                                    deviceList = string.Join(", ", readers.Select(r => r.Description.Name));
                            }
                            catch (Exception ex)
                            {
                                deviceList = $"Error listing devices: {ex.Message}";
                            }

                            Console.WriteLine($"[FingerprintService] Failed to reinitialize fingerprint reader after 3 attempts. Devices found: {deviceList}");
                        }
                    }
                    finally
                    {
                        _cancellationRequested = false;
                        _semaphore.Release();
                    }
                }
                else
                {
                    Console.WriteLine("[FingerprintService] Failed to acquire lock for reinitialization (Timeout).");
                    _cancellationRequested = false; // Reset flag even if we timed out
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FingerprintService] Critical error during reinitialization: {ex.Message}");
                _cancellationRequested = false;
            }
        }

        public void ResetReader()
        {
            try
            {
                Console.WriteLine("[FingerprintService] Resetting fingerprint reader...");

                // Dispose of the existing reader
                if (_selectedReader != null)
                {
                    Console.WriteLine("[FingerprintService] Disposing existing reader during reset...");
                    _selectedReader.Dispose();
                }

                _isInitialized = false;
                _selectedReader = null;

                // Attempt to reinitialize the reader
                InitializeReader();

                if (_isInitialized)
                {
                    Console.WriteLine("[FingerprintService] Fingerprint reader reset and reinitialized successfully.");
                }
                else
                {
                    Console.WriteLine("[FingerprintService] Failed to reset and reinitialize fingerprint reader.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FingerprintService] Error during reader reset: {ex.Message}");
            }
        }

        private void DisposeReader()
        {
            if (_selectedReader != null)
            {
                try
                {
                    _selectedReader.Dispose();
                }
                catch
                {
                    // Ignore errors on disposal
                }
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            DisposeReader();
        }
    }
}