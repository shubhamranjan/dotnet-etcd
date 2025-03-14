# Authentication

This page documents how to authenticate with etcd using the `dotnet-etcd` client.

## Overview

etcd supports authentication through username and password credentials. When authentication is enabled, a token is generated upon successful authentication, which is then used for subsequent requests.

## Enabling Authentication

Before using authentication, you need to enable it on the etcd server. This is typically done using the etcdctl command-line tool:

```bash
# Create root user
etcdctl user add root

# Enable authentication
etcdctl auth enable
```

## Authenticating with dotnet-etcd

### Basic Authentication

To authenticate with etcd, provide the username and password when initializing the client:

```csharp
// Initialize client with authentication
var client = new EtcdClient(
    "https://localhost:2379",
    username: "root",
    password: "rootpwd"
);

// Now all operations will be authenticated
var response = client.Get("my-key");
```

### Authentication with Connection String

You can also include authentication credentials in the connection string:

```csharp
// Using authentication in connection string
var client = new EtcdClient("https://root:rootpwd@localhost:2379");
```

### Changing Authentication Credentials

You can change the authentication credentials after client initialization:

```csharp
// Initialize client without authentication
var client = new EtcdClient("https://localhost:2379");

// Later, set authentication credentials
client.SetCredentials("root", "rootpwd");

// Now all subsequent operations will be authenticated
var response = client.Get("my-key");
```

## User Management

The `dotnet-etcd` client provides methods for managing users and roles.

### Adding a User

```csharp
// Add a new user
client.UserAdd("username", "password");
```

### Deleting a User

```csharp
// Delete a user
client.UserDelete("username");
```

### Changing User Password

```csharp
// Change user password
client.UserChangePassword("username", "newpassword");
```

### Listing Users

```csharp
// List all users
var users = client.UserList();
foreach (var user in users.Users)
{
    Console.WriteLine(user);
}
```

## Role Management

etcd uses role-based access control (RBAC) to manage permissions.

### Adding a Role

```csharp
// Add a new role
client.RoleAdd("admin");
```

### Deleting a Role

```csharp
// Delete a role
client.RoleDelete("admin");
```

### Granting Role to User

```csharp
// Grant role to user
client.UserGrantRole("username", "admin");
```

### Revoking Role from User

```csharp
// Revoke role from user
client.UserRevokeRole("username", "admin");
```

### Listing Roles

```csharp
// List all roles
var roles = client.RoleList();
foreach (var role in roles.Roles)
{
    Console.WriteLine(role);
}
```

## Permission Management

You can grant and revoke permissions for roles on specific keys or key ranges.

### Granting Permissions

```csharp
// Grant read permission on a key
client.RoleGrantPermission(
    "admin",
    "my-key",
    RoleGrantPermissionRequest.Types.Permission.Read
);

// Grant read/write permission on a key range
client.RoleGrantPermission(
    "admin",
    "my-prefix/",
    "my-prefix0",  // Range end (exclusive)
    RoleGrantPermissionRequest.Types.Permission.Readwrite
);
```

### Revoking Permissions

```csharp
// Revoke permission on a key
client.RoleRevokePermission("admin", "my-key");

// Revoke permission on a key range
client.RoleRevokePermission("admin", "my-prefix/", "my-prefix0");
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for authentication operations
- [Client Initialization](../client-initialization/index.md) - How to initialize and configure the etcd client
