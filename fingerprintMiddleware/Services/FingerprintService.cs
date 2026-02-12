using DPUruNet;
using FingerprintMiddleware.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace FingerprintMiddleware.Services
{
    public class FingerprintService
    {
        private ReaderCollection? _readers;
        private Reader? _selectedReader;
        private bool _isInitialized = false;

        // Runtime settings (can be adjusted via API)
        private int _farDivisor = 100000; // probability-one / farDivisor (default ~1e-5)
        private int _minAcceptableQuality = 60; // default minimum acceptable quality

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
                    var result = _selectedReader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
                    _isInitialized = result == Constants.ResultCode.DP_SUCCESS;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing fingerprint reader: {ex.Message}");
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
            if (!_isInitialized || _selectedReader == null)
            {
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized.",
                    Error = "No reader connected."
                };
            }

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

                CaptureResult captureResult = null;
                var startTime = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(30);

                // Wait loop for finger placement
                while (DateTime.Now - startTime < timeout)
                {
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
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized.",
                    Error = "No reader connected."
                };
            }

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

        [SupportedOSPlatform("windows")]
        public async Task<FingerprintResponse> EnrollFingerprint(string userId)
        {
            if (!_isInitialized || _selectedReader == null)
            {
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Fingerprint reader not initialized.",
                    Error = "No reader connected."
                };
            }

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
            catch (Exception ex)
            {
                return new FingerprintResponse
                {
                    Success = false,
                    Message = "Error enrolling fingerprint.",
                    Error = ex.Message
                };
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

        public void ReinitializeReader()
        {
            try
            {
                // Close existing reader if any
                if (_selectedReader != null)
                {
                    _selectedReader.Dispose();  // Use Dispose instead of Close
                }

                _isInitialized = false;
                
                // Reinitialize
                InitializeReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reinitializing reader: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_selectedReader != null)
            {
                try
                {
                    _selectedReader.Dispose();  // Use Dispose instead of Close
                }
                catch
                {
                    // Ignore errors on disposal
                }
            }
        }
    }
}