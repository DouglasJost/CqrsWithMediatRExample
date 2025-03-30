using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CqrsWithMediatR.API;
using CqrsWithMediatR.Tests.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CqrsWithMediatR.Tests.Integration
{
    public class ProductControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_WithoutToken_Should_Return_Unauthorized()
        {
            var response = await _client.GetAsync("/api/products");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_With_Valid_Token_Should_Return_Ok()
        {
            var token = await TestUtilities.GetJwtTokenAsync(_client);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

