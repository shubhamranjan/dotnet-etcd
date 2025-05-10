using Etcdserverpb;
using V3Electionpb;
using V3Lockpb;

namespace dotnet_etcd.interfaces;

/// <summary>
///     Interface for Connection to make it testable
/// </summary>
public interface IConnection
{
    /// <summary>
    ///     Gets the KV client
    /// </summary>
    KV.KVClient KVClient { get; }

    /// <summary>
    ///     Gets the Watch client
    /// </summary>
    Watch.WatchClient WatchClient { get; }

    /// <summary>
    ///     Gets the Lease client
    /// </summary>
    Lease.LeaseClient LeaseClient { get; }

    /// <summary>
    ///     Gets the Lock client
    /// </summary>
    Lock.LockClient LockClient { get; }

    /// <summary>
    ///     Gets the Cluster client
    /// </summary>
    Cluster.ClusterClient ClusterClient { get; }

    /// <summary>
    ///     Gets the Maintenance client
    /// </summary>
    Maintenance.MaintenanceClient MaintenanceClient { get; }

    /// <summary>
    ///     Gets the Auth client
    /// </summary>
    Auth.AuthClient AuthClient { get; }

    /// <summary>
    ///     Gets the Election client
    /// </summary>
    Election.ElectionClient ElectionClient { get; }
}
