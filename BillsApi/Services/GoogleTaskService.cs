using BillsApi.Configuration;
using BillsApi.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BillsApi.Services
{
    public class GoogleTaskService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleTasksApiOptions _options;
        private string? _accessToken;

        // This static instance is created only once for the entire application lifetime.
        private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GoogleTaskService(HttpClient httpClient, IOptions<GoogleTasksApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);

            // Get the initial access token during startup
            _accessToken = GetNewAccessTokenAsync().Result;
        }

        private async Task<string?> GetNewAccessTokenAsync()
        {
            var tokenRequest = new
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret,
                refresh_token = _options.RefreshToken,
                grant_type = "refresh_token"
            };

            var tokenContent = new StringContent(JsonSerializer.Serialize(tokenRequest), Encoding.UTF8, "application/json");

            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenContent);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            return tokenResult.GetProperty("access_token").GetString();
        }

        public async Task UpdateTaskAsync(string taskId, GoogleTaskUpdateDto updateDto, string? taskListId = null)
        {
            if (string.IsNullOrEmpty(taskId)) return;

            if (_accessToken == null) return;

            // Set the Authorization header with the current access token
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var listId = taskListId ?? _options.DefaultTaskListId;
            var requestUrl = $"lists/{listId}/tasks/{taskId}";

            var jsonContent = JsonSerializer.Serialize(updateDto, s_serializerOptions);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(requestUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to update Google Task. Status: {response.StatusCode}. Content: {errorContent}");
            }
        }

        public async Task<GoogleTaskCreateDto?> CreateTaskAsync(string newTitle, DateTime dueDate, string? notes = null, string? taskListId = null)
        {
            if (_accessToken == null) return null;

            // Set the Authorization header with the current access token
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var listId = taskListId ?? _options.DefaultTaskListId;

            // The endpoint for creating a new task
            var requestUrl = $"lists/{listId}/tasks";

            // Create the payload from the input parameters
            var createPayload = new GoogleTaskCreateDto
            {
                Title = newTitle,
                Notes = notes,
                Due = dueDate
            };

            var jsonContent = JsonSerializer.Serialize(createPayload, s_serializerOptions);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Make the POST request
            var response = await _httpClient.PostAsync(requestUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create Google Task. Status: {response.StatusCode}. Content: {errorContent}");
            }

            // The API returns the newly created task object.
            // We can deserialize it and return it for use in our controller.
            var createdTask = await response.Content.ReadFromJsonAsync<GoogleTaskCreateDto>();
            return createdTask;
        }
    }
}
