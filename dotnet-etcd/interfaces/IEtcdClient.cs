// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;
using V3Electionpb;
using V3Lockpb;

namespace dotnet_etcd.interfaces;

public interface IEtcdClient
{
    AlarmResponse Alarm(AlarmRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AlarmResponse> AlarmAsync(AlarmRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    AuthDisableResponse AuthDisable(AuthDisableRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthEnableResponse AuthEnable(AuthEnableRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthenticateResponse Authenticate(AuthenticateRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    CompactionResponse Compact(CompactionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<CompactionResponse> CompactAsync(CompactionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    CampaignResponse Campaign(CampaignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    CampaignResponse Campaign(string name, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<CampaignResponse> CampaignAsync(CampaignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<CampaignResponse> CampaignAsync(string name, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    DefragmentResponse Defragment(DefragmentRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<DefragmentResponse> DefragmentAsync(DefragmentRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    DeleteRangeResponse Delete(DeleteRangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    DeleteRangeResponse Delete(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<DeleteRangeResponse> DeleteAsync(DeleteRangeRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<DeleteRangeResponse> DeleteAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    DeleteRangeResponse DeleteRange(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<DeleteRangeResponse> DeleteRangeAsync(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Dispose();

    RangeResponse Get(RangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    RangeResponse Get(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<RangeResponse> GetAsync(RangeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<RangeResponse> GetAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    RangeResponse GetRange(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<RangeResponse> GetRangeAsync(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    IDictionary<string, string> GetRangeVal(string prefixKey, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<IDictionary<string, string>> GetRangeValAsync(string prefixKey, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    string GetVal(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<string> GetValAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    HashResponse Hash(HashRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<HashResponse> HashAsync(HashRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    HashKVResponse HashKV(HashKVRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<HashKVResponse> HashKVAsync(HashKVRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method,
        CancellationToken cancellationToken, Metadata headers = null);

    Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods,
        CancellationToken cancellationToken, Metadata headers = null);

    Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method,
        CancellationToken cancellationToken, Metadata headers = null);

    Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods,
        CancellationToken cancellationToken, Metadata headers = null, DateTime? deadline = null);

    Task LeaseKeepAlive(CancellationTokenSource cancellationTokenSource, long leaseId, int keepAliveTimeout = 1000,
        int? communicationTimeout = null, Metadata headers = null, DateTime? deadline = null);

    Task LeaseKeepAlive(long leaseId, CancellationToken cancellationToken);

    LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    LockResponse Lock(LockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    LockResponse Lock(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LockResponse> LockAsync(LockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LockResponse> LockAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    LeaderResponse Leader(LeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    LeaderResponse Leader(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LeaderResponse> LeaderAsync(LeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<LeaderResponse> LeaderAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    MemberAddResponse MemberAdd(MemberAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    MemberListResponse MemberList(MemberListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<MemberListResponse> MemberListAsync(MemberListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    MoveLeaderResponse MoveLeader(MoveLeaderRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<MoveLeaderResponse> MoveLeaderAsync(MoveLeaderRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<LeaderResponse> ObserveAsync(LeaderRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<LeaderResponse> ObserveAsync(string name, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    ProclaimResponse Proclaim(ProclaimRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    ProclaimResponse Proclaim(LeaderKey leader, string value, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<ProclaimResponse> ProclaimAsync(ProclaimRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<ProclaimResponse> ProclaimAsync(LeaderKey leader, string value, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    ResignResponse Resign(ResignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    ResignResponse Resign(LeaderKey leader, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<ResignResponse> ResignAsync(ResignRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<ResignResponse> ResignAsync(LeaderKey leader, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    PutResponse Put(PutRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    PutResponse Put(string key, string val, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<PutResponse> PutAsync(PutRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<PutResponse> PutAsync(string key, string val, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthRoleGetResponse RoleGet(AuthRoleGetRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthRoleGetResponse> RoleGetAsync(AuthRoleGetRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(AuthRoleGrantPermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthRoleListResponse RoleList(AuthRoleListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(AuthRoleRevokePermissionRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task Snapshot(SnapshotRequest request, Action<SnapshotResponse> method, CancellationToken cancellationToken,
        Metadata headers = null, DateTime? deadline = null);

    Task Snapshot(SnapshotRequest request, Action<SnapshotResponse>[] methods, CancellationToken cancellationToken,
        Metadata headers = null, DateTime? deadline = null);

    StatusResponse Status(StatusRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<StatusResponse> StatusAsync(StatusRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    TxnResponse Transaction(TxnRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<TxnResponse> TransactionAsync(TxnRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    UnlockResponse Unlock(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    UnlockResponse Unlock(UnlockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<UnlockResponse> UnlockAsync(string key, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<UnlockResponse> UnlockAsync(UnlockRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    AuthUserAddResponse UserAdd(AuthUserAddRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request,
        Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserGetResponse UserGet(AuthUserGetRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserListResponse UserList(AuthUserListRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    void Watch(string key, Action<WatchEvent[]> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string key, Action<WatchEvent[]>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string key, Action<WatchResponse> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string key, Action<WatchResponse>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string[] keys, Action<WatchEvent[]> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string[] keys, Action<WatchResponse> method, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    void Watch(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    Task<long> WatchAsync(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long> WatchAsync(WatchRequest request, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    long WatchRange(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    long WatchRange(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    long[] WatchRange(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    long[] WatchRange(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long> WatchRangeAsync(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long> WatchRangeAsync(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);
}
