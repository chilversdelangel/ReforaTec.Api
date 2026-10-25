using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ReforaTec.Api.Infrastructure.Ingress;

namespace ReforaTec.Api.IntegrationTests.Infrastructure.Ingress;

public class ClientIpResolverTests
{
    [Fact]
    public void Resolve_WhenConfiguredRealIpHeaderPresent_ShouldPrioritizeOverAllOtherHeaders()
    {
        // Arrange
        var options = Options.Create(new IngressOptions { RealIpHeader = "X-Custom-IP" });
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Request.Headers["X-Custom-IP"] = "198.51.100.99";
        context.Request.Headers["CF-Connecting-IP"] = "203.0.113.1";
        context.Request.Headers["X-Real-IP"] = "192.0.2.1";
        context.Request.Headers["X-Forwarded-For"] = "192.0.2.2";

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("198.51.100.99");
    }

    [Fact]
    public void Resolve_WhenCloudflareHeaderPresent_ShouldPrioritizeOverOtherProxyHeaders()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Request.Headers["CF-Connecting-IP"] = "203.0.113.195";
        context.Request.Headers["X-Real-IP"] = "198.51.100.2";
        context.Request.Headers["X-Forwarded-For"] = "192.0.2.1";

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("203.0.113.195");
    }

    [Fact]
    public void Resolve_WhenNginxRealIpPresentAndCloudflareAbsent_ShouldPrioritizeOverForwardedFor()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Request.Headers["X-Real-IP"] = "198.51.100.2";
        context.Request.Headers["X-Forwarded-For"] = "192.0.2.1";

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("198.51.100.2");
    }

    [Fact]
    public void Resolve_WhenForwardedForContainsMultipleIps_ShouldReturnFirstClientIpTrimmed()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Request.Headers["X-Forwarded-For"] = "  192.0.2.5 , 198.51.100.10, 203.0.113.1 ";

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("192.0.2.5");
    }

    [Fact]
    public void Resolve_WhenNoProxyHeadersPresent_ShouldFallbackToRemoteIpAddress()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("127.0.0.1");
    }

    [Fact]
    public void Resolve_WhenNoHeadersAndNoRemoteIp_ShouldReturnAnonymousFallback()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("anonymous");
    }

    [Fact]
    public void Resolve_WhenHeadersContainOnlyWhitespaceOrEmptyValues_ShouldFallbackToAnonymous()
    {
        // Arrange
        var options = Options.Create(new IngressOptions());
        
        var resolver = new ClientIpResolver(options);
        var context = new DefaultHttpContext();
        
        context.Request.Headers["X-Forwarded-For"] = "   ,   ";
        context.Request.Headers["CF-Connecting-IP"] = "   ";

        // Act
        var clientIp = resolver.Resolve(context);

        // Assert
        clientIp.Should().Be("anonymous");
    }
}
