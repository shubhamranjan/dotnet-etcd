# Authentication

This page documents how to authenticate with etcd using the `dotnet-etcd` client.

## Overview

etcd supports authentication through username and password credentials. The `dotnet-etcd` client provides **automatic authentication** - simply provide your credentials and the client handles token management for you.

## Enabling Authentication on etcd Server

Before using authentication, you need to enable it on the etcd server:

```bash
# Create root user
etcdctl user add root

# Enable authentication
etcdctl auth enable
```

## Automatic Authentication

### Constructor with Credentials (Recommended)

The simplest way to use authentication is to provide credentials when creating the client:

```csharp
// Client automatically authenticates and manages tokens
var client = new EtcdClient("localhost:2379", "root", "rootpwd");

// All operations are automatically authenticated
client.Put("my-key", "my-value");
var response = client.Get("my-key");
```

### SetCredentials Method

You can also set credentials after client creation:

```csharp
// Create client without authentication
var client = new EtcdClient("localhost:2379");

// Set credentials when needed
client.SetCredentials("root", "rootpwd");

// All subsequent operations are automatically authenticated
client.Put("my-key", "my-value");
```

### How Automatic Authentication Works

When you provide credentials:
1. The client authenticates with etcd on the first request
2. The authentication token is cached and reused
3. All requests automatically include the token in the `authorization` header
4. You never need to manually manage tokens

### Changing Credentials

You can change credentials at runtime:

```csharp
var client = new EtcdClient("localhost:2379", "user1", "pass1");
client.Put("key1", "value1");  // Authenticated as user1

// Switch to different credentials
client.SetCredentials("user2", "pass2");
client.Put("key2", "value2");  // Authenticated as user2
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
