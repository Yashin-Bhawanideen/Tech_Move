using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TechMove.API.IntegrationTests
{
    public static class TestHelper
    {
        public static StringContent GetJsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public static bool IsValidJwtToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var parts = token.Split('.');
            return parts.Length == 3;
        }

        public static async Task<string> GetAuthToken(HttpClient client)
        {
            var loginData = new { username = "testuser", password = "testpass" };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await DeserializeResponse<Dictionary<string, string>>(response);
                return result?["token"];
            }
            return null;
        }
    }
}
 
