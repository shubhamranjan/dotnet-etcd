using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;
using V3Electionpb;
using V3Lockpb;

namespace dotnet_etcd.multiplexer;

internal sealed class Connection : IConnection
{
    private readonly CallInvoker _callInvoker;

    private Auth.AuthClient _authClient;

    private Cluster.ClusterClient _clusterClient;

    private Election.ElectionClient _electionClient;

    private KV.KVClient _kvClient;

    private Lease.LeaseClient _leaseClient;

    private Lock.LockClient _lockClient;

    private Maintenance.MaintenanceClient _maintenanceClient;

    private Watch.WatchClient _watchClient;

    internal Connection(CallInvoker callInvoker) => _callInvoker = callInvoker;

    internal KV.KVClient KVClient => _kvClient ??= new KV.KVClient(_callInvoker);

    internal Watch.WatchClient WatchClient => _watchClient ??= new Watch.WatchClient(_callInvoker);

    internal Lease.LeaseClient LeaseClient => _leaseClient ??= new Lease.LeaseClient(_callInvoker);

    internal Lock.LockClient LockClient => _lockClient ??= new Lock.LockClient(_callInvoker);

    internal Cluster.ClusterClient ClusterClient => _clusterClient ??= new Cluster.ClusterClient(_callInvoker);

    internal Maintenance.MaintenanceClient MaintenanceClient =>
        _maintenanceClient ??= new Maintenance.MaintenanceClient(_callInvoker);

    internal Auth.AuthClient AuthClient => _authClient ??= new Auth.AuthClient(_callInvoker);

    internal Election.ElectionClient ElectionClient => _electionClient ??= new Election.ElectionClient(_callInvoker);

    KV.KVClient IConnection.KVClient => KVClient;

    Watch.WatchClient IConnection.WatchClient => WatchClient;

    Lease.LeaseClient IConnection.LeaseClient => LeaseClient;

    Lock.LockClient IConnection.LockClient => LockClient;

    Cluster.ClusterClient IConnection.ClusterClient => ClusterClient;

    Maintenance.MaintenanceClient IConnection.MaintenanceClient => MaintenanceClient;

    Auth.AuthClient IConnection.AuthClient => AuthClient;

    Election.ElectionClient IConnection.ElectionClient => ElectionClient;
}
