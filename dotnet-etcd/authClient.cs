using Etcdserverpb;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// AuthEnable enables authentication
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthEnableResponse AuthEnable(AuthEnableRequest request)
        {
            AuthEnableResponse response = new AuthEnableResponse();
            try
            {
                response = _authClient.AuthEnable(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// AuthEnableAsync enables authentication in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request)
        {
            AuthEnableResponse response = new AuthEnableResponse();
            try
            {
                response = await _authClient.AuthEnableAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// AuthDisable disables authentication
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthDisableResponse AuthDisable(AuthDisableRequest request)
        {
            AuthDisableResponse response = new AuthDisableResponse();
            try
            {
                response = _authClient.AuthDisable(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// AuthDisableAsync disables authentication in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request)
        {
            AuthDisableResponse response = new AuthDisableResponse();
            try
            {
                response = await _authClient.AuthDisableAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserAdd adds a new user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserAddResponse UserAdd(AuthUserAddRequest request)
        {

            AuthUserAddResponse response = new AuthUserAddResponse();
            try
            {
                response = _authClient.UserAdd(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserAddAsync adds a new user in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request)
        {

            AuthUserAddResponse response = new AuthUserAddResponse();
            try
            {
                response = await _authClient.UserAddAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserGet gets detailed user information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserGetResponse UserGet(AuthUserGetRequest request)
        {
            AuthUserGetResponse response = new AuthUserGetResponse();
            try
            {
                response = _authClient.UserGet(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserGetAsync gets detailed user information in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request)
        {
            AuthUserGetResponse response = new AuthUserGetResponse();
            try
            {
                response = await _authClient.UserGetAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserList gets a list of all users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserListResponse UserList(AuthUserListRequest request)
        {
            AuthUserListResponse response = new AuthUserListResponse();
            try
            {
                response = _authClient.UserList(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserListAsync gets a list of all users in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request)
        {
            AuthUserListResponse response = new AuthUserListResponse();
            try
            {
                response = await _authClient.UserListAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserDelete deletes a specified user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request)
        {
            AuthUserDeleteResponse response = new AuthUserDeleteResponse();
            try
            {
                response = _authClient.UserDelete(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserDeleteAsync deletes a specified user in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request)
        {
            AuthUserDeleteResponse response = new AuthUserDeleteResponse();
            try
            {
                response = await _authClient.UserDeleteAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserChangePassword changes the password of a specified user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request)
        {
            AuthUserChangePasswordResponse response = new AuthUserChangePasswordResponse();
            try
            {
                response = _authClient.UserChangePassword(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserChangePasswordAsync changes the password of a specified user in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request)
        {
            AuthUserChangePasswordResponse response = new AuthUserChangePasswordResponse();
            try
            {
                response = await _authClient.UserChangePasswordAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserGrant grants a role to a specified user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request)
        {
            AuthUserGrantRoleResponse response = new AuthUserGrantRoleResponse();
            try
            {
                response = _authClient.UserGrantRole(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserGrantRoleAsync grants a role to a specified user in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request)
        {
            AuthUserGrantRoleResponse response = new AuthUserGrantRoleResponse();
            try
            {
                response = await _authClient.UserGrantRoleAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserRevokeRole revokes a role of specified user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request)
        {
            AuthUserRevokeRoleResponse response = new AuthUserRevokeRoleResponse();
            try
            {
                response = _authClient.UserRevokeRole(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// UserRevokeRoleAsync revokes a role of specified user in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request)
        {
            AuthUserRevokeRoleResponse response = new AuthUserRevokeRoleResponse();
            try
            {
                response = await _authClient.UserRevokeRoleAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleAdd adds a new role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request)
        {
            AuthRoleAddResponse response = new AuthRoleAddResponse();
            try
            {
                response = _authClient.RoleAdd(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleAddAsync adds a new role in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request)
        {
            AuthRoleAddResponse response = new AuthRoleAddResponse();
            try
            {
                response = await _authClient.RoleAddAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleGet gets detailed role information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleGetResponse RoleGet(AuthRoleGetRequest request)
        {
            AuthRoleGetResponse response = new AuthRoleGetResponse();
            try
            {
                response = _authClient.RoleGet(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleGetASync gets detailed role information in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleGetResponse> RoleGetASync(AuthRoleGetRequest request)
        {
            AuthRoleGetResponse response = new AuthRoleGetResponse();
            try
            {
                response = await _authClient.RoleGetAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleList gets lists of all roles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleListResponse RoleList(AuthRoleListRequest request)
        {
            AuthRoleListResponse response = new AuthRoleListResponse();
            try
            {
                response = _authClient.RoleList(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleListAsync gets lists of all roles async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request)
        {
            AuthRoleListResponse response = new AuthRoleListResponse();
            try
            {
                response = await _authClient.RoleListAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleDelete deletes a specified role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request)
        {
            AuthRoleDeleteResponse response = new AuthRoleDeleteResponse();
            try
            {
                response = _authClient.RoleDelete(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleDeleteAsync deletes a specified role in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request)
        {
            AuthRoleDeleteResponse response = new AuthRoleDeleteResponse();
            try
            {
                response = await _authClient.RoleDeleteAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleGrantPermission grants a permission of a specified key or range to a specified role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request)
        {
            AuthRoleGrantPermissionResponse response = new AuthRoleGrantPermissionResponse();
            try
            {
                response = _authClient.RoleGrantPermission(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleGrantPermissionAsync grants a permission of a specified key or range to a specified role in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(AuthRoleGrantPermissionRequest request)
        {
            AuthRoleGrantPermissionResponse response = new AuthRoleGrantPermissionResponse();
            try
            {
                response = await _authClient.RoleGrantPermissionAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleRevokePermission revokes a key or range permission of a specified role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request)
        {
            AuthRoleRevokePermissionResponse response = new AuthRoleRevokePermissionResponse();
            try
            {
                response = _authClient.RoleRevokePermission(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// RoleRevokePermissionAsync revokes a key or range permission of a specified role in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(AuthRoleRevokePermissionRequest request)
        {
            AuthRoleRevokePermissionResponse response = new AuthRoleRevokePermissionResponse();
            try
            {
                response = await _authClient.RoleRevokePermissionAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }
    }
}
