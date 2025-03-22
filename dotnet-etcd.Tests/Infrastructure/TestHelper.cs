using System.Reflection;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;
using System.Collections.Generic;

namespace dotnet_etcd.Tests.Infrastructure;

/// <summary>
///     Helper class for setting up mocks and common test functionality
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
    
    /// <summary>
    ///     Creates a sample KeyValue for testing
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>A KeyValue instance</returns>
    public static KeyValue CreateKeyValue(string key, string value)
    {
        return new KeyValue
        {
            Key = ByteString.CopyFromUtf8(key),
            Value = ByteString.CopyFromUtf8(value)
        };
    }
    
    /// <summary>
    ///     Creates a sample RangeResponse for testing
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>A RangeResponse with a single KeyValue</returns>
    public static RangeResponse CreateRangeResponse(string key, string value)
    {
        var response = new RangeResponse();
        response.Kvs.Add(CreateKeyValue(key, value));
        return response;
    }
    
    /// <summary>
    ///     Creates an AsyncUnaryCall for mocking async responses
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="response">The response object</param>
    /// <returns>An AsyncUnaryCall instance</returns>
    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );
    }
    
    /// <summary>
    ///     Setups common connection parameters for integration tests
    /// </summary>
    /// <returns>A dictionary of connection parameters</returns>
    public static Dictionary<string, string> GetIntegrationTestConnectionParameters()
    {
        return new Dictionary<string, string>
        {
            {"endpoints", "127.0.0.1:2379"},
            {"username", ""},
            {"password", ""}
        };
    }
}