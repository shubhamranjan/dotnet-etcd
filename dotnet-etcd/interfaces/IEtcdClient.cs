using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Grpc.Core;
using V3Electionpb;
using V3Lockpb;

namespace dotnet_etcd.interfaces;

/// <summary>
///     The public surface of <see cref="EtcdClient" />: every etcd RPC wrapper, in sync and async form.
///     Resolve it from dependency injection or create an <see cref="EtcdClient" /> directly; dispose it
///     when done. Member documentation is inherited from <see cref="EtcdClient" />.
/// </summary>
public interface IEtcdClient : IDisposable
{
    /// <inheritdoc cref="EtcdClient.Alarm(AlarmRequest, Metadata, DateTime?, CancellationToken)" />
    AlarmResponse Alarm(AlarmRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AlarmAsync(AlarmRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AlarmResponse> AlarmAsync(AlarmRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthDisable(AuthDisableRequest, Metadata, DateTime?, CancellationToken)" />
    AuthDisableResponse AuthDisable(AuthDisableRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthDisableAsync(AuthDisableRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthEnable(AuthEnableRequest, Metadata, DateTime?, CancellationToken)" />
    AuthEnableResponse AuthEnable(AuthEnableRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthEnableAsync(AuthEnableRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthStatus(AuthStatusRequest, Metadata, DateTime?, CancellationToken)" />
    AuthStatusResponse AuthStatus(AuthStatusRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthStatusAsync(AuthStatusRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthStatusResponse> AuthStatusAsync(AuthStatusRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Authenticate(AuthenticateRequest, Metadata, DateTime?, CancellationToken)" />
    AuthenticateResponse Authenticate(AuthenticateRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.AuthenticateAsync(AuthenticateRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Compact(CompactionRequest, Metadata, DateTime?, CancellationToken)" />
    CompactionResponse Compact(CompactionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.CompactAsync(CompactionRequest, Metadata, DateTime?, CancellationToken)" />
    Task<CompactionResponse> CompactAsync(CompactionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Campaign(CampaignRequest, Metadata, DateTime?, CancellationToken)" />
    CampaignResponse Campaign(CampaignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Campaign(string, string, Metadata, DateTime?, CancellationToken)" />
    [Obsolete(
        "Campaign without an explicit lease gives up leadership after ~60 seconds with no signal " +
        "to the caller, because etcd creates and orphans a session of its own. Grant a lease, keep " +
        "it alive with LeaseKeepAlive, and pass its id to Campaign(name, leaseId, value, ...).")]
    CampaignResponse Campaign(string name, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Campaign(string, long, string, Metadata, DateTime?, CancellationToken)" />
    CampaignResponse Campaign(string name, long leaseId, string value = null, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.CampaignAsync(CampaignRequest, Metadata, DateTime?, CancellationToken)" />
    Task<CampaignResponse> CampaignAsync(CampaignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.CampaignAsync(string, string, Metadata, DateTime?, CancellationToken)" />
    [Obsolete(
        "CampaignAsync without an explicit lease gives up leadership after ~60 seconds with no " +
        "signal to the caller, because etcd creates and orphans a session of its own. Grant a lease, " +
        "keep it alive with LeaseKeepAlive, and pass its id to CampaignAsync(name, leaseId, value, ...).")]
    Task<CampaignResponse> CampaignAsync(string name, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.CampaignAsync(string, long, string, Metadata, DateTime?, CancellationToken)" />
    Task<CampaignResponse> CampaignAsync(string name, long leaseId, string value = null, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Defragment(DefragmentRequest, Metadata, DateTime?, CancellationToken)" />
    DefragmentResponse Defragment(DefragmentRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DefragmentAsync(DefragmentRequest, Metadata, DateTime?, CancellationToken)" />
    Task<DefragmentResponse> DefragmentAsync(DefragmentRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Downgrade(DowngradeRequest, Metadata, DateTime?, CancellationToken)" />
    DowngradeResponse Downgrade(DowngradeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DowngradeAsync(DowngradeRequest, Metadata, DateTime?, CancellationToken)" />
    Task<DowngradeResponse> DowngradeAsync(DowngradeRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Delete(DeleteRangeRequest, Metadata, DateTime?, CancellationToken)" />
    DeleteRangeResponse Delete(DeleteRangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Delete(string, Metadata, DateTime?, CancellationToken)" />
    DeleteRangeResponse Delete(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DeleteAsync(DeleteRangeRequest, Metadata, DateTime?, CancellationToken)" />
    Task<DeleteRangeResponse> DeleteAsync(DeleteRangeRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DeleteAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<DeleteRangeResponse> DeleteAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DeleteRange(string, Metadata, DateTime?, CancellationToken)" />
    DeleteRangeResponse DeleteRange(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.DeleteRangeAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<DeleteRangeResponse> DeleteRangeAsync(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Get(RangeRequest, Metadata, DateTime?, CancellationToken)" />
    RangeResponse Get(RangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Get(string, Metadata, DateTime?, CancellationToken)" />
    RangeResponse Get(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetAsync(RangeRequest, Metadata, DateTime?, CancellationToken)" />
    Task<RangeResponse> GetAsync(RangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<RangeResponse> GetAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetRange(string, Metadata, DateTime?, CancellationToken)" />
    RangeResponse GetRange(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetRangeAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<RangeResponse> GetRangeAsync(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RangeStream(RangeRequest, Action{RangeResponse}, CancellationToken, Metadata, DateTime?)" />
    Task RangeStream(RangeRequest request, Action<RangeResponse> method, CancellationToken cancellationToken,
        Metadata headers = null, DateTime? deadline = null);

    /// <inheritdoc cref="EtcdClient.RangeStreamAsync(RangeRequest, Metadata, DateTime?, CancellationToken)" />
    Task<RangeResponse> RangeStreamAsync(RangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetRangeVal(string, Metadata, DateTime?, CancellationToken)" />
    IDictionary<string, string> GetRangeVal(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetRangeValAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<IDictionary<string, string>> GetRangeValAsync(string prefixKey, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetVal(string, Metadata, DateTime?, CancellationToken)" />
    string GetVal(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.GetValAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<string> GetValAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Hash(HashRequest, Metadata, DateTime?, CancellationToken)" />
    HashResponse Hash(HashRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.HashAsync(HashRequest, Metadata, DateTime?, CancellationToken)" />
    Task<HashResponse> HashAsync(HashRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.HashKV(HashKVRequest, Metadata, DateTime?, CancellationToken)" />
    HashKVResponse HashKV(HashKVRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.HashKVAsync(HashKVRequest, Metadata, DateTime?, CancellationToken)" />
    Task<HashKVResponse> HashKVAsync(HashKVRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseGrant(LeaseGrantRequest, Metadata, DateTime?, CancellationToken)" />
    LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseGrantAsync(LeaseGrantRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(LeaseKeepAliveRequest, Action{LeaseKeepAliveResponse}, CancellationToken, Metadata)" />
    Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method,
        CancellationToken cancellationToken, Metadata headers = null);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(LeaseKeepAliveRequest, Action{LeaseKeepAliveResponse}[], CancellationToken, Metadata)" />
    Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods,
        CancellationToken cancellationToken, Metadata headers = null);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(LeaseKeepAliveRequest[], Action{LeaseKeepAliveResponse}, CancellationToken, Metadata)" />
    Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method,
        CancellationToken cancellationToken, Metadata headers = null);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(LeaseKeepAliveRequest[], Action{LeaseKeepAliveResponse}[], CancellationToken, Metadata, DateTime?)" />
    Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods,
        CancellationToken cancellationToken, Metadata headers = null, DateTime? deadline = null);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(CancellationTokenSource, long, int, int?, Metadata, DateTime?)" />
    Task LeaseKeepAlive(CancellationTokenSource cancellationTokenSource, long leaseId, int keepAliveTimeout = 1000,
        int? communicationTimeout = null, Metadata headers = null, DateTime? deadline = null);

    /// <inheritdoc cref="EtcdClient.LeaseKeepAlive(long, CancellationToken)" />
    Task LeaseKeepAlive(long leaseId, CancellationToken cancellationToken);

    /// <inheritdoc cref="EtcdClient.LeaseRevoke(LeaseRevokeRequest, Metadata, DateTime?, CancellationToken)" />
    LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseLeases(LeaseLeasesRequest, Metadata, DateTime?, CancellationToken)" />
    LeaseLeasesResponse LeaseLeases(LeaseLeasesRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseLeases(Metadata, DateTime?, CancellationToken)" />
    LeaseLeasesResponse LeaseLeases(Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseLeasesAsync(LeaseLeasesRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LeaseLeasesResponse> LeaseLeasesAsync(LeaseLeasesRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseLeasesAsync(Metadata, DateTime?, CancellationToken)" />
    Task<LeaseLeasesResponse> LeaseLeasesAsync(Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseRevokeAsync(LeaseRevokeRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseTimeToLive(LeaseTimeToLiveRequest, Metadata, DateTime?, CancellationToken)" />
    LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaseTimeToLiveAsync(LeaseTimeToLiveRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Lock(LockRequest, Metadata, DateTime?, CancellationToken)" />
    LockResponse Lock(LockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Lock(string, Metadata, DateTime?, CancellationToken)" />
    [Obsolete(
        "Lock without an explicit lease releases the lock after ~60 seconds with no signal to the " +
        "caller, because etcd creates and orphans a session of its own. Grant a lease, keep it " +
        "alive with LeaseKeepAlive, and pass its id to Lock(name, leaseId, ...).")]
    LockResponse Lock(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Lock(string, long, Metadata, DateTime?, CancellationToken)" />
    LockResponse Lock(string name, long leaseId, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LockAsync(LockRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LockResponse> LockAsync(LockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LockAsync(string, Metadata, DateTime?, CancellationToken)" />
    [Obsolete(
        "LockAsync without an explicit lease releases the lock after ~60 seconds with no signal to " +
        "the caller, because etcd creates and orphans a session of its own. Grant a lease, keep it " +
        "alive with LeaseKeepAlive, and pass its id to LockAsync(name, leaseId, ...).")]
    Task<LockResponse> LockAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LockAsync(string, long, Metadata, DateTime?, CancellationToken)" />
    Task<LockResponse> LockAsync(string name, long leaseId, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Leader(LeaderRequest, Metadata, DateTime?, CancellationToken)" />
    LeaderResponse Leader(LeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Leader(string, Metadata, DateTime?, CancellationToken)" />
    LeaderResponse Leader(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaderAsync(LeaderRequest, Metadata, DateTime?, CancellationToken)" />
    Task<LeaderResponse> LeaderAsync(LeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.LeaderAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<LeaderResponse> LeaderAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberAdd(MemberAddRequest, Metadata, DateTime?, CancellationToken)" />
    MemberAddResponse MemberAdd(MemberAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberAddAsync(MemberAddRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberAddAsLearner(string[], Metadata, DateTime?, CancellationToken)" />
    MemberAddResponse MemberAddAsLearner(string[] peerURLs, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberAddAsLearnerAsync(string[], Metadata, DateTime?, CancellationToken)" />
    Task<MemberAddResponse> MemberAddAsLearnerAsync(string[] peerURLs, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberList(MemberListRequest, Metadata, DateTime?, CancellationToken)" />
    MemberListResponse MemberList(MemberListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberListAsync(MemberListRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MemberListResponse> MemberListAsync(MemberListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberPromote(MemberPromoteRequest, Metadata, DateTime?, CancellationToken)" />
    MemberPromoteResponse MemberPromote(MemberPromoteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberPromote(ulong, Metadata, DateTime?, CancellationToken)" />
    MemberPromoteResponse MemberPromote(ulong memberId, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberPromoteAsync(MemberPromoteRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MemberPromoteResponse> MemberPromoteAsync(MemberPromoteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberPromoteAsync(ulong, Metadata, DateTime?, CancellationToken)" />
    Task<MemberPromoteResponse> MemberPromoteAsync(ulong memberId, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberRemove(MemberRemoveRequest, Metadata, DateTime?, CancellationToken)" />
    MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberRemoveAsync(MemberRemoveRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberUpdate(MemberUpdateRequest, Metadata, DateTime?, CancellationToken)" />
    MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MemberUpdateAsync(MemberUpdateRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MoveLeader(MoveLeaderRequest, Metadata, DateTime?, CancellationToken)" />
    MoveLeaderResponse MoveLeader(MoveLeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.MoveLeaderAsync(MoveLeaderRequest, Metadata, DateTime?, CancellationToken)" />
    Task<MoveLeaderResponse> MoveLeaderAsync(MoveLeaderRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ObserveAsync(LeaderRequest, Metadata, DateTime?, CancellationToken)" />
    IAsyncEnumerable<LeaderResponse> ObserveAsync(LeaderRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ObserveAsync(string, Metadata, DateTime?, CancellationToken)" />
    IAsyncEnumerable<LeaderResponse> ObserveAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Proclaim(ProclaimRequest, Metadata, DateTime?, CancellationToken)" />
    ProclaimResponse Proclaim(ProclaimRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Proclaim(LeaderKey, string, Metadata, DateTime?, CancellationToken)" />
    ProclaimResponse Proclaim(LeaderKey leader, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ProclaimAsync(ProclaimRequest, Metadata, DateTime?, CancellationToken)" />
    Task<ProclaimResponse> ProclaimAsync(ProclaimRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ProclaimAsync(LeaderKey, string, Metadata, DateTime?, CancellationToken)" />
    Task<ProclaimResponse> ProclaimAsync(LeaderKey leader, string value, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Resign(ResignRequest, Metadata, DateTime?, CancellationToken)" />
    ResignResponse Resign(ResignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Resign(LeaderKey, Metadata, DateTime?, CancellationToken)" />
    ResignResponse Resign(LeaderKey leader, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ResignAsync(ResignRequest, Metadata, DateTime?, CancellationToken)" />
    Task<ResignResponse> ResignAsync(ResignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.ResignAsync(LeaderKey, Metadata, DateTime?, CancellationToken)" />
    Task<ResignResponse> ResignAsync(LeaderKey leader, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Put(PutRequest, Metadata, DateTime?, CancellationToken)" />
    PutResponse Put(PutRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Put(string, string, Metadata, DateTime?, CancellationToken)" />
    PutResponse Put(string key, string val, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.PutAsync(PutRequest, Metadata, DateTime?, CancellationToken)" />
    Task<PutResponse> PutAsync(PutRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.PutAsync(string, string, Metadata, DateTime?, CancellationToken)" />
    Task<PutResponse> PutAsync(string key, string val, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleAdd(AuthRoleAddRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleAddAsync(AuthRoleAddRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleDelete(AuthRoleDeleteRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleDeleteAsync(AuthRoleDeleteRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleGet(AuthRoleGetRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleGetResponse RoleGet(AuthRoleGetRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleGetAsync(AuthRoleGetRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleGetResponse> RoleGetAsync(AuthRoleGetRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleGrantPermission(AuthRoleGrantPermissionRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleGrantPermissionAsync(AuthRoleGrantPermissionRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(AuthRoleGrantPermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleList(AuthRoleListRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleListResponse RoleList(AuthRoleListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleListAsync(AuthRoleListRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleRevokePermission(AuthRoleRevokePermissionRequest, Metadata, DateTime?, CancellationToken)" />
    AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.RoleRevokePermissionAsync(AuthRoleRevokePermissionRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(AuthRoleRevokePermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Snapshot(SnapshotRequest, Action{SnapshotResponse}, CancellationToken, Metadata, DateTime?)" />
    Task Snapshot(SnapshotRequest request, Action<SnapshotResponse> method, CancellationToken cancellationToken,
        Metadata headers = null, DateTime? deadline = null);

    /// <inheritdoc cref="EtcdClient.Snapshot(SnapshotRequest, Action{SnapshotResponse}[], CancellationToken, Metadata, DateTime?)" />
    Task Snapshot(SnapshotRequest request, Action<SnapshotResponse>[] methods, CancellationToken cancellationToken,
        Metadata headers = null, DateTime? deadline = null);

    /// <inheritdoc cref="EtcdClient.Status(StatusRequest, Metadata, DateTime?, CancellationToken)" />
    StatusResponse Status(StatusRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.StatusAsync(StatusRequest, Metadata, DateTime?, CancellationToken)" />
    Task<StatusResponse> StatusAsync(StatusRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Transaction(TxnRequest, Metadata, DateTime?, CancellationToken)" />
    TxnResponse Transaction(TxnRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.TransactionAsync(TxnRequest, Metadata, DateTime?, CancellationToken)" />
    Task<TxnResponse> TransactionAsync(TxnRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Unlock(string, Metadata, DateTime?, CancellationToken)" />
    UnlockResponse Unlock(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Unlock(UnlockRequest, Metadata, DateTime?, CancellationToken)" />
    UnlockResponse Unlock(UnlockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UnlockAsync(string, Metadata, DateTime?, CancellationToken)" />
    Task<UnlockResponse> UnlockAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UnlockAsync(UnlockRequest, Metadata, DateTime?, CancellationToken)" />
    Task<UnlockResponse> UnlockAsync(UnlockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserAdd(AuthUserAddRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserAddResponse UserAdd(AuthUserAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserAddAsync(AuthUserAddRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserChangePassword(AuthUserChangePasswordRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserChangePasswordAsync(AuthUserChangePasswordRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserDelete(AuthUserDeleteRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserDeleteAsync(AuthUserDeleteRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserGet(AuthUserGetRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserGetResponse UserGet(AuthUserGetRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserGetAsync(AuthUserGetRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserGrantRole(AuthUserGrantRoleRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserGrantRoleAsync(AuthUserGrantRoleRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserList(AuthUserListRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserListResponse UserList(AuthUserListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserListAsync(AuthUserListRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserRevokeRole(AuthUserRevokeRoleRequest, Metadata, DateTime?, CancellationToken)" />
    AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.UserRevokeRoleAsync(AuthUserRevokeRoleRequest, Metadata, DateTime?, CancellationToken)" />
    Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string, Action{WatchEvent[]}, Metadata, DateTime?, CancellationToken)" />
    void Watch(string key, Action<WatchEvent[]> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string, Action{WatchEvent[]}[], Metadata, DateTime?, CancellationToken)" />
    void Watch(string key, Action<WatchEvent[]>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string, Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    void Watch(string key, Action<WatchResponse> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string, Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    void Watch(string key, Action<WatchResponse>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string[], Action{WatchEvent[]}, Metadata, DateTime?, CancellationToken)" />
    void Watch(string[] keys, Action<WatchEvent[]> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string[], Action{WatchEvent[]}[], Metadata, DateTime?, CancellationToken)" />
    void Watch(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string[], Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    void Watch(string[] keys, Action<WatchResponse> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.Watch(string[], Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    void Watch(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest, Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    Task<long> WatchAsync(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest, Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    Task<long> WatchAsync(WatchRequest request, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest[], Action{WatchEvent[]}, Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest[], Action{WatchEvent[]}[], Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest[], Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchAsync(WatchRequest[], Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRange(string, Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    long WatchRange(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRange(string, Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    long WatchRange(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRange(string[], Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    long[] WatchRange(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRange(string[], Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    long[] WatchRange(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRangeAsync(string, Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    Task<long> WatchRangeAsync(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRangeAsync(string, Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    Task<long> WatchRangeAsync(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRangeAsync(string[], Action{WatchResponse}, Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.WatchRangeAsync(string[], Action{WatchResponse}[], Metadata, DateTime?, CancellationToken)" />
    Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="EtcdClient.CancelWatch(long)" />
    void CancelWatch(long watchId);

    /// <inheritdoc cref="EtcdClient.CancelWatch(long[])" />
    void CancelWatch(long[] watchIds);
}
