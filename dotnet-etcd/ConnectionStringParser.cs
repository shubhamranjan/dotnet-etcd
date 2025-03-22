using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Grpc.Core;

namespace dotnet_etcd;

/// <summary>
///     Handles parsing of connection strings for etcd client
/// </summary>
public class ConnectionStringParser
{
    private const string InsecurePrefix = "http://";
    private const string SecurePrefix = "https://";
    private const string StaticHostsPrefix = "static://";
    private const string DnsPrefix = "dns://";
    private const string AlternateDnsPrefix = "discovery-srv://";

    /// <summary>
    ///     Parses a connection string and returns the appropriate URI and connection type
    /// </summary>
    /// <param name="connectionString">The connection string to parse</param>
    /// <param name="port">The port to use if not specified in the connection string</param>
    /// <param name="credentials">The credentials to use for the connection</param>
    /// <returns>A tuple containing the parsed URI and whether it's a DNS connection</returns>
    public (Uri[] Uris, bool IsDnsConnection) ParseConnectionString(string connectionString, int port, ChannelCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        if (connectionString.StartsWith(AlternateDnsPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            connectionString = connectionString.Substring(AlternateDnsPrefix.Length);
            connectionString = DnsPrefix + connectionString;
        }

        if (connectionString.StartsWith(DnsPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            return (new[] { new Uri(connectionString) }, true);
        }

        string[] hosts = connectionString.Split(',');
        List<Uri> nodes = new();

        foreach (string host in hosts)
        {
            string processedHost = host.Trim();

            // Only append port if no port is specified and it's not a full URL
            if (!processedHost.Contains(':') &&
                !processedHost.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) &&
                !processedHost.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                processedHost += $":{Convert.ToString(port, CultureInfo.InvariantCulture)}";
            }

            if (!(processedHost.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) ||
                  processedHost.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase)))
            {
                processedHost = credentials == ChannelCredentials.Insecure
                    ? $"{InsecurePrefix}{processedHost}"
                    : $"{SecurePrefix}{processedHost}";
            }

            nodes.Add(new Uri(processedHost));
        }

        return (nodes.ToArray(), false);
    }
}
