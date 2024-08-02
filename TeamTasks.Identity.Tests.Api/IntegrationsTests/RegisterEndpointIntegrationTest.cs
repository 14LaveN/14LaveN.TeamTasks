using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TeamTasks.Common.Tests.Abstractions;

namespace TeamTasks.Identity.Tests.Api.IntegrationsTests;

/// <summary>
/// Represents the register endpoint <see cref="BaseIntegrationTest"/>
/// </summary>
public sealed class RegisterEndpointIntegrationTest
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    
    public RegisterEndpointIntegrationTest()
    {
        // Arrange
        _server = new TestServer(new WebHostBuilder()
            .UseStartup<Program>());
        _client = _server.CreateClient();
    }
    [Fact]
    public async Task ReturnHelloWorld()
    {
        // Act
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal("Hello World!", responseString);
    }

}