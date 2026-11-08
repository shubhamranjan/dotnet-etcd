using System;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd;

public partial class EtcdClient
{
    /// <summary>
    ///     MemberAdd adds a member into the cluster
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberAddResponse MemberAdd(MemberAddRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.ClusterClient
        .MemberAdd(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MemberAddAsync adds a member into the cluster in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .ClusterClient
        .MemberAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     MemberRemove removes an existing member from the cluster
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.ClusterClient
        .MemberRemove(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MemberRemoveAsync removes an existing member from the cluster in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .ClusterClient
        .MemberRemoveAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     MemberUpdate updates the member configuration
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.ClusterClient
        .MemberUpdate(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MemberUpdateAsync updates the member configuration in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .ClusterClient
        .MemberUpdateAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     MemberList lists all the members in the cluster
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberListResponse MemberList(MemberListRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.ClusterClient
        .MemberList(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MemberListAsync lists all the members in the cluster in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberListResponse> MemberListAsync(MemberListRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .ClusterClient
        .MemberListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     MemberPromote promotes a learner member to a voting member of the cluster.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberPromoteResponse MemberPromote(MemberPromoteRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.ClusterClient
        .MemberPromote(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     MemberPromoteAsync promotes a learner member to a voting member of the cluster in async.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberPromoteResponse> MemberPromoteAsync(MemberPromoteRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .ClusterClient
        .MemberPromoteAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     MemberPromote promotes a learner member to a voting member of the cluster.
    /// </summary>
    /// <param name="memberId">The ID of the learner member to promote.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberPromoteResponse MemberPromote(ulong memberId, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        MemberPromote(new MemberPromoteRequest { ID = memberId }, headers, deadline, cancellationToken);

    /// <summary>
    ///     MemberPromoteAsync promotes a learner member to a voting member of the cluster in async.
    /// </summary>
    /// <param name="memberId">The ID of the learner member to promote.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberPromoteResponse> MemberPromoteAsync(ulong memberId, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default) =>
        await MemberPromoteAsync(new MemberPromoteRequest { ID = memberId }, headers, deadline, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    ///     MemberAddAsLearner adds a non-voting learner member to the cluster. Promote it with
    ///     <see cref="MemberPromote(ulong, Metadata, DateTime?, CancellationToken)" /> once it has caught up.
    /// </summary>
    /// <param name="peerURLs">The peer URLs of the new learner member.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public MemberAddResponse MemberAddAsLearner(string[] peerURLs, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default) =>
        MemberAdd(new MemberAddRequest { PeerURLs = { peerURLs ?? throw new ArgumentNullException(nameof(peerURLs)) }, IsLearner = true },
            headers, deadline, cancellationToken);

    /// <summary>
    ///     MemberAddAsLearnerAsync adds a non-voting learner member to the cluster in async. Promote it with
    ///     <see cref="MemberPromoteAsync(ulong, Metadata, DateTime?, CancellationToken)" /> once it has caught up.
    /// </summary>
    /// <param name="peerURLs">The peer URLs of the new learner member.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<MemberAddResponse> MemberAddAsLearnerAsync(string[] peerURLs, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default) =>
        await MemberAddAsync(
                new MemberAddRequest { PeerURLs = { peerURLs ?? throw new ArgumentNullException(nameof(peerURLs)) }, IsLearner = true },
                headers, deadline, cancellationToken)
            .ConfigureAwait(false);
}
