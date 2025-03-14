// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd;

public partial class EtcdClient
{
    /// <summary>
    ///     Authenticate processes an authenticate request.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthenticateResponse Authenticate(AuthenticateRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .Authenticate(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     Authenticate processes an authenticate request.
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .AuthenticateAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     AuthEnable enables authentication
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthEnableResponse AuthEnable(AuthEnableRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .AuthEnable(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     AuthEnableAsync enables authentication in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .AuthEnableAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     AuthDisable disables authentication
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthDisableResponse AuthDisable(AuthDisableRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .AuthDisable(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     AuthDisableAsync disables authentication in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .AuthDisableAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserAdd adds a new user
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserAddResponse UserAdd(AuthUserAddRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserAdd(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserAddAsync adds a new user in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserGet gets detailed user information
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserGetResponse UserGet(AuthUserGetRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserGet(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserGetAsync gets detailed user information in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserGetAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserList gets a list of all users
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserListResponse UserList(AuthUserListRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserList(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserListAsync gets a list of all users in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserDelete deletes a specified user
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserDelete(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserDeleteAsync deletes a specified user in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserDeleteAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserChangePassword changes the password of a specified user
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserChangePassword(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserChangePasswordAsync changes the password of a specified user in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserChangePasswordAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserGrant grants a role to a specified user
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserGrantRole(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserGrantRoleAsync grants a role to a specified user in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserGrantRoleAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     UserRevokeRole revokes a role of specified user
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .UserRevokeRole(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     UserRevokeRoleAsync revokes a role of specified user in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .UserRevokeRoleAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleAdd adds a new role
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleAdd(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleAddAsync adds a new role in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleGet gets detailed role information
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleGetResponse RoleGet(AuthRoleGetRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleGet(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleGetAsync gets detailed role information in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleGetResponse> RoleGetAsync(AuthRoleGetRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleGetAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleList gets lists of all roles
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleListResponse RoleList(AuthRoleListRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleList(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleListAsync gets lists of all roles async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleDelete deletes a specified role
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleDelete(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleDeleteAsync deletes a specified role in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleDeleteAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleGrantPermission grants a permission of a specified key or range to a specified role
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleGrantPermission(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleGrantPermissionAsync grants a permission of a specified key or range to a specified role in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(
        AuthRoleGrantPermissionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleGrantPermissionAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    ///     RoleRevokePermission revokes a key or range permission of a specified role
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => CallEtcd(connection => connection.AuthClient
        .RoleRevokePermission(request, headers, deadline, cancellationToken));

    /// <summary>
    ///     RoleRevokePermissionAsync revokes a key or range permission of a specified role in async
    /// </summary>
    /// <param name="request">The request to send to the server.</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>The response received from the server.</returns>
    public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(
        AuthRoleRevokePermissionRequest request, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => await CallEtcdAsync(async connection => await connection
        .AuthClient
        .RoleRevokePermissionAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
}
