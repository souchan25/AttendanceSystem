using System.Text;
using Newtonsoft.Json;
using AttendanceWeb.Models;

namespace AttendanceWeb.Services
{
    public class FingerprintApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FingerprintApiClient(string baseUrl = "http://localhost:5000")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<FingerprintResponse> GetDeviceInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/fingerprint/device-info");
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(content) 
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<FingerprintResponse> CaptureAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/fingerprint/capture", null);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(content)
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<FingerprintResponse> EnrollAsync(string userId)
        {
            try
            {
                var data = new FingerprintData { UserId = userId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/fingerprint/enroll", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(responseContent)
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<FingerprintResponse> VerifyAsync(string template)
        {
            try
            {
                var data = new FingerprintData { Template = template };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/fingerprint/verify", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(responseContent)
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<FingerprintResponse> ReinitializeAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/fingerprint/reinitialize", null);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(content)
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<FingerprintResponse> CompareTemplatesAsync(string template1, string template2)

        {
            try
            {
                var data = new { Template1 = template1, Template2 = template2 };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/fingerprint/compare", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FingerprintResponse>(responseContent)
                    ?? new FingerprintResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new FingerprintResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/fingerprint/status");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
