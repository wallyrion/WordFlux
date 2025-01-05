using FluentAssertions;
using WordFlux.Contracts;
using Wordflux.Tests.Integration.Extensions;

namespace Wordflux.Tests.Integration.Tests;

[Collection(nameof(SharedTestCollection))]
public class ProtectionKeysPersistenceTests(DockerFixtures fixtures)
{
    [Fact]
    public async Task Register_And_Login_From_Separate_Application_Instances_Should_Succeed()
    {
        var app1 = new IntegrationTestWebFactory(fixtures);
        var app2 = new IntegrationTestWebFactory(fixtures);

        var client1 = app1.CreateDefaultClient();
        var client2 = app2.CreateDefaultClient();
        
        
        var registerDto = new { email = "test@gmail.com", password = "Qwerty1234!" };
        var registerResponse = await client1.PostAsJsonAsync("register", registerDto);
        registerResponse.Should().BeSuccessful();

        var loginResponse = await client2.PostAsJsonAsync("login?useCookies=false&useSessionCookies=false", registerDto).WaitForJson<AuthResponse>();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }
}
