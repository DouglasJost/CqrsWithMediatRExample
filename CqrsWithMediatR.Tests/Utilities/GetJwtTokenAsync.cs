using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Tests.Utilities
{
    public static class TestUtilities
    {
        public static async Task<string> GetJwtTokenAsync(HttpClient client)
        {
            var payload = new
            {
                login = "QATestUser",
                password = "GoodbyeRubyTuesday"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/authentication/authenticate", content);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var tokenObj = JsonDocument.Parse(json);
            return tokenObj.RootElement.GetProperty("token").GetString()!;
        }
    }
}
