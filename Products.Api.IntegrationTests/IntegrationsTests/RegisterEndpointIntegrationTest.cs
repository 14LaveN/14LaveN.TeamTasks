using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Products.Api.IntegrationTests.IntegrationsTests;

public sealed class RegisterEndpointIntegrationTest
{
    [Fact]
    public async Task Create()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>();
        var httpClient = webApplicationFactory.CreateClient();
        var sc = httpClient.BaseAddress;
    }

}