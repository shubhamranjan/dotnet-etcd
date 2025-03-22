// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace dotnet_etcd.DependencyInjection;

/// <summary>
///     Validates etcd client options.
/// </summary>
internal static class EtcdClientOptionsValidator
{
    /// <summary>
    ///     Validates the specified options.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if options is null.</exception>
    /// <exception cref="ArgumentException">Thrown if options is invalid.</exception>
    public static void ValidateOptions(EtcdClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("ConnectionString must be provided", nameof(options));
        }

        // Validate port is in a valid range
        if (options.Port is <= 0 or > 65535)
        {
            throw new ArgumentException($"Port must be between 1 and 65535, but was {options.Port}", nameof(options));
        }

        // Validate server name if using static hosts format
        if (options.ConnectionString.StartsWith("static://", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(options.ServerName))
        {
            throw new ArgumentException("ServerName must be provided when using static:// connection string format",
                nameof(options));
        }
    }
}
