// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace dotnet_etcd.DependencyInjection;

/// <summary>
/// Options for configuring an EtcdClient instance.
/// </summary>
public class EtcdClientOptions
{
    /// <summary>
    /// Gets or sets the connection string for the etcd server.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the port to connect to. Default is 2379.
    /// </summary>
    public int Port { get; set; } = 2379;

    /// <summary>
    /// Gets or sets the server name. Default is "my-etcd-server".
    /// </summary>
    public string ServerName { get; set; } = "my-etcd-server";

    /// <summary>
    /// Gets or sets the action to configure the gRPC channel options.
    /// </summary>
    public Action<GrpcChannelOptions> ConfigureChannel { get; set; }

    /// <summary>
    /// Gets or sets the interceptors to apply to gRPC calls.
    /// </summary>
    public Interceptor[] Interceptors { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use insecure credentials (no SSL).
    /// Setting this to true will automatically configure the channel with ChannelCredentials.Insecure.
    /// </summary>
    public bool UseInsecureChannel { get; set; }
    
    /// <summary>
    /// Gets or sets the authorization credentials. If set, this will be used to create the channel credentials.
    /// </summary>
    public CallCredentials CallCredentials { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable retry policy.
    /// </summary>
    public bool EnableRetryPolicy { get; set; } = true;
    
    /// <summary>
    /// Applies the options to the channel options.
    /// </summary>
    /// <param name="options">The gRPC channel options to configure.</param>
    internal void ApplyTo(GrpcChannelOptions options)
    {
        if (UseInsecureChannel)
        {
            options.Credentials = ChannelCredentials.Insecure;
        }
        else if (CallCredentials != null)
        {
            var channelCredentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials);
            options.Credentials = channelCredentials;
        }
        
        // Apply custom configuration if provided
        ConfigureChannel?.Invoke(options);
    }
}