using System.Reflection;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Infrastructure;

/// <summary>
///     Helper class for setting up mocks in unit tests
/// </summary>
public static class TestHelper
{
    /// <summary>
    ///     Sets up a mock client on the EtcdClient instance
    /// </summary>
    /// <typeparam name="T">The type of client to mock</typeparam>
    /// <param name="etcdClient">The EtcdClient instance</param>
    /// <param name="mockClient">The mock client to set</param>
    /// <param name="fieldName">The name of the field to set on the Connection object</param>
    public static void SetupMockClientViaConnection<T>(EtcdClient etcdClient, T mockClient, string fieldName)
        where T : class
    {
        // Get the _connection field from EtcdClient
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var connection = connectionField?.GetValue(etcdClient);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the client field on the Connection object
        var clientField = connection.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (clientField == null)
            throw new InvalidOperationException($"Failed to get {fieldName} field from Connection");

        clientField.SetValue(connection, mockClient);
    }

    /// <summary>
    ///     Creates an EtcdClient with a mock CallInvoker
    /// </summary>
    /// <returns>An EtcdClient instance with a mock CallInvoker</returns>
    public static EtcdClient CreateEtcdClientWithMockCallInvoker()
    {
        var mockCallInvoker = new Mock<CallInvoker>();
        return new EtcdClient(mockCallInvoker.Object);
    }
}