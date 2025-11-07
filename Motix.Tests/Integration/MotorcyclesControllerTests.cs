using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Motix.Tests.Integration;

public class MotorcyclesControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public MotorcyclesControllerTests(CustomWebAppFactory factory)
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
    private static HttpRequestMessage PutWithKey(string url, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
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

    private async Task<Guid> CreateSector(string code = "A1")
    {
        var resp = await _client.SendAsync(PostWithKey("/api/v1/sectors", new { code }));
        resp.EnsureSuccessStatusCode();
        return await ReadId(resp);
    }

    [Fact]
    public async Task Post_InvalidSector_Returns400()
    {
        var resp = await _client.SendAsync(PostWithKey("/api/v1/motorcycles", new
        {
            plate = "ABC1D23",
            sectorId = Guid.NewGuid()
        }));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FullFlow_Create_Update_Delete_Works()
    {
        var sectorId = await CreateSector("S1");

        // CREATE
        var create = await _client.SendAsync(PostWithKey("/api/v1/motorcycles", new
        {
            plate = "ABC1D23",
            sectorId
        }));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadId(create);

        // GET BY ID
        var got = await _client.GetAsync($"/api/v1/motorcycles/{id}");
        got.StatusCode.Should().Be(HttpStatusCode.OK);

        // UPDATE (troca placa e setor)
        var newSectorId = await CreateSector("S2");
        var update = await _client.SendAsync(PutWithKey($"/api/v1/motorcycles/{id}", new
        {
            plate = "DEF4G56",
            sectorId = newSectorId
        }));
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // DELETE
        var del = await _client.SendAsync(DeleteWithKey($"/api/v1/motorcycles/{id}"));
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
