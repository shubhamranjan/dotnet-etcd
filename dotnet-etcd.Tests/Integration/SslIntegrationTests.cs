using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace dotnet_etcd.Tests.Integration
{
    [Collection("Auth tests")] // Run sequentially to avoid port conflicts or resource contention
    [Trait("Category", "Integration")]
    public class SslIntegrationTests : IDisposable
    {
        private readonly EtcdClient _client;
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public SslIntegrationTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
            // Load the CA certificate
            // Ensure certs are copied to output directory via csproj
            var caCertPath = System.IO.Path.Combine(AppContext.BaseDirectory, "certs", "ca.pem");
            var caCert = X509CertificateLoader.LoadCertificateFromFile(caCertPath);

            // Connect to the SSL-enabled etcd instance exposed on port 2389
            _client = new EtcdClient(
                "https://localhost:2389",
                configureSslOptions: sslOptions =>
                {
                    // Configure validation callback to trust our custom CA
                    sslOptions.RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
                    {
                        _output.WriteLine($"Validation callback entered. Errors: {errors}");
                        
                        if (errors == System.Net.Security.SslPolicyErrors.None)
                        {
                            return true;
                        }

                        // If the only error is RemoteCertificateChainErrors (untrusted root),
                        // verify it validates against our loaded CA.
                        if (errors == System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors)
                        {
                            using var customChain = new X509Chain();
                            customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                            // Add our CA to the extra store so the chain can be built
                            customChain.ChainPolicy.ExtraStore.Add(caCert);
                            // Allow untrusted root because our CA is not in the system store
                            customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

                            if (cert is null) return false;
                            
                            bool isChainValid = customChain.Build((X509Certificate2)cert);
                            
                            if (!isChainValid)
                            {
                                var statuses = string.Join(", ", System.Linq.Enumerable.Select(customChain.ChainStatus, s => s.StatusInformation));
                                throw new Exception($"Certificate validation failed. Chain statuses: {statuses}");
                            }

                            // Ensure the chain roots to our CA
                            var root = customChain.ChainElements[customChain.ChainElements.Count - 1].Certificate;
                            if (root.Thumbprint != caCert.Thumbprint)
                            {
                                throw new Exception($"Certificate chain root mismatch. Expected {caCert.Thumbprint}, got {root.Thumbprint}");
                            }
                            
                            return true;
                        }

                        return false;
                    };
                        
                    // Explicitly set ALPN to HTTP/2 - CRITICAL for gRPC on Windows
                    sslOptions.ApplicationProtocols = new System.Collections.Generic.List<System.Net.Security.SslApplicationProtocol>
                    {
                        System.Net.Security.SslApplicationProtocol.Http2
                    };
                });
        }

        [Fact]
        public async Task SslConnection_ShouldSucceed_AndPerformOperations()
        {
            // Arrange
            var key = "ssl-test-key";
            var value = "ssl-test-value";

            // Act & Assert
            
            // 1. Put
            var putResponse = await _client.PutAsync(key, value);
            Assert.NotNull(putResponse);

            // 2. Get
            var getVal = await _client.GetValAsync(key);
            Assert.Equal(value, getVal);

            // 3. Delete
            await _client.DeleteAsync(key);
            getVal = await _client.GetValAsync(key);
            Assert.Equal(string.Empty, getVal);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
