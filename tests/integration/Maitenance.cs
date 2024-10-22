using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Integration.Utils;
using NUnit.Framework;

namespace Integration;

public class Maitenance
{
    private const string Etcd1 = "127.0.0.1:23790";
    private const string Etcd2 = "127.0.0.1:23791";
    private const string Etcd3 = "127.0.0.1:23792";
    private const string ConnectionString = $"http://{Etcd1},http://{Etcd2},http://{Etcd3}";
    
    [Test]
    public async Task SnapshotTransferredConsistently()
    {
        var delegateInterceptor = new DelegateInterceptor<SnapshotRequest, SnapshotResponse>();
        var client = new EtcdClient(
            connectionString: ConnectionString,
            interceptors: delegateInterceptor);
        List<SnapshotResponse> originalSnapshot = new()
        {
            new SnapshotResponse
            {
                RemainingBytes = 30,
                Blob = ByteString.CopyFromUtf8("part1")
            },
            new SnapshotResponse
            {
                RemainingBytes = 20,
                Blob = ByteString.CopyFromUtf8("part2")
            },
            new SnapshotResponse
            {
                RemainingBytes = 10,
                Blob = ByteString.CopyFromUtf8("part3")
            },
        }; 
        
        SnapshotRequest request = new SnapshotRequest();
        List<SnapshotResponse> receivedSnapshot = new();
        var callTask = Task.Run(
            () => client.Snapshot(
                request,
                rsp =>
                {
                    receivedSnapshot.Add(rsp);
                },
                CancellationToken.None));
        await foreach (var (address, callId, message, _) in delegateInterceptor.ReadAllRequests(CancellationToken.None))
        {
            foreach (var snapshotPart in originalSnapshot)
            {
                await delegateInterceptor.WriteResponseAsync(address, callId, snapshotPart);
            }

            await delegateInterceptor.CloseResponseStreamAsync(
                address,
                callId);
            break;
        }
        await callTask;
        CollectionAssert.AreEqual(receivedSnapshot, originalSnapshot);
    }
    
    [Test]
    public async Task SnapshotTransferredConsistentlyThrowExceptions()
    {
        var delegateInterceptor = new DelegateInterceptor<SnapshotRequest, SnapshotResponse>(ignoreCallId: true);
        var client = new EtcdClient(
            connectionString: ConnectionString,
            interceptors: delegateInterceptor);

        var snapshotPart1 = new SnapshotResponse
        {
            RemainingBytes = 30,
            Blob = ByteString.CopyFromUtf8("part1")
        };
        var snapshotPart2 = new SnapshotResponse
        {
            RemainingBytes = 20,
            Blob = ByteString.CopyFromUtf8("part2")
        };
        var snapshotPart3 = new SnapshotResponse
        {
            RemainingBytes = 10,
            Blob = ByteString.CopyFromUtf8("part3")
        };
        List<SnapshotResponse> originalSnapshot = new()
        {
            snapshotPart1,
            snapshotPart2,
            snapshotPart3,
        };

        RpcException unavailableException = new RpcException(new Status(StatusCode.Unavailable, ""));
        await delegateInterceptor.WriteResponseAsync(Etcd1, Guid.Empty, snapshotPart1);
        await delegateInterceptor.WriteResponseAsync(Etcd1, Guid.Empty, unavailableException);
        await delegateInterceptor.CloseResponseStreamAsync(Etcd1, Guid.Empty);
        await delegateInterceptor.WriteResponseAsync(Etcd2, Guid.Empty, snapshotPart1);
        await delegateInterceptor.WriteResponseAsync(Etcd2, Guid.Empty, snapshotPart2);
        await delegateInterceptor.WriteResponseAsync(Etcd2, Guid.Empty, unavailableException);
        await delegateInterceptor.CloseResponseStreamAsync(Etcd2, Guid.Empty);
        await delegateInterceptor.WriteResponseAsync(Etcd3, Guid.Empty, snapshotPart1);
        await delegateInterceptor.WriteResponseAsync(Etcd3, Guid.Empty, snapshotPart2);
        await delegateInterceptor.WriteResponseAsync(Etcd3, Guid.Empty, snapshotPart3);
        await delegateInterceptor.CloseResponseStreamAsync(Etcd3, Guid.Empty);

        
         SnapshotRequest request = new SnapshotRequest();
         List<SnapshotResponse> receivedSnapshot = new();
        await client.Snapshot(
            request,
            rsp =>
            {
                receivedSnapshot.Add(rsp);
            },
            CancellationToken.None);
        
        CollectionAssert.AreEqual(receivedSnapshot, originalSnapshot);
        // test failed because current stream retry didnt correct
    }
}