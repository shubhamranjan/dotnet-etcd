// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;
using Grpc.Core;
using V3Electionpb;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        /// Campaign waits to acquire leadership in an election, returning a LeaderKey
        /// representing the leadership if successful. The LeaderKey can then be used
        /// to issue new values on the election, transactionally guard API requests on
        /// leadership still being held, and resign from the election.
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
            => CallEtcd((connection) => connection._electionClient.Campaign(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Campaign waits to acquire leadership in an election, returning a LeaderKey
        /// representing the leadership if successful. The LeaderKey can then be used
        /// to issue new values on the election, transactionally guard API requests on
        /// leadership still being held, and resign from the election.
        /// </summary>
        /// <param name="name">The name is the election’s identifier for the campaign.</param>
        /// <param name="lease">The lease is the ID of the lease attached to leadership of the election. If the lease expires or is revoked before resigning leadership, then the leadership is transferred to the next campaigner, if any.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public CampaignResponse Campaign(
            string name,
            long lease,
            Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => CallEtcd((connection) => connection._electionClient.Campaign(
                new CampaignRequest() {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Lease = lease,
                },
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// CampaignAsync waits to acquire leadership in an election, returning a LeaderKey
        /// representing the leadership if successful. The LeaderKey can then be used
        /// to issue new values on the election, transactionally guard API requests on
        /// leadership still being held, and resign from the election.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.CampaignAsync(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Campaign waits to acquire leadership in an election, returning a LeaderKey
        /// representing the leadership if successful. The LeaderKey can then be used
        /// to issue new values on the election, transactionally guard API requests on
        /// leadership still being held, and resign from the election.
        /// </summary>
        /// <param name="name">The name is the election’s identifier for the campaign.</param>
        /// <param name="lease">The lease is the ID of the lease attached to leadership of the election. If the lease expires or is revoked before resigning leadership, then the leadership is transferred to the next campaigner, if any.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<CampaignResponse> CampaignAsync(
            string name,
            long lease,
            Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => await CallEtcdAsync(async (connection) => await connection._electionClient.CampaignAsync(
                new CampaignRequest()
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name))),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Lease = lease,
                },
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Proclaim updates the leader's posted value with a new value.
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
            => CallEtcd((connection) => connection._electionClient.Proclaim(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Proclaim updates the leader's posted value with a new value.
        /// </summary>
        /// <param name="leader">The leader is the leadership hold on the election.</param>
        /// <param name="value">The value is an update meant to overwrite the leader’s current value.</param>
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
            => CallEtcd((connection) => connection._electionClient.Proclaim(
                new ProclaimRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Leader = leader ?? throw new ArgumentNullException(nameof(leader)),
                    Value = ByteString.CopyFromUtf8(value ?? throw new ArgumentNullException(nameof(value))),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Proclaim updates the leader's posted value with a new value.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.ProclaimAsync(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Proclaim updates the leader's posted value with a new value.
        /// </summary>
        /// <param name="leader">The leader is the leadership hold on the election.</param>
        /// <param name="value">The value is an update meant to overwrite the leader’s current value.</param>
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.ProclaimAsync(
                new ProclaimRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Leader = leader ?? throw new ArgumentNullException(nameof(leader)),
                    Value = ByteString.CopyFromUtf8(value ?? throw new ArgumentNullException(nameof(value))),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Leader returns the current election proclamation, if any.
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
            => CallEtcd((connection) => connection._electionClient.Leader(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Leader returns the current election proclamation, if any.
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
            => CallEtcd((connection) => connection._electionClient.Leader(
                new LeaderRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name)))
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Leader returns the current election proclamation, if any.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.LeaderAsync(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Leader returns the current election proclamation, if any.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.LeaderAsync(
                new LeaderRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name)))
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Observe streams election proclamations in-order as made by the election's
        /// elected leaders.
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
            => CallEtcd((connection) => connection._electionClient.Observe(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Observe streams election proclamations in-order as made by the election's
        /// elected leaders.
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
            => CallEtcd((connection) => connection._electionClient.Observe(
                new LeaderRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Name = ByteString.CopyFromUtf8(name ?? throw new ArgumentNullException(nameof(name)))
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Observe streams election proclamations in-order as made by the election's
        /// elected leaders.
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
            using (AsyncServerStreamingCall<LeaderResponse> leaderResponse = _connection._electionClient.Observe(
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
        /// Observe streams election proclamations in-order as made by the election's
        /// elected leaders.
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
            using (AsyncServerStreamingCall<LeaderResponse> leaderResponse = _connection._electionClient.Observe(
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
        /// Resign releases election leadership so other campaigners may acquire
        /// leadership on the election.
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
            => CallEtcd((connection) => connection._electionClient.Resign(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Resign releases election leadership so other campaigners may acquire
        /// leadership on the election.
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
            => CallEtcd((connection) => connection._electionClient.Resign(
                new ResignRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Leader = leader ?? throw new ArgumentNullException(nameof(leader))
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken));

        /// <summary>
        /// Resign releases election leadership so other campaigners may acquire
        /// leadership on the election.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.ResignAsync(
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                request ?? throw new ArgumentNullException(nameof(request)),
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Resign releases election leadership so other campaigners may acquire
        /// leadership on the election.
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
            => await CallEtcdAsync(async (connection) => await connection._electionClient.ResignAsync(
                new ResignRequest
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
                    Leader = leader ?? throw new ArgumentNullException(nameof(leader))
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
                },
                headers,
                deadline,
                cancellationToken)).ConfigureAwait(false);
    }
}
