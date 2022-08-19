using System;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Grpc.Core;
using Integration.Utils;
using NUnit.Framework;

namespace Integration;

public class Lease
{
    private const string Etcd1 = "127.0.0.1:23790";
    private const string Etcd2 = "127.0.0.1:23791";
    private const string Etcd3 = "127.0.0.1:23792";
    private const string ConnectionString = $"http://{Etcd1},http://{Etcd2},http://{Etcd3}";

    [SetUp]
    public async Task Setup()
    {
    }

    [TearDown]
    public async Task TearDownAsync()
    {
    }
    
    
    [Test]
    public async Task LeaseGranted()
    {
        var delegateInterceptor = new DelegateInterceptor<LeaseGrantRequest, LeaseGrantResponse>();
        var client = new EtcdClient(
            connectionString: ConnectionString,
            interceptors: delegateInterceptor);
        LeaseGrantRequest request = new() { ID = 777, TTL = 777 };
        LeaseGrantResponse response = new() { ID = 888, TTL = 888 };
        var responseTask = Task.Run(() =>
        {
            return client.LeaseGrant(request);
        });
        await foreach ((string address, Guid callId, LeaseGrantRequest message, bool _) in delegateInterceptor.ReadAllRequests(CancellationToken.None))
        {
            Assert.AreEqual(message, request);
            await delegateInterceptor.WriteResponseAsync(address, callId,response);
            break;
        }

        var rsp = responseTask.Result;
        Assert.AreEqual(rsp, response);
    }
    
    [Test]
    public async Task LeaseGrantedAsync()
    {
        var delegateInterceptor = new DelegateInterceptor<LeaseGrantRequest, LeaseGrantResponse>();
        var client = new EtcdClient(
            connectionString: ConnectionString,
            interceptors: delegateInterceptor);
        LeaseGrantRequest request = new() { ID = 777, TTL = 777 };
        LeaseGrantResponse response = new() { ID = 888, TTL = 888 };
        var responseTask = Task.Run(() =>
        {
            return client.LeaseGrantAsync(request);
        });
        await foreach ((string address, Guid callId, LeaseGrantRequest message, bool _) in delegateInterceptor.ReadAllRequests(CancellationToken.None))
        {
            Assert.AreEqual(message, request);
            await delegateInterceptor.WriteResponseAsync(address, callId,response);
            break;
        }

        var rsp = responseTask.Result;
        Assert.AreEqual(rsp, response);
    }
     
     [Test]
     public async Task AfterExceptionsLeaseGranted()
     {
         var delegateInterceptor = new DelegateInterceptor<LeaseGrantRequest, LeaseGrantResponse>();
         var client = new EtcdClient(
             connectionString: ConnectionString,
             interceptors: delegateInterceptor);
         LeaseGrantRequest request = new() { ID = 777, TTL = 777 };
         LeaseGrantResponse response = new() { ID = 888, TTL = 888 };
         var responseTask = Task.Run(() =>
         {
             return client.LeaseGrantAsync(request);
         });
         RpcException unavailableException = new RpcException(
             new Status(
                 StatusCode.Unavailable,
                 ""));

         var iterator = delegateInterceptor.ReadAllRequests(CancellationToken.None).GetAsyncEnumerator();
         await iterator.MoveNextAsync();
         var current = iterator.Current;
         await delegateInterceptor.WriteResponseAsync(current.address, current.callId,unavailableException);
         await iterator.MoveNextAsync();
         current = iterator.Current;
         await delegateInterceptor.WriteResponseAsync(current.address, current.callId,unavailableException);
         await iterator.MoveNextAsync();
         current = iterator.Current;
         await delegateInterceptor.WriteResponseAsync(current.address, current.callId,response);
         var rsp = responseTask.Result;
         
         Assert.AreEqual(rsp, response);
     }

     [Test]
     public async Task AfterThreeExceptionsLeaseGrantedFail()
     {
         var delegateInterceptor = new DelegateInterceptor<LeaseGrantRequest, LeaseGrantResponse>(ignoreCallId: true);
         var client = new EtcdClient(
             connectionString: ConnectionString,
             interceptors:
             delegateInterceptor);

         RpcException unavailableException = new RpcException(
             new Status(
                 StatusCode.Unavailable,
                 ""));
         await delegateInterceptor.WriteResponseAsync(Etcd1, Guid.Empty, unavailableException); 
         await delegateInterceptor.WriteResponseAsync(Etcd2, Guid.Empty,unavailableException); 
         await delegateInterceptor.WriteResponseAsync(Etcd3, Guid.Empty,unavailableException);
         LeaseGrantRequest request = new() { ID = 777, TTL = 777 };
         
         var ex = Assert.Throws<RpcException>(
             () =>
             {
                 try
                 {
                     client.LeaseGrantAsync(request).Wait();
                 }
                 catch (AggregateException e)
                 {
     
                     throw e.InnerException;
                 }
             });
         Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.Unavailable));
     }
     
     [Test]
     public async Task LeaseKeepAliveRequestSendedAfterDelay()
     {
         var delegateInterceptor = new DelegateInterceptor<LeaseKeepAliveRequest, LeaseKeepAliveResponse>(true);
         var client = new EtcdClient(
             connectionString: ConnectionString,
             interceptors:
             delegateInterceptor);
         var responseTask = Task.Run(
             () =>
             {
                 return client.LeaseKeepAlive(
                     777,
                     CancellationToken.None);
             });
         var iterator = delegateInterceptor.ReadAllRequests(CancellationToken.None).GetAsyncEnumerator();
         await iterator.MoveNextAsync();
         Assert.AreEqual(iterator.Current.message.ID, 777);
         await delegateInterceptor.WriteResponseAsync(iterator.Current.address, Guid.Empty,new LeaseKeepAliveResponse() { ID = 777, TTL = 1 });
         var nextKeepAliveTask = iterator.MoveNextAsync();
         await Task.Delay(100);
         Assert.True(nextKeepAliveTask.IsCompleted == false);
         await Task.Delay(300);
         Assert.True(nextKeepAliveTask.Result);
     }
}
