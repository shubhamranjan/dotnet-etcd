using Etcdserverpb;
using V3Lockpb;

namespace dotnet_etcd.multiplexer
{
    internal class Connection
    {
        internal KV.KVClient kvClient;

        internal Watch.WatchClient watchClient;

        internal Lease.LeaseClient leaseClient;

        internal Lock.LockClient lockClient;

        internal Cluster.ClusterClient clusterClient;

        internal Maintenance.MaintenanceClient maintenanceClient;

        internal Auth.AuthClient authClient;
    }
}
