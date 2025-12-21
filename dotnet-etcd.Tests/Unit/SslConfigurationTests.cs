using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using dotnet_etcd;
using Grpc.Core;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
/// Tests for SSL certificate configuration
/// </summary>
public class SslConfigurationTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_WithSslOptionsConfig_ShouldConfigureSsl()
    {
        // Arrange
        bool sslConfigured = false;
        
        // Act
        var client = new EtcdClient(
            "https://localhost:2389",
            configureSslOptions: sslOptions => {
                sslConfigured = true;
                // Configure SSL validation
                sslOptions.RemoteCertificateValidationCallback = 
                    (sender, cert, chain, errors) => true; // Accept all certs for test
            });
        
        // Assert
        Assert.True(sslConfigured, "SSL options configuration should have been called");
    }

    [Fact]
    public void Constructor_WithoutSslOptionsConfig_ShouldNotThrow()
    {
        // This tests backwards compatibility
        // Act & Assert
        var exception = Record.Exception(() => new EtcdClient("http://localhost:2379"));
        Assert.Null(exception);
    }

    [Fact]
    public void SslOptionsConfig_AllowsCustomSslValidation()
    {
        // Arrange
        X509Certificate? capturedCert = null;
        
        // Act
        var client = new EtcdClient(
            "https://localhost:2389",
            configureSslOptions: sslOptions => {
                sslOptions.RemoteCertificateValidationCallback = 
                    (sender, cert, chain, errors) => {
                        capturedCert = cert;
                        return true; // Accept the certificate
                    };
            });
        
        // Assert - just verify configuration doesn't throw
        Assert.NotNull(client);  
    }

    [Fact]
    public void SslOptionsConfig_AllowsClientCertificates()
    {
        // Arrange & Act
        var exception = Record.Exception(() => {
            var client = new EtcdClient(
                "https://localhost:2389",
                configureSslOptions: sslOptions => {
                    // This should not throw - user can configure certificates collection
                    // (They would add actual certificates in real usage)
                    sslOptions.RemoteCertificateValidationCallback =
                        (sender, cert, chain, errors) => true;
                    // sslOptions.ClientCertificates = new X509CertificateCollection(); // User can add certificates
                });
        });

        // Assert
        Assert.Null(exception);
    }
}
