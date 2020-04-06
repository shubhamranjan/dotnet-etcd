using System;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// Authenticate processes an authenticate request.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthenticateResponse Authenticate(AuthenticateRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthenticateResponse response = new AuthenticateResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .Authenticate(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }
        
        /// <summary>
        /// Authenticate processes an authenticate request.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthenticateResponse response = new AuthenticateResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .AuthenticateAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// AuthEnable enables authentication
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthEnableResponse AuthEnable(AuthEnableRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthEnableResponse response = new AuthEnableResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .AuthEnable(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// AuthEnableAsync enables authentication in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthEnableResponse response = new AuthEnableResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .AuthEnableAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// AuthDisable disables authentication
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthDisableResponse AuthDisable(AuthDisableRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthDisableResponse response = new AuthDisableResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .AuthDisable(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// AuthDisableAsync disables authentication in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthDisableResponse response = new AuthDisableResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .AuthDisableAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserAdd adds a new user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserAddResponse UserAdd(AuthUserAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {

            AuthUserAddResponse response = new AuthUserAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserAdd(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserAddAsync adds a new user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {

            AuthUserAddResponse response = new AuthUserAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserAddAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserGet gets detailed user information
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserGetResponse UserGet(AuthUserGetRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserGetResponse response = new AuthUserGetResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserGet(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserGetAsync gets detailed user information in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserGetResponse response = new AuthUserGetResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserGetAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserList gets a list of all users
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserListResponse UserList(AuthUserListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserListResponse response = new AuthUserListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserList(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserListAsync gets a list of all users in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserListResponse response = new AuthUserListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserListAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserDelete deletes a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserDeleteResponse response = new AuthUserDeleteResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserDelete(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserDeleteAsync deletes a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserDeleteResponse response = new AuthUserDeleteResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserDeleteAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserChangePassword changes the password of a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserChangePasswordResponse response = new AuthUserChangePasswordResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserChangePassword(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserChangePasswordAsync changes the password of a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserChangePasswordResponse response = new AuthUserChangePasswordResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserChangePasswordAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserGrant grants a role to a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserGrantRoleResponse response = new AuthUserGrantRoleResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserGrantRole(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserGrantRoleAsync grants a role to a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserGrantRoleResponse response = new AuthUserGrantRoleResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserGrantRoleAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserRevokeRole revokes a role of specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserRevokeRoleResponse response = new AuthUserRevokeRoleResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .UserRevokeRole(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// UserRevokeRoleAsync revokes a role of specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthUserRevokeRoleResponse response = new AuthUserRevokeRoleResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .UserRevokeRoleAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleAdd adds a new role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleAddResponse response = new AuthRoleAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleAdd(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleAddAsync adds a new role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleAddResponse response = new AuthRoleAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleAddAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleGet gets detailed role information
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleGetResponse RoleGet(AuthRoleGetRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleGetResponse response = new AuthRoleGetResponse();

            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleGet(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleGetASync gets detailed role information in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleGetResponse> RoleGetASync(AuthRoleGetRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleGetResponse response = new AuthRoleGetResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleGetAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleList gets lists of all roles
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleListResponse RoleList(AuthRoleListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleListResponse response = new AuthRoleListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleList(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleListAsync gets lists of all roles async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleListResponse response = new AuthRoleListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleListAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleDelete deletes a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleDeleteResponse response = new AuthRoleDeleteResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleDelete(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleDeleteAsync deletes a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleDeleteResponse response = new AuthRoleDeleteResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleDeleteAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleGrantPermission grants a permission of a specified key or range to a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleGrantPermissionResponse response = new AuthRoleGrantPermissionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleGrantPermission(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleGrantPermissionAsync grants a permission of a specified key or range to a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(
            AuthRoleGrantPermissionRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleGrantPermissionResponse response = new AuthRoleGrantPermissionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleGrantPermissionAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleRevokePermission revokes a key or range permission of a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleRevokePermissionResponse response = new AuthRoleRevokePermissionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().authClient
                        .RoleRevokePermission(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// RoleRevokePermissionAsync revokes a key or range permission of a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(
            AuthRoleRevokePermissionRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            AuthRoleRevokePermissionResponse response = new AuthRoleRevokePermissionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().authClient
                        .RoleRevokePermissionAsync(request, headers, deadline, cancellationToken);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }
    }
}