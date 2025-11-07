using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Motix.Tests.Integration;

public class SectorsControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public SectorsControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static async Task<Guid> ReadIdFromResponse(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    private static HttpRequestMessage PostWithKey(string url, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        { Content = JsonContent.Create(body) };
        req.Headers.Add("X-API-KEY", "motix-secret-key");
        return req;
    }

    private static HttpRequestMessage PutWithKey(string url, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, url)
        { Content = JsonContent.Create(body) };
        req.Headers.Add("X-API-KEY", "motix-secret-key");
        return req;
    }

    private static HttpRequestMessage DeleteWithKey(string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Add("X-API-KEY", "motix-secret-key");
        return req;
    }

    [Fact]
    public async Task Get_List_ReturnsOk()
    {
        var resp = await _client.GetAsync("/api/v1/sectors");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_WithoutApiKey_Returns401()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/sectors", new { code = "A1" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Get_Put_Delete_FullFlow_Works()
    {
        // CREATE
        var create = await _client.SendAsync(PostWithKey("/api/v1/sectors", new { code = "A1" }));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadIdFromResponse(create);

        // GET BY ID
        var got = await _client.GetAsync($"/api/v1/sectors/{id}");
        got.StatusCode.Should().Be(HttpStatusCode.OK);

        // UPDATE
        var update = await _client.SendAsync(PutWithKey($"/api/v1/sectors/{id}", new { code = "B2" }));
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // DELETE
        var del = await _client.SendAsync(DeleteWithKey($"/api/v1/sectors/{id}"));
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // NOT FOUND after delete
        var notFound = await _client.GetAsync($"/api/v1/sectors/{id}");
        notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
