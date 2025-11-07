using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Motix.Tests.Integration;

public class MovementsControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public MovementsControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static HttpRequestMessage PostWithKey(string url, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        req.Headers.Add("X-API-KEY", "motix-secret-key");
        return req;
    }
    private static HttpRequestMessage DeleteWithKey(string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Add("X-API-KEY", "motix-secret-key");
        return req;
    }
    private static async Task<Guid> ReadId(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateSector(string code)
    {
        var resp = await _client.SendAsync(PostWithKey("/api/v1/sectors", new { code }));
        resp.EnsureSuccessStatusCode();
        return await ReadId(resp);
    }

    private async Task<Guid> CreateMotorcycle(Guid sectorId, string plate)
    {
        var resp = await _client.SendAsync(PostWithKey("/api/v1/motorcycles", new { plate, sectorId }));
        resp.EnsureSuccessStatusCode();
        return await ReadId(resp);
    }

    [Fact]
    public async Task Post_InvalidFK_Returns400()
    {
        var resp = await _client.SendAsync(PostWithKey("/api/v1/movements", new
        {
            motorcycleId = Guid.NewGuid(),
            sectorId = Guid.NewGuid()
        }));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_List_Delete_Works()
    
    {
        var sectorA = await CreateSector("A1");
        var sectorB = await CreateSector("B1");
        var moto = await CreateMotorcycle(sectorA, "ABC1D23");

        // CREATE movement para setor B
        var create = await _client.SendAsync(PostWithKey("/api/v1/movements", new
        {
            motorcycleId = moto,
            sectorId = sectorB
        }));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadId(create);

        // LIST
        var list = await _client.GetAsync("/api/v1/movements?page=1&pageSize=10");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        // DELETE
        var del = await _client.SendAsync(DeleteWithKey($"/api/v1/movements/{id}"));
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
