// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using dotnet_etcd.interfaces;
using dotnet_etcd.multiplexer;

using Etcdserverpb;
using V3Lockpb;

namespace dotnet_etcd
{
    /// <summary>
    /// Etcd client is the entrypoint for this library.
    /// It contains all the functions required to perform operations on etcd.
    /// </summary>
    internal partial class EtcdClient : IEtcdClient
    {
        private readonly Connection _connection;

        public EtcdClient(
            KV.KVClient kvClient,
            Watch.WatchClient watchClient,
            Lease.LeaseClient leaseClient,
            Lock.LockClient lockClient,
            Cluster.ClusterClient clusterClient,
            Maintenance.MaintenanceClient maintenanceClient,
            Auth.AuthClient authClient)
        {
            _connection = new Connection
            {
                _kvClient = kvClient,
                _watchClient = watchClient,
                _leaseClient = leaseClient,
                _lockClient = lockClient,
                _clusterClient = clusterClient,
                _maintenanceClient = maintenanceClient,
                _authClient = authClient
            };
        }
    }
}
