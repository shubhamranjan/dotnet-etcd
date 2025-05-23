﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd;

public partial class EtcdClient
{
    /// <summary>
    ///     Alarm activates, deactivates, and queries alarms regarding cluster health
    /// </summary>
    /// <param name="request">Alarm request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Alarm Response</returns>
    public AlarmResponse Alarm(AlarmRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .Alarm(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     Alarm activates, deactivates, and queries alarms regarding cluster health in async
    /// </summary>
    /// <param name="request">Alarm request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Alarm Response</returns>
    public async Task<AlarmResponse> AlarmAsync(AlarmRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .AlarmAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Status gets the status of the member.
    /// </summary>
    /// <param name="request">Status Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Status response</returns>
    public StatusResponse Status(StatusRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .Status(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     Status gets the status of the member in async.
    /// </summary>
    /// <param name="request">Status Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Status response</returns>
    public async Task<StatusResponse> StatusAsync(StatusRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .StatusAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Defragment defragments a member's backend database to recover storage space.
    /// </summary>
    /// <param name="request">Defragment Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Defragment Response</returns>
    public DefragmentResponse Defragment(DefragmentRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .Defragment(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     Defragment defragments a member's backend database to recover storage space in async.
    /// </summary>
    /// <param name="request">Defragment Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Defragment Response</returns>
    public async Task<DefragmentResponse> DefragmentAsync(DefragmentRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .DefragmentAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Hash computes the hash of whole backend keyspace,
    ///     including key, lease, and other buckets in storage.
    ///     This is designed for testing ONLY!
    ///     Do not rely on this in production with ongoing transactions,
    ///     since Hash operation does not hold MVCC locks.
    ///     Use "HashKV" API instead for "key" bucket consistency checks.
    /// </summary>
    /// <param name="request">Hash Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Hash Response</returns>
    public HashResponse Hash(HashRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .Hash(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     Hash computes the hash of whole backend keyspace,
    ///     including key, lease, and other buckets in storage in async.
    ///     This is designed for testing ONLY!
    ///     Do not rely on this in production with ongoing transactions,
    ///     since Hash operation does not hold MVCC locks.
    ///     Use "HashKV" API instead for "key" bucket consistency checks.
    /// </summary>
    /// <param name="request">Hash Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>Hash Response</returns>
    public async Task<HashResponse> HashAsync(HashRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .HashAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     HashKV computes the hash of all MVCC keys up to a given revision.
    ///     It only iterates "key" bucket in backend storage.
    /// </summary>
    /// <param name="request">HashKV Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>HashKV Response</returns>
    public HashKVResponse HashKV(HashKVRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .HashKV(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     HashKV computes the hash of all MVCC keys up to a given revision in async.
    ///     It only iterates "key" bucket in backend storage.
    /// </summary>
    /// <param name="request">HashKV Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>HashKV Response</returns>
    public async Task<HashKVResponse> HashKVAsync(HashKVRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .HashKVAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="method"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    public async Task Snapshot(SnapshotRequest request, Action<SnapshotResponse> method,
        CancellationToken cancellationToken, Metadata headers = null, DateTime? deadline = null) => await CallEtcdAsync(
        async connection =>
        {
            using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = connection
                       .MaintenanceClient.Snapshot(request, headers, deadline, cancellationToken))
            {
                while (await snapshotter.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    SnapshotResponse update = snapshotter.ResponseStream.Current;
                    method(update);
                }
            }
        }).ConfigureAwait(false);

    /// <summary>
    ///     Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="methods"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    public async Task Snapshot(SnapshotRequest request, Action<SnapshotResponse>[] methods,
        CancellationToken cancellationToken, Metadata headers = null, DateTime? deadline = null) => await CallEtcdAsync(
        async connection =>
        {
            using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = connection
                       .MaintenanceClient.Snapshot(request, headers, deadline, cancellationToken))
            {
                while (await snapshotter.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    SnapshotResponse update = snapshotter.ResponseStream.Current;
                    foreach (Action<SnapshotResponse> method in methods)
                    {
                        method(update);
                    }
                }
            }
        }).ConfigureAwait(false);

    /// <summary>
    ///     MoveLeader requests current leader node to transfer its leadership to transferee.
    /// </summary>
    /// <param name="request">MoveLeader Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>MoveLeader Response</returns>
    public MoveLeaderResponse MoveLeader(MoveLeaderRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.MaintenanceClient
        .MoveLeader(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MoveLeader requests current leader node to transfer its leadership to transferee in async.
    /// </summary>
    /// <param name="request">MoveLeader Request</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>MoveLeader Response</returns>
    public async Task<MoveLeaderResponse> MoveLeaderAsync(MoveLeaderRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .MaintenanceClient
        .MoveLeaderAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
}
