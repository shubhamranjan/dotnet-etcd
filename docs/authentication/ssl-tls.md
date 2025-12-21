# SSL/TLS Support

`dotnet-etcd` supports connecting to secure etcd clusters using SSL/TLS. This includes support for self-signed certificates, which is common in development and testing environments.

## Prerequisite

-   A running etcd cluster with SSL/TLS enabled.
-   Valid certificates (CA, Server, Client - optional).

## Configuration

The `EtcdClient` provides an overload that accepts `Action<SslClientAuthenticationOptions>` to configure the underlying `SocketsHttpHandler.SslOptions`. This allows you to customize certificate validation, cipher suites, and ALPN protocols.

### Basic Usage (Trusted Certificates)

If your etcd server uses a certificate signed by a public CA (e.g., Let's Encrypt), connection is straightforward:

```csharp
EtcdClient client = new EtcdClient("https://etcd-host:2379");
```

### Self-Signed Certificates

For self-signed certificates, you typically need to:

1.  Trust the CA that signed the server certificate.
2.  Configure HTTP/2 ALPN (especially on Windows).

```csharp
EtcdClient client = new EtcdClient(
    "https://localhost:2379",
    configureSslOptions: sslOptions =>
    {
        // 1. Configure ALPN for gRPC (Critical for Windows)
        sslOptions.ApplicationProtocols = new List<SslApplicationProtocol> 
        { 
            SslApplicationProtocol.Http2 
        };

        // 2. Configure Certificate Validation
        sslOptions.RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
        {
             if (errors == SslPolicyErrors.None)
             {
                 return true;
             }
             
             // Setup custom validation logic here (e.g. check against local CA file)
             // Example:
             // var caCert = new X509Certificate2("path/to/ca.pem");
             // ... build chain ...
             
             // For development ONLY (Insecure):
             // return true; 
             return CustomValidator.Validate(cert, chain, errors);
        };
    }
);
```

## Docker Setup for SSL

To run an SSL-enabled etcd instance for testing, you can use the following `docker-compose.yml` snippet:

```yaml
services:
  etcd-ssl:
    image: quay.io/coreos/etcd:v3.5.21
    command: >
      /usr/local/bin/etcd
      --name etcd-ssl
      --listen-client-urls https://0.0.0.0:2379
      --advertise-client-urls https://localhost:2389
      --cert-file=/certs/server.pem
      --key-file=/certs/server-key.pem
      --client-cert-auth=false
    ports:
      - "2389:2379"
    volumes:
      - ./certs:/certs
```

### Generating Certificates

We recommend using standardized scripts to generate PKIX-compliant certificates that work across platforms (Linux, Windows/Schannel).

See `dotnet-etcd.Tests/generate-certs.sh` for a reference implementation using OpenSSL. Key requirements for certificates:

-   **Root CA**: Must have `basicConstraints=critical,CA:TRUE`.
-   **Server Cert**: Must have `extendedKeyUsage=serverAuth` and proper SANs (Subject Alternative Names).
