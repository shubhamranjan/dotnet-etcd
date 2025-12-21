using System.Text;
using Authpb;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
/// Authentication tests that must run sequentially to avoid interfering with each other
/// All tests in this collection will run one at a time
/// </summary>
[Collection("AuthTests")]
[Trait("Category", "Integration")]
public class AuthClientIntegrationTests : IDisposable
{
    private const string TestUsername = "testuser";
    private const string TestPassword = "testpass123";
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123";
    
    // Track clients created during tests for cleanup
    private readonly List<EtcdClient> _clientsToDispose = new();

    public void Dispose()
    {
        // Ensure auth is disabled after each test to not affect other tests
        try
        {
            using var cleanupClient = new EtcdClient("127.0.0.1:2379");
            CleanupAuth(cleanupClient).Wait();
        }
        catch
        {
            // Ignore cleanup errors
        }

        // Dispose all clients
        foreach (var client in _clientsToDispose)
        {
            try
            {
                client.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        _clientsToDispose.Clear();
    }

    #region Basic Authentication Tests

    [Fact]
    public async Task EnableAndDisableAuth_ShouldSucceed()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            // Create root user before enabling auth (required by etcd)
            await CreateRootUser(client);

            // Act - Enable authentication
            var enableResponse = await client.AuthEnableAsync(new AuthEnableRequest());

            // Assert
            Assert.NotNull(enableResponse);

            // Verify auth is enabled by trying an operation without credentials (should fail)
            using var unauthClient = new EtcdClient("127.0.0.1:2379");
            await Assert.ThrowsAsync<RpcException>(async () =>
                await unauthClient.GetAsync("test"));

            // Disable auth (use root credentials since admin doesn't exist in this test)
            client.SetCredentials("root", "root");
            var disableResponse = await client.AuthDisableAsync(new AuthDisableRequest());
            Assert.NotNull(disableResponse);

            // Verify auth is disabled
            var rangeResponse = await unauthClient.GetAsync("test");
            Assert.NotNull(rangeResponse);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task CreateUserAndAuthenticate_ShouldReturnToken()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);

            // Act - Create a new user
            client.SetCredentials(AdminUsername, AdminPassword);
            var addUserResponse = await client.UserAddAsync(new AuthUserAddRequest
            {
                Name = TestUsername,
                Password = TestPassword,
                Options = new UserAddOptions { NoPassword = false }
            });

            Assert.NotNull(addUserResponse);

            // Authenticate as the new user
            var authResponse = await client.AuthenticateAsync(new AuthenticateRequest
            {
                Name = TestUsername,
                Password = TestPassword
            });

            // Assert
            Assert.NotNull(authResponse);
            Assert.NotEmpty(authResponse.Token);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    #endregion

    #region Interceptor Tests

    [Fact]
    public async Task AuthenticatedOperations_WithInterceptor_ShouldSucceed()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            // Act - Set credentials (interceptor will auto-add token)
            client.SetCredentials(TestUsername, TestPassword);

            // Perform various operations to verify interceptor adds token
            var putResponse = await client.PutAsync("auth-test-key", "auth-test-value");
            var rangeResponse = await client.GetAsync("auth-test-key");
            var deleteResponse = await client.DeleteAsync("auth-test-key");

            // Assert - All operations should succeed
            Assert.NotNull(putResponse);
            Assert.NotNull(rangeResponse);
            Assert.NotNull(deleteResponse);
            Assert.Single(rangeResponse.Kvs);
            Assert.Equal("auth-test-value", rangeResponse.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task MultipleOperations_ShouldReuseToken()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        var operationCount = 10;

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            client.SetCredentials(TestUsername, TestPassword);

            // Act - Perform multiple operations
            for (int i = 0; i < operationCount; i++)
            {
                var response = await client.PutAsync($"test-key-{i}", $"test-value-{i}");
                Assert.NotNull(response);
            }

            // Verify all keys were created
            var rangeResponse = await client.GetRangeAsync("test-key-");

            // Assert
            Assert.Equal(operationCount, rangeResponse.Kvs.Count);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    #endregion

    #region Credential Management Tests

    [Fact]
    public async Task SetCredentials_ShouldAutoAuthenticate()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            // Act - SetCredentials should trigger authentication
            client.SetCredentials(TestUsername, TestPassword);

            // Perform an operation that requires auth
            var response = await client.GetAsync("any-key");

            // Assert - Operation should succeed with auto-authentication
            Assert.NotNull(response);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task CredentialRotation_ShouldRefreshToken()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        const string newPassword = "newpass456";

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            // Set initial credentials
            client.SetCredentials(TestUsername, TestPassword);

            // Perform operation with first set of credentials
            var response1 = await client.PutAsync("rotation-test-1", "value-1");
            Assert.NotNull(response1);

            // Act - Change password as admin
            client.SetCredentials(AdminUsername, AdminPassword);
            await client.UserChangePasswordAsync(new AuthUserChangePasswordRequest
            {
                Name = TestUsername,
                Password = newPassword
            });

            // Set new credentials - should trigger new authentication
            client.SetCredentials(TestUsername, newPassword);

            // Perform operation with new credentials
            var response2 = await client.PutAsync("rotation-test-2", "value-2");

            // Assert
            Assert.NotNull(response2);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task InvalidCredentials_ShouldThrowException()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);

            // Act & Assert - Authenticate with wrong credentials
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.AuthenticateAsync(new AuthenticateRequest
                {
                    Name = "nonexistent",
                    Password = "wrongpassword"
                }));

            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task OperationWithoutAuth_WhenAuthEnabled_ShouldFail()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        var unauthClient = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(unauthClient);

