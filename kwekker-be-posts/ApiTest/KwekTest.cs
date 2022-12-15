using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ApiTest;

public class KwekTest
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly KwekkerContext _context;

    public KwekTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        var scope = _factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        _context = scopedServices.GetRequiredService<KwekkerContext>();
    }

    [Fact]
    public async Task GetKweksReturnsSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/kweks");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task GetKweksReturnsCorrectAmountOfKweks()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/kweks");
        var responseString =
            await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());

        var expectedAmount = await _context.Kweks.CountAsync();

        Assert.Equal(expectedAmount, responseString.GetArrayLength());
    }

    [Fact]
    public async Task GetKwekReturnsSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/kweks/1");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType!.ToString());
    }

    [Fact]
    public async Task GetKwekReturnsCorrectKwek()
    {
        var client = _factory.CreateClient();
        var kwek = await _context.Kweks.FirstAsync();

        var response = await client.GetAsync($"/kweks/{kwek.Id}");
        var responseString =
            await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());

        Assert.Equal(kwek.Id, responseString.GetProperty("id").GetInt32());
        Assert.Equal(kwek.Text, responseString.GetProperty("text").GetString());
        Assert.True(kwek.PostedAt.Equals(DateTime.Parse(responseString.GetProperty("postedAt").GetString()!)));
    }

    [Fact]
    public async Task PostKwekWithoutAuthorizationReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/kweks",
            new StringContent("{\"kwek\": \"test\"}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateKwekWithoutAuthorizationReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsync("/kweks/1",
            new StringContent("{\"kwek\": \"test\"}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteKwekWithoutAuthorizationReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync("/kweks/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostValidKwekReturnsCreated()
    {
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
            })
            .CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsync("/kweks", new StringContent(JsonSerializer.Serialize(new
        {
            text = "Foo"
        }), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(
        "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public async Task PostInvalidKwekTextReturnsBadRequest(string kwekText)
    {
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
            })
            .CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsync("/kweks", new StringContent(JsonSerializer.Serialize(new
        {
            text = kwekText
        }), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public async Task PostWithoutKwekTextReturnsBadRequest()
    {
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
            })
            .CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsync("/kweks", new StringContent(JsonSerializer.Serialize(new
        {
            foo = "bar"
        }), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutKwekAuthorizedReturnsNoContent()
    {
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
            })
            .CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        
        var kwekToEdit = new Kwek
        {
            Text = "foo",
            PostedAt = DateTime.Now,
            User = await _context.Users.FirstAsync()
        };

        _context.Kweks.Add(kwekToEdit);
        await _context.SaveChangesAsync();

        var response = await client.PutAsync($"/kweks/{kwekToEdit.Id}", new StringContent(JsonSerializer.Serialize(new
        {
            text = "Bar"
        }), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteKwekAuthorizedReturnsNoContent()
    {
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
            })
            .CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        
        var kwekToDelete = new Kwek
        {
            Text = "foo",
            PostedAt = DateTime.Now,
            User = await _context!.Users.FirstAsync()
        };

        _context.Kweks.Add(kwekToDelete);
        await _context.SaveChangesAsync();

        var response = await client.DeleteAsync($"/kweks/{kwekToDelete.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}