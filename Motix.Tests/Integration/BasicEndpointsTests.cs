using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Motix.Application.DTOs;
using Xunit;

namespace Motix.Tests.Integration;

public class BasicEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;

    public BasicEndpointsTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var resp = await _client.GetAsync("/health");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Sectors_ReturnsOk()
    {
        var resp = await _client.GetAsync("/api/v1/sectors");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_Sector_RequiresApiKey()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/sectors", new { code = "A1" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Sector_WithApiKey_Creates()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sectors")
        {
            Content = JsonContent.Create(new { code = "A1" })
        };
        req.Headers.Add("X-API-KEY", "motix-secret-key");

        var resp = await _client.SendAsync(req);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task MlPredict_RequiresApiKey()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/ml/predict", new MovementFeaturesDto(5, 2, 8));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MlPredict_WithApiKey_ReturnsOk()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ml/predict")
        {
            Content = JsonContent.Create(new MovementFeaturesDto(6, 2, 8))
        };
        req.Headers.Add("X-API-KEY", "motix-secret-key");

        var resp = await _client.SendAsync(req);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<MovementPredictionDto>();
        body.Should().NotBeNull();
    }
}