        try
        {
            await SetupAuth(client);

            // Act & Assert - Try operation without credentials
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await unauthClient.PutAsync("test", "value"));

            // etcd may return either Unauthenticated or InvalidArgument when no credentials provided
            Assert.True(
                exception.StatusCode == StatusCode.Unauthenticated || exception.StatusCode == StatusCode.InvalidArgument,
                $"Expected Unauthenticated or InvalidArgument but got {exception.StatusCode}");
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
            unauthClient.Dispose();
        }
    }

    [Fact]
    public async Task WrongPassword_AfterPasswordChange_ShouldFail()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        const string newPassword = "newpass789";

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            // Authenticate with initial password
            client.SetCredentials(TestUsername, TestPassword);
            var response1 = await client.GetAsync("test");
            Assert.NotNull(response1);

            // Change password
            client.SetCredentials(AdminUsername, AdminPassword);
            await client.UserChangePasswordAsync(new AuthUserChangePasswordRequest
            {
                Name = TestUsername,
                Password = newPassword
            });

            // Act & Assert - Try to authenticate with old password
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.AuthenticateAsync(new AuthenticateRequest
                {
                    Name = TestUsername,
                    Password = TestPassword // Old password
                }));

            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    #endregion

    #region User & Role Management Tests

    [Fact]
    public async Task CreateUserWithRole_ShouldGrantPermissions()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        const string roleName = "readonlyrole";
        const string keyPrefix = "readonly/";

        try
        {
            await SetupAuth(client);

            client.SetCredentials(AdminUsername, AdminPassword);

            // Create a role with read-only permission
            await client.RoleAddAsync(new AuthRoleAddRequest { Name = roleName });

            await client.RoleGrantPermissionAsync(new AuthRoleGrantPermissionRequest
            {
                Name = roleName,
                Perm = new Permission
                {
                    PermType = Permission.Types.Type.Read,
                    Key = ByteString.CopyFromUtf8(keyPrefix),
                    RangeEnd = ByteString.CopyFromUtf8(EtcdClient.GetRangeEnd(keyPrefix))
                }
            });

            // Create user and grant role
            await client.UserAddAsync(new AuthUserAddRequest
            {
                Name = TestUsername,
                Password = TestPassword
            });

            await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest
            {
                User = TestUsername,
                Role = roleName
            });

            // Act - Try to read with test user (should succeed)
            client.SetCredentials(TestUsername, TestPassword);

            // Put a key as admin first
            client.SetCredentials(AdminUsername, AdminPassword);
            await client.PutAsync(keyPrefix + "testkey", "testvalue");

            // Read as test user
            client.SetCredentials(TestUsername, TestPassword);
            var readResponse = await client.GetAsync(keyPrefix + "testkey");

            Assert.NotNull(readResponse);
            Assert.Single(readResponse.Kvs);

            // Try to write (should fail - permission denied)
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.PutAsync(keyPrefix + "newkey", "newvalue"));

            Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task DeleteUser_ShouldInvalidateCredentials()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);
            await CreateTestUserWithRootRole(client);

            // Verify user can authenticate
            client.SetCredentials(TestUsername, TestPassword);
            var response1 = await client.GetAsync("test");
            Assert.NotNull(response1);

            // Act - Delete the user as admin
            client.SetCredentials(AdminUsername, AdminPassword);
            await client.UserDeleteAsync(new AuthUserDeleteRequest { Name = TestUsername });

            // Try to authenticate with deleted user credentials
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.AuthenticateAsync(new AuthenticateRequest
                {
                    Name = TestUsername,
                    Password = TestPassword
                }));

            // Assert
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task ListUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);

        try
        {
            await SetupAuth(client);

            client.SetCredentials(AdminUsername, AdminPassword);

            // Create multiple users
            await client.UserAddAsync(new AuthUserAddRequest { Name = "user1", Password = "pass1" });
            await client.UserAddAsync(new AuthUserAddRequest { Name = "user2", Password = "pass2" });
            await client.UserAddAsync(new AuthUserAddRequest { Name = "user3", Password = "pass3" });

            // Act
            var listResponse = await client.UserListAsync(new AuthUserListRequest());

            // Assert
            Assert.NotNull(listResponse);
            Assert.True(listResponse.Users.Count >= 4); // root + 3 created users
            Assert.Contains("user1", listResponse.Users);
            Assert.Contains("user2", listResponse.Users);
            Assert.Contains("user3", listResponse.Users);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    [Fact]
    public async Task RevokeUserRole_ShouldRemovePermissions()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(client);
        const string roleName = "testrole";

        try
        {
            await SetupAuth(client);

            client.SetCredentials(AdminUsername, AdminPassword);

            // Create role with root permission
            await client.RoleAddAsync(new AuthRoleAddRequest { Name = roleName });
            await client.RoleGrantPermissionAsync(new AuthRoleGrantPermissionRequest
            {
                Name = roleName,
                Perm = new Permission
                {
                    PermType = Permission.Types.Type.Readwrite,
                    Key = ByteString.CopyFromUtf8("\0"),
                    RangeEnd = ByteString.CopyFromUtf8("\0")
                }
            });

            // Create user with role
            await client.UserAddAsync(new AuthUserAddRequest { Name = TestUsername, Password = TestPassword });
            await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest { User = TestUsername, Role = roleName });

            // Verify user can perform operations
            client.SetCredentials(TestUsername, TestPassword);
            var response1 = await client.PutAsync("test", "value");
            Assert.NotNull(response1);

            // Act - Revoke role from user
            client.SetCredentials(AdminUsername, AdminPassword);
            await client.UserRevokeRoleAsync(new AuthUserRevokeRoleRequest
            {
                Name = TestUsername,
                Role = roleName
            });

            // Try to perform operation without role
            client.SetCredentials(TestUsername, TestPassword);
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.PutAsync("test2", "value2"));

            // Assert
            Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
        }
        finally
        {
            await CleanupAuth(client);
            client.Dispose();
        }
    }

    #endregion

    #region Concurrent Client Tests

    [Fact]
    public async Task MultipleConcurrentClients_WithDifferentCredentials_ShouldWork()
    {
        // Arrange
        var adminClient = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(adminClient);
        var userClient = new EtcdClient("127.0.0.1:2379");
        _clientsToDispose.Add(userClient);

        try
        {
            await SetupAuth(adminClient);
            await CreateTestUserWithRootRole(adminClient);

            adminClient.SetCredentials(AdminUsername, AdminPassword);
            userClient.SetCredentials(TestUsername, TestPassword);

            // Act - Perform concurrent operations with different clients
            var adminTask = adminClient.PutAsync("admin-key", "admin-value");
            var userTask = userClient.PutAsync("user-key", "user-value");

            await Task.WhenAll(adminTask, userTask);

            // Verify both operations succeeded
            var adminRange = await adminClient.GetAsync("admin-key");
            var userRange = await userClient.GetAsync("user-key");

            // Assert
            Assert.Single(adminRange.Kvs);
            Assert.Equal("admin-value", adminRange.Kvs[0].Value.ToStringUtf8());
            Assert.Single(userRange.Kvs);
            Assert.Equal("user-value", userRange.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            await CleanupAuth(adminClient);
            adminClient.Dispose();
            userClient.Dispose();
        }
    }

    #endregion

    #region Helper Methods

    private static async Task CreateRootUser(EtcdClient client)
    {
        try
        {
            // Create root user
            await client.UserAddAsync(new AuthUserAddRequest
            {
                Name = "root",
                Password = "root",
                Options = new UserAddOptions { NoPassword = false }
            });

            // Grant root role to root user
            await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest
            {
                User = "root",
                Role = "root"
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            // User might already exist, ignore
        }
    }

    private static async Task SetupAuth(EtcdClient client)
    {
        // Disable auth first (in case it was left enabled from a previous test)
        try
        {
            client.SetCredentials("root", "root");
            await client.AuthDisableAsync(new AuthDisableRequest());
        }
        catch
        {
            // Ignore errors (auth might already be disabled)
        }

        // Create root user if needed
        await CreateRootUser(client);

        // Create admin user
        try
        {
            await client.UserAddAsync(new AuthUserAddRequest
            {
                Name = AdminUsername,
                Password = AdminPassword
            });

            await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest
            {
                User = AdminUsername,
                Role = "root"
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            // User might already exist, ignore
        }

        // Enable auth
        await client.AuthEnableAsync(new AuthEnableRequest());
    }

    private static async Task CreateTestUserWithRootRole(EtcdClient client)
    {
        client.SetCredentials(AdminUsername, AdminPassword);

        try
        {
            await client.UserAddAsync(new AuthUserAddRequest
            {
                Name = TestUsername,
                Password = TestPassword
            });

            await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest
            {
                User = TestUsername,
                Role = "root"
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            // User might already exist, ignore
        }
    }

    private static async Task CleanupAuth(EtcdClient client)
    {
        try
        {
            // First, try to disable auth without credentials (in case auth is already disabled)
            try
            {
                await client.AuthDisableAsync(new AuthDisableRequest());
            }
            catch
            {
                // Auth might be enabled, try with credentials
                var credentialSets = new[]
                {
                    (AdminUsername, AdminPassword),
                    ("root", "root"),
                    (TestUsername, TestPassword)
                };

                foreach (var (username, password) in credentialSets)
                {
                    try
                    {
                        // Create a new client with credentials to disable auth
                        using var authClient = new EtcdClient("127.0.0.1:2379", username, password);
                        await authClient.AuthDisableAsync(new AuthDisableRequest());
                        break; // Success, exit loop
                    }
                    catch
                    {
                        // Try next credentials
                    }
                }
            }

            // Clean up users (auth must be disabled to do this)
            try
            {
                var userListResponse = await client.UserListAsync(new AuthUserListRequest());
                foreach (var user in userListResponse.Users)
                {
                    if (user != "root") // Don't delete root user
                    {
                        try
                        {
                            await client.UserDeleteAsync(new AuthUserDeleteRequest { Name = user });
                        }
                        catch
                        {
                            // Ignore errors deleting individual users
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors listing/deleting users
            }

            // Clean up roles
            try
            {
                var roleListResponse = await client.RoleListAsync(new AuthRoleListRequest());
                foreach (var role in roleListResponse.Roles)
                {
                    if (role != "root") // Don't delete root role
                    {
                        try
                        {
                            await client.RoleDeleteAsync(new AuthRoleDeleteRequest { Role = role });
                        }
                        catch
                        {
                            // Ignore errors deleting individual roles
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors listing/deleting roles
            }

            // Clean up any test keys
            try
            {
                await client.DeleteRangeAsync("\0");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        catch
        {
            // Ignore all cleanup errors
        }
    }

    #endregion
}
