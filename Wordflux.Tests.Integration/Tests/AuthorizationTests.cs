
using FluentAssertions;
using Wordflux.Tests.Integration.Containers;
using Wordflux.Tests.Integration.TestFixture;

namespace Wordflux.Tests.Integration.Tests;

public class AuthorizationTests(DockerFixtures fixtures) : BaseIntegrationTest(fixtures)
{
    [Fact]
    public async Task Register_Should_Success()
    {
        var registerDto = new { email = "test@gmail.com", password = "Qwerty1234!" };
        var registerResponse = await ApiClient.PostAsJsonAsync("register", registerDto);

        registerResponse.Should().Be200Ok();

        var login = await ApiClient.PostAsJsonAsync("login?useCookies=false&useSessionCookies=false", registerDto);
        login.Should().Be200Ok();
    }
    
    
    [Theory]
    [InlineData("", "Qwerty!1234")]
    [InlineData("qwerty", "Qwerty!1234")]
    [InlineData("qwerty@gmail.com", "QWERTY!1234")]
    [InlineData("qwerty@gmail.com", "")]
    [InlineData("qwerty@gmail.com", "q")]
    public async Task Register_Should_BadRequest_When_Request_Not_Valid(string email, string password)
    {
        var registerDto = new { email, password };
        var registerResponse = await ApiClient.PostAsJsonAsync("register", registerDto);

        registerResponse.Should().Be400BadRequest();
    }
    
    [Fact]
    public async Task Register_Should_BadRequest_When_Email_Already_Exists()
    {
        var registerDto = new { email = "test@gmail.com", password = "Qwerty1234!" };
        var registerResponse = await ApiClient.PostAsJsonAsync("register", registerDto);
        registerResponse.Should().Be200Ok();
        
        var registerResponseDuplicate = await ApiClient.PostAsJsonAsync("register", registerDto);

        registerResponseDuplicate.Should().Be400BadRequest();
    }
    
    [Theory]
    [InlineData("", "")]
    [InlineData("test@gmail.com", "1232131")]
    [InlineData("qwerty@gmail.com", "Qwerty1234!")]
    public async Task Login_Should_Unauthorized_When_Invalid_Request(string email, string password)
    {
        var registerDto = new { email = "test@gmail.com", password = "Qwerty1234!" };
        var registerResponse = await ApiClient.PostAsJsonAsync("register", registerDto);
        registerResponse.Should().Be200Ok();
        
        var login = await ApiClient.PostAsJsonAsync("login?useCookies=false&useSessionCookies=false", new {email, password});
        login.Should().Be401Unauthorized();
    }
}