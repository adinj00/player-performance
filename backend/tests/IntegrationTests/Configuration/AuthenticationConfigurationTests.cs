using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.IntegrationTests.Configuration;

public sealed class AuthenticationConfigurationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public AuthenticationConfigurationTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void IdentityServices_ShouldBeResolvable_FromApplicationServiceProvider()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetService<UserManager<ApplicationUser>>());
        Assert.NotNull(services.GetService<SignInManager<ApplicationUser>>());
        Assert.NotNull(services.GetService<IPasswordHasher<ApplicationUser>>());
    }

    [Fact]
    public void IdentityOptions_ShouldUseExpectedSecurityBaseline()
    {
        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<IdentityOptions>>()
            .Value;

        Assert.True(options.Password.RequireDigit);
        Assert.True(options.Password.RequireLowercase);
        Assert.True(options.Password.RequireUppercase);
        Assert.True(options.Password.RequireNonAlphanumeric);
        Assert.Equal(12, options.Password.RequiredLength);
        Assert.Equal(4, options.Password.RequiredUniqueChars);

        Assert.True(options.Lockout.AllowedForNewUsers);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);

        Assert.True(options.User.RequireUniqueEmail);
        Assert.False(options.SignIn.RequireConfirmedAccount);
        Assert.False(options.SignIn.RequireConfirmedEmail);
        Assert.False(options.SignIn.RequireConfirmedPhoneNumber);
    }

    [Fact]
    public async Task CookieAuthenticationOptions_ShouldReturnApiFriendlyStatusCodes_ForRedirectEvents()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var authenticationOptions = optionsMonitor.Get(IdentityConstants.ApplicationScheme);

        Assert.Equal("player-performance.auth", authenticationOptions.Cookie.Name);
        Assert.True(authenticationOptions.Cookie.HttpOnly);
        Assert.Equal("/", authenticationOptions.Cookie.Path);
        Assert.Equal(SameSiteMode.Lax, authenticationOptions.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, authenticationOptions.Cookie.SecurePolicy);

        var authenticationScheme = new AuthenticationScheme(
            IdentityConstants.ApplicationScheme,
            IdentityConstants.ApplicationScheme,
            typeof(CookieAuthenticationHandler));

        var loginHttpContext = CreateHttpContext(services);
        var loginRedirectUri = "https://example.test/login";
        var loginProperties = new AuthenticationProperties();
        var loginContext = new RedirectContext<CookieAuthenticationOptions>(
            loginHttpContext,
            authenticationScheme,
            authenticationOptions,
            loginProperties,
            loginRedirectUri);

        await authenticationOptions.Events.RedirectToLogin(loginContext);

        Assert.Equal(StatusCodes.Status401Unauthorized, loginHttpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", loginHttpContext.Response.ContentType);
        await AssertProblemDetailsAsync(loginHttpContext, StatusCodes.Status401Unauthorized, "Unauthorized");

        var deniedHttpContext = CreateHttpContext(services);
        var deniedRedirectUri = "https://example.test/access-denied";
        var deniedProperties = new AuthenticationProperties();
        var deniedContext = new RedirectContext<CookieAuthenticationOptions>(
            deniedHttpContext,
            authenticationScheme,
            authenticationOptions,
            deniedProperties,
            deniedRedirectUri);

        await authenticationOptions.Events.RedirectToAccessDenied(deniedContext);

        Assert.Equal(StatusCodes.Status403Forbidden, deniedHttpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", deniedHttpContext.Response.ContentType);
        await AssertProblemDetailsAsync(deniedHttpContext, StatusCodes.Status403Forbidden, "Forbidden");
    }

    [Fact]
    public void AntiforgeryOptions_ShouldUseExpectedCookieAndHeaderConfiguration()
    {
        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<AntiforgeryOptions>>()
            .Value;

        Assert.Equal("X-CSRF-TOKEN", options.HeaderName);
        Assert.Equal("XSRF-TOKEN", options.Cookie.Name);
        Assert.False(options.Cookie.HttpOnly);
        Assert.Equal("/", options.Cookie.Path);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
        Assert.True(options.Cookie.IsEssential);
    }

    private static async Task AssertProblemDetailsAsync(
        HttpContext httpContext,
        int expectedStatusCode,
        string expectedTitle)
    {
        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);

        Assert.Equal(expectedStatusCode, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, document.RootElement.GetProperty("title").GetString());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
    }

    private static DefaultHttpContext CreateHttpContext(IServiceProvider services)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(new HttpResponseFeature());
        context.RequestServices = services;
        context.Response.Body = new MemoryStream();

        return context;
    }
}
