using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using V3Electionpb;

namespace dotnet_etcd;

public partial class EtcdClient
{
    /// <summary>
    ///     Campaign waits to acquire leadership in an election, returning a LeaderKey
    ///     representing the leadership if successful. The LeaderKey can then be used
    ///     to issue new values on the election, transactionally guard API requests on
    ///     leadership still being held, and resign from the election.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public CampaignResponse Campaign(
        CampaignRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Campaign(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Campaign waits to acquire leadership in an election, returning a LeaderKey
    ///     representing the leadership if successful. The LeaderKey can then be used
    ///     to issue new values on the election, transactionally guard API requests on
    ///     leadership still being held, and resign from the election.
    /// </summary>
    /// <param name="name">The name is the election�s identifier for the campaign.</param>
    /// <param name="value">The value is the initial proclaimed value set when the campaigner wins the election.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public CampaignResponse Campaign(
        string name,
        string value = null,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Campaign(
            new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))),
                Value = value == null ? null : ByteString.CopyFromUtf8(value)
            },
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     CampaignAsync waits to acquire leadership in an election, returning a LeaderKey
    ///     representing the leadership if successful. The LeaderKey can then be used
    ///     to issue new values on the election, transactionally guard API requests on
    ///     leadership still being held, and resign from the election.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<CampaignResponse> CampaignAsync(
        CampaignRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.CampaignAsync(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Campaign waits to acquire leadership in an election, returning a LeaderKey
    ///     representing the leadership if successful. The LeaderKey can then be used
    ///     to issue new values on the election, transactionally guard API requests on
    ///     leadership still being held, and resign from the election.
    /// </summary>
    /// <param name="name">The name is the election�s identifier for the campaign.</param>
    /// <param name="value">The value is the initial proclaimed value set when the campaigner wins the election.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<CampaignResponse> CampaignAsync(
        string name,
        string value,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.CampaignAsync(
            new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))),
                Value = value == null ? null : ByteString.CopyFromUtf8(value)
            },
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Proclaim updates the leader's posted value with a new value.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public ProclaimResponse Proclaim(
        ProclaimRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Proclaim(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Proclaim updates the leader's posted value with a new value.
    /// </summary>
    /// <param name="leader">The leader is the leadership hold on the election.</param>
    /// <param name="value">The value is an update meant to overwrite the leader�s current value.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public ProclaimResponse Proclaim(
        LeaderKey leader,
        string value,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Proclaim(
            new ProclaimRequest
            {
                Leader = leader ?? throw new ArgumentNullException(nameof(leader)),
                Value = ByteString.CopyFromUtf8(value ?? throw new ArgumentNullException(nameof(value)))
            },
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Proclaim updates the leader's posted value with a new value.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<ProclaimResponse> ProclaimAsync(
        ProclaimRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.ProclaimAsync(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Proclaim updates the leader's posted value with a new value.
    /// </summary>
    /// <param name="leader">The leader is the leadership hold on the election.</param>
    /// <param name="value">The value is an update meant to overwrite the leader�s current value.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<ProclaimResponse> ProclaimAsync(
        LeaderKey leader,
        string value,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.ProclaimAsync(
            new ProclaimRequest
            {
                Leader = leader ?? throw new ArgumentNullException(nameof(leader)),
                Value = ByteString.CopyFromUtf8(value ?? throw new ArgumentNullException(nameof(value)))
            },
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Leader returns the current election proclamation, if any.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public LeaderResponse Leader(
        LeaderRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Leader(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Leader returns the current election proclamation, if any.
    /// </summary>
    /// <param name="name">The name is the election identifier for the leadership information.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public LeaderResponse Leader(
        string name,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Leader(
            new LeaderRequest { Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))) },
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Leader returns the current election proclamation, if any.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<LeaderResponse> LeaderAsync(
        LeaderRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.LeaderAsync(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Leader returns the current election proclamation, if any.
    /// </summary>
    /// <param name="name">The name is the election identifier for the leadership information.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<LeaderResponse> LeaderAsync(
        string name,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.LeaderAsync(
            new LeaderRequest { Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))) },
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Observe streams election proclamations in-order as made by the election's
    ///     elected leaders.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async IAsyncEnumerable<LeaderResponse> ObserveAsync(
        LeaderRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (AsyncServerStreamingCall<LeaderResponse> leaderResponse = _connection.ElectionClient.Observe(
                   request ?? throw new ArgumentNullException(nameof(request)),
                   headers,
                   deadline,
                   cancellationToken))
        {
            while (await leaderResponse.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
            {
                yield return leaderResponse.ResponseStream.Current;
            }
        }
    }

    /// <summary>
    ///     Observe streams election proclamations in-order as made by the election's
    ///     elected leaders.
    /// </summary>
    /// <param name="name">The name is the election identifier for the leadership information.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async IAsyncEnumerable<LeaderResponse> ObserveAsync(
        string name,
        Metadata headers = null,
        DateTime? deadline = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (AsyncServerStreamingCall<LeaderResponse> leaderResponse = _connection.ElectionClient.Observe(
                   new LeaderRequest
                   {
                       Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name)))
                   },
                   headers,
                   deadline,
                   cancellationToken))
        {
            while (await leaderResponse.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
            {
                yield return leaderResponse.ResponseStream.Current;
            }
        }
    }

    /// <summary>
    ///     Resign releases election leadership so other campaigners may acquire
    ///     leadership on the election.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public ResignResponse Resign(
        ResignRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Resign(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Resign releases election leadership so other campaigners may acquire
    ///     leadership on the election.
    /// </summary>
    /// <param name="leader">The leader is the leadership to relinquish by resignation.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public ResignResponse Resign(
        LeaderKey leader,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Resign(
            new ResignRequest { Leader = leader ?? throw new ArgumentNullException(nameof(leader)) },
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Resign releases election leadership so other campaigners may acquire
    ///     leadership on the election.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<ResignResponse> ResignAsync(
        ResignRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.ResignAsync(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Resign releases election leadership so other campaigners may acquire
    ///     leadership on the election.
    /// </summary>
    /// <param name="leader">The leader is the leadership to relinquish by resignation.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<ResignResponse> ResignAsync(
        LeaderKey leader,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => await CallEtcdAsync(async connection => await connection.ElectionClient.ResignAsync(
            new ResignRequest { Leader = leader ?? throw new ArgumentNullException(nameof(leader)) },
            headers,
            deadline,
            cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     Observe streams election proclamations in-order as made by the election's
    ///     elected leaders.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AsyncServerStreamingCall<LeaderResponse> Observe(
        LeaderRequest request,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Observe(
            request ?? throw new ArgumentNullException(nameof(request)),
            headers,
            deadline,
            cancellationToken));

    /// <summary>
    ///     Observe streams election proclamations in-order as made by the election's
    ///     elected leaders.
    /// </summary>
    /// <param name="name">The name is the election identifier for the leadership information.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AsyncServerStreamingCall<LeaderResponse> Observe(
        string name,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => CallEtcd(connection => connection.ElectionClient.Observe(
            new LeaderRequest { Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))) },
            headers,
            deadline,
            cancellationToken));
}
