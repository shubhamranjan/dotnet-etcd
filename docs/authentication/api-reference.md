# Authentication API Reference

This page provides a complete reference of all authentication-related methods available in the `dotnet-etcd` client.

## Authentication Methods

### SetCredentials

Sets the authentication credentials for the client.

#### SetCredentials Parameters

- `username`: The username to authenticate with.
- `password`: The password to authenticate with.

```csharp
public void SetCredentials(string username, string password)
```

## User Management Methods

### UserAdd

Adds a new user.

#### UserAdd Overloads

```csharp
// Add a user with username and password
public AuthUserAddResponse UserAdd(
    string name,
    string password,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserAdd Parameters

- `name`: The name of the user to add.
- `password`: The password for the user.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserAdd Returns

- `AuthUserAddResponse`: The etcd response for the user add operation.

### UserAddAsync

Adds a new user asynchronously.

#### UserAddAsync Overloads

```csharp
// Add a user with username and password asynchronously
public async Task<AuthUserAddResponse> UserAddAsync(
    string name,
    string password,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserAddAsync Parameters

- `name`: The name of the user to add.
- `password`: The password for the user.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserAddAsync Returns

- `Task<AuthUserAddResponse>`: The etcd response for the user add operation.

### UserGet

Gets detailed information about a user.

#### UserGet Overloads

```csharp
// Get user information
public AuthUserGetResponse UserGet(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserGet Parameters

- `name`: The name of the user to get information about.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserGet Returns

- `AuthUserGetResponse`: The etcd response containing user information.

### UserGetAsync

Gets detailed information about a user asynchronously.

#### UserGetAsync Overloads

```csharp
// Get user information asynchronously
public async Task<AuthUserGetResponse> UserGetAsync(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserGetAsync Parameters

- `name`: The name of the user to get information about.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserGetAsync Returns

- `Task<AuthUserGetResponse>`: The etcd response containing user information.

### UserList

Lists all users.

#### UserList Overloads

```csharp
// List all users
public AuthUserListResponse UserList(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserList Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserList Returns

- `AuthUserListResponse`: The etcd response containing the list of users.

### UserListAsync

Lists all users asynchronously.

#### UserListAsync Overloads

```csharp
// List all users asynchronously
public async Task<AuthUserListResponse> UserListAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserListAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserListAsync Returns

- `Task<AuthUserListResponse>`: The etcd response containing the list of users.

### UserDelete

Deletes a user.

#### UserDelete Overloads

```csharp
// Delete a user
public AuthUserDeleteResponse UserDelete(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserDelete Parameters

- `name`: The name of the user to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserDelete Returns

- `AuthUserDeleteResponse`: The etcd response for the user delete operation.

### UserDeleteAsync

Deletes a user asynchronously.

#### UserDeleteAsync Overloads

```csharp
// Delete a user asynchronously
public async Task<AuthUserDeleteResponse> UserDeleteAsync(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserDeleteAsync Parameters

- `name`: The name of the user to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserDeleteAsync Returns

- `Task<AuthUserDeleteResponse>`: The etcd response for the user delete operation.

### UserChangePassword

Changes a user's password.

#### UserChangePassword Overloads

```csharp
// Change user password
public AuthUserChangePasswordResponse UserChangePassword(
    string name,
    string password,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserChangePassword Parameters

- `name`: The name of the user whose password to change.
- `password`: The new password.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserChangePassword Returns

- `AuthUserChangePasswordResponse`: The etcd response for the password change operation.

### UserChangePasswordAsync

Changes a user's password asynchronously.

#### UserChangePasswordAsync Overloads

```csharp
// Change user password asynchronously
public async Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(
    string name,
    string password,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserChangePasswordAsync Parameters

- `name`: The name of the user whose password to change.
- `password`: The new password.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserChangePasswordAsync Returns

- `Task<AuthUserChangePasswordResponse>`: The etcd response for the password change operation.

## Role Management Methods

### RoleAdd

Adds a new role.

#### RoleAdd Overloads

```csharp
// Add a role
public AuthRoleAddResponse RoleAdd(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleAdd Parameters

- `name`: The name of the role to add.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleAdd Returns

- `AuthRoleAddResponse`: The etcd response for the role add operation.

### RoleAddAsync

Adds a new role asynchronously.

#### RoleAddAsync Overloads

```csharp
// Add a role asynchronously
public async Task<AuthRoleAddResponse> RoleAddAsync(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleAddAsync Parameters

- `name`: The name of the role to add.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleAddAsync Returns

- `Task<AuthRoleAddResponse>`: The etcd response for the role add operation.

### RoleGet

Gets detailed information about a role.

#### RoleGet Overloads

```csharp
// Get role information
public AuthRoleGetResponse RoleGet(
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleGet Parameters

- `role`: The name of the role to get information about.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleGet Returns

- `AuthRoleGetResponse`: The etcd response containing role information.

### RoleGetAsync

Gets detailed information about a role asynchronously.

#### RoleGetAsync Overloads

```csharp
// Get role information asynchronously
public async Task<AuthRoleGetResponse> RoleGetAsync(
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleGetAsync Parameters

- `role`: The name of the role to get information about.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleGetAsync Returns

- `Task<AuthRoleGetResponse>`: The etcd response containing role information.

### RoleList

Lists all roles.

#### RoleList Overloads

```csharp
// List all roles
public AuthRoleListResponse RoleList(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleList Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleList Returns

- `AuthRoleListResponse`: The etcd response containing the list of roles.

### RoleListAsync

Lists all roles asynchronously.

#### RoleListAsync Overloads

```csharp
// List all roles asynchronously
public async Task<AuthRoleListResponse> RoleListAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleListAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleListAsync Returns

- `Task<AuthRoleListResponse>`: The etcd response containing the list of roles.

### RoleDelete

Deletes a role.

#### RoleDelete Overloads

```csharp
// Delete a role
public AuthRoleDeleteResponse RoleDelete(
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleDelete Parameters

- `role`: The name of the role to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleDelete Returns

- `AuthRoleDeleteResponse`: The etcd response for the role delete operation.

### RoleDeleteAsync

Deletes a role asynchronously.

#### RoleDeleteAsync Overloads

```csharp
// Delete a role asynchronously
public async Task<AuthRoleDeleteResponse> RoleDeleteAsync(
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleDeleteAsync Parameters

- `role`: The name of the role to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleDeleteAsync Returns

- `Task<AuthRoleDeleteResponse>`: The etcd response for the role delete operation.

## Permission Management Methods

### RoleGrantPermission

Grants a permission to a role.

#### RoleGrantPermission Overloads

```csharp
// Grant permission on a key
public AuthRoleGrantPermissionResponse RoleGrantPermission(
    string role,
    string key,
    Etcdserverpb.RoleGrantPermissionRequest.Types.Permission permType,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Grant permission on a key range
public AuthRoleGrantPermissionResponse RoleGrantPermission(
    string role,
    string key,
    string rangeEnd,
    Etcdserverpb.RoleGrantPermissionRequest.Types.Permission permType,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleGrantPermission Parameters

- `role`: The name of the role to grant permission to.
- `key`: The key to grant permission on.
- `rangeEnd`: The end of the key range (exclusive) to grant permission on.
- `permType`: The type of permission to grant (Read, Write, or Readwrite).
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleGrantPermission Returns

- `AuthRoleGrantPermissionResponse`: The etcd response for the permission grant operation.

### RoleGrantPermissionAsync

Grants a permission to a role asynchronously.

#### RoleGrantPermissionAsync Overloads

```csharp
// Grant permission on a key asynchronously
public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(
    string role,
    string key,
    Etcdserverpb.RoleGrantPermissionRequest.Types.Permission permType,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Grant permission on a key range asynchronously
public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(
    string role,
    string key,
    string rangeEnd,
    Etcdserverpb.RoleGrantPermissionRequest.Types.Permission permType,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleGrantPermissionAsync Parameters

- `role`: The name of the role to grant permission to.
- `key`: The key to grant permission on.
- `rangeEnd`: The end of the key range (exclusive) to grant permission on.
- `permType`: The type of permission to grant (Read, Write, or Readwrite).
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleGrantPermissionAsync Returns

- `Task<AuthRoleGrantPermissionResponse>`: The etcd response for the permission grant operation.

### RoleRevokePermission

Revokes a permission from a role.

#### RoleRevokePermission Overloads

```csharp
// Revoke permission on a key
public AuthRoleRevokePermissionResponse RoleRevokePermission(
    string role,
    string key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Revoke permission on a key range
public AuthRoleRevokePermissionResponse RoleRevokePermission(
    string role,
    string key,
    string rangeEnd,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleRevokePermission Parameters

- `role`: The name of the role to revoke permission from.
- `key`: The key to revoke permission on.
- `rangeEnd`: The end of the key range (exclusive) to revoke permission on.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleRevokePermission Returns

- `AuthRoleRevokePermissionResponse`: The etcd response for the permission revoke operation.

### RoleRevokePermissionAsync

Revokes a permission from a role asynchronously.

#### RoleRevokePermissionAsync Overloads

```csharp
// Revoke permission on a key asynchronously
public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(
    string role,
    string key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Revoke permission on a key range asynchronously
public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(
    string role,
    string key,
    string rangeEnd,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### RoleRevokePermissionAsync Parameters

- `role`: The name of the role to revoke permission from.
- `key`: The key to revoke permission on.
- `rangeEnd`: The end of the key range (exclusive) to revoke permission on.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### RoleRevokePermissionAsync Returns

- `Task<AuthRoleRevokePermissionResponse>`: The etcd response for the permission revoke operation.

## User-Role Management Methods

### UserGrantRole

Grants a role to a user.

#### UserGrantRole Overloads

```csharp
// Grant role to user
public AuthUserGrantRoleResponse UserGrantRole(
    string user,
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserGrantRole Parameters

- `user`: The name of the user to grant the role to.
- `role`: The name of the role to grant.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserGrantRole Returns

- `AuthUserGrantRoleResponse`: The etcd response for the role grant operation.

### UserGrantRoleAsync

Grants a role to a user asynchronously.

#### UserGrantRoleAsync Overloads

```csharp
// Grant role to user asynchronously
public async Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(
    string user,
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserGrantRoleAsync Parameters

- `user`: The name of the user to grant the role to.
- `role`: The name of the role to grant.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserGrantRoleAsync Returns

- `Task<AuthUserGrantRoleResponse>`: The etcd response for the role grant operation.

### UserRevokeRole

Revokes a role from a user.

#### UserRevokeRole Overloads

```csharp
// Revoke role from user
public AuthUserRevokeRoleResponse UserRevokeRole(
    string name,
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserRevokeRole Parameters

- `name`: The name of the user to revoke the role from.
- `role`: The name of the role to revoke.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserRevokeRole Returns

- `AuthUserRevokeRoleResponse`: The etcd response for the role revoke operation.

### UserRevokeRoleAsync

Revokes a role from a user asynchronously.

#### UserRevokeRoleAsync Overloads

```csharp
// Revoke role from user asynchronously
public async Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(
    string name,
    string role,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UserRevokeRoleAsync Parameters

- `name`: The name of the user to revoke the role from.
- `role`: The name of the role to revoke.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UserRevokeRoleAsync Returns

- `Task<AuthUserRevokeRoleResponse>`: The etcd response for the role revoke operation.

## Authentication Response Types

### AuthUserAddResponse

The `AuthUserAddResponse` class represents the response from a user add operation.

#### AuthUserAddResponse Properties

- `Header`: The response header.

### AuthUserGetResponse

The `AuthUserGetResponse` class represents the response from a user get operation.

#### AuthUserGetResponse Properties

- `Header`: The response header.
- `Roles`: The list of roles assigned to the user.

### AuthUserListResponse

The `AuthUserListResponse` class represents the response from a user list operation.

#### AuthUserListResponse Properties

- `Header`: The response header.
- `Users`: The list of users.

### AuthUserDeleteResponse

The `AuthUserDeleteResponse` class represents the response from a user delete operation.

#### AuthUserDeleteResponse Properties

- `Header`: The response header.

### AuthUserChangePasswordResponse

The `AuthUserChangePasswordResponse` class represents the response from a user change password operation.

#### AuthUserChangePasswordResponse Properties

- `Header`: The response header.

### AuthUserGrantRoleResponse

The `AuthUserGrantRoleResponse` class represents the response from a user grant role operation.

#### AuthUserGrantRoleResponse Properties

- `Header`: The response header.

### AuthUserRevokeRoleResponse

The `AuthUserRevokeRoleResponse` class represents the response from a user revoke role operation.

#### AuthUserRevokeRoleResponse Properties

- `Header`: The response header.

### AuthRoleAddResponse

The `AuthRoleAddResponse` class represents the response from a role add operation.

#### AuthRoleAddResponse Properties

- `Header`: The response header.

### AuthRoleGetResponse

The `AuthRoleGetResponse` class represents the response from a role get operation.

#### AuthRoleGetResponse Properties

- `Header`: The response header.
- `Perm`: The list of permissions assigned to the role.

### AuthRoleListResponse

The `AuthRoleListResponse` class represents the response from a role list operation.

#### AuthRoleListResponse Properties

- `Header`: The response header.
- `Roles`: The list of roles.

### AuthRoleDeleteResponse

The `AuthRoleDeleteResponse` class represents the response from a role delete operation.

#### AuthRoleDeleteResponse Properties

- `Header`: The response header.

### AuthRoleGrantPermissionResponse

The `AuthRoleGrantPermissionResponse` class represents the response from a role grant permission operation.

#### AuthRoleGrantPermissionResponse Properties

- `Header`: The response header.

### AuthRoleRevokePermissionResponse

The `AuthRoleRevokePermissionResponse` class represents the response from a role revoke permission operation.

#### AuthRoleRevokePermissionResponse Properties

- `Header`: The response header.

### AuthEnableResponse

The `AuthEnableResponse` class represents the response from an auth enable operation.

#### AuthEnableResponse Properties

- `Header`: The response header.

### AuthDisableResponse

The `AuthDisableResponse` class represents the response from an auth disable operation.

#### AuthDisableResponse Properties

- `Header`: The response header.

### AuthStatusResponse

The `AuthStatusResponse` class represents the response from an auth status operation.

#### AuthStatusResponse Properties

- `Header`: The response header.
- `Enabled`: Whether authentication is enabled.

### AuthenticateResponse

The `AuthenticateResponse` class represents the response from an authenticate operation.

#### AuthenticateResponse Properties

- `Header`: The response header.
- `Token`: The authentication token.

## See Also

- [Authentication](index.md) - Overview and examples of authentication operations
- [Client Initialization](../client-initialization/index.md) - How to initialize and configure the etcd client
