// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace dotnet_etcd.options
{
    public class EtcdOptions
    {
        public string ConnectionString { get; set; }

        public string ServerName { get; set; }

        public int Port { get; set; }
    }
}
