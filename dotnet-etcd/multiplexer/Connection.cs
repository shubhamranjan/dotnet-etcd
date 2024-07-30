// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Etcdserverpb;

using Grpc.Core;

using V3Electionpb;

using V3Lockpb;

namespace dotnet_etcd.multiplexer
{
    internal sealed class Connection
    {
        private readonly CallInvoker _callInvoker;

        internal Connection(CallInvoker callInvoker)
        {
            _callInvoker = callInvoker;
        }

        private KV.KVClient _kvClient;

        private Watch.WatchClient _watchClient;

        private Lease.LeaseClient _leaseClient;

        private Lock.LockClient _lockClient;

        private Cluster.ClusterClient _clusterClient;

        private Maintenance.MaintenanceClient _maintenanceClient;

        private Auth.AuthClient _authClient;

        private Election.ElectionClient _electionClient;

        internal KV.KVClient KVClient => _kvClient ??= new KV.KVClient(_callInvoker);

        internal Watch.WatchClient WatchClient => _watchClient ??= new Watch.WatchClient(_callInvoker);

        internal Lease.LeaseClient LeaseClient => _leaseClient ??= new Lease.LeaseClient(_callInvoker);

        internal Lock.LockClient LockClient => _lockClient ??= new Lock.LockClient(_callInvoker);

        internal Cluster.ClusterClient ClusterClient => _clusterClient ??= new Cluster.ClusterClient(_callInvoker);

        internal Maintenance.MaintenanceClient MaintenanceClient => _maintenanceClient ??= new Maintenance.MaintenanceClient(_callInvoker);

        internal Auth.AuthClient AuthClient => _authClient ??= new Auth.AuthClient(_callInvoker);

        internal Election.ElectionClient ElectionClient => _electionClient ??= new Election.ElectionClient(_callInvoker);
    }
}
