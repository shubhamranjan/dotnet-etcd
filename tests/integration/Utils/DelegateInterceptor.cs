using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Integration.Utils;

internal class DelegateInterceptor<TReq, TRsp> : Interceptor
    where TReq : class
    where TRsp : class
{
    private readonly MessageStore _requestsStore = new();
    private readonly MessageStore _responsesStore = new();
    private readonly bool _ignoreCallId = false;

    public DelegateInterceptor(bool ignoreCallId = false)
    {
        _ignoreCallId = ignoreCallId;
    }
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        ValidateCall<TRequest, TResponse>();
        Guid callId = _ignoreCallId? Guid.Empty : Guid.NewGuid();
        string address = GetEtcdAdders(continuation);
        _requestsStore.WriteAsync(
            address, callId,
            request).Wait();
        _requestsStore.Complete(address, callId);
        var enumerator = _responsesStore.GetReader<TResponse>(address,callId);
        if (!enumerator.MoveNext().Result) throw new Exception("response required");
        var response = enumerator.Current;
        _responsesStore.Complete(address,callId);
        var call = new AsyncUnaryCall<TResponse>(
            responseAsync: Task.FromResult(response),
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => new Status(
                statusCode: StatusCode.OK,
                detail: ""),
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { });
        return call;
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        ValidateCall<TRequest, TResponse>();
        Guid callId = _ignoreCallId? Guid.Empty : Guid.NewGuid();
        string address = GetEtcdAdders(continuation);
        _requestsStore.WriteAsync(
            address,callId,
            request).Wait();
        _requestsStore.Complete(address, callId);
        var enumerator = _responsesStore.GetReader<TResponse>(address,callId);
        if (!enumerator.MoveNext().Result) throw new Exception("response required");
        _responsesStore.Complete(address,callId);
        return enumerator.Current;
    }
    
    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        ValidateCall<TRequest, TResponse>();
        Guid callId = _ignoreCallId? Guid.Empty : Guid.NewGuid();
        string address = GetEtcdAdders(continuation);
        var reader = _responsesStore.GetReader<TResponse>(address,callId);
        var call = new AsyncClientStreamingCall<TRequest, TResponse>(
            requestStream: _requestsStore.GetWriter<TRequest>(address, callId),
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => new Status(
                statusCode: StatusCode.OK,
                detail: ""),
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { },
            responseAsync: Task.Run(
                async () =>
                {
                    await reader.MoveNext();
                    _responsesStore.Complete(address,callId);
                    return reader.Current;
                }));
        return call;
    }
    
    
    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        ValidateCall<TRequest, TResponse>();
        Guid callId = _ignoreCallId? Guid.Empty : Guid.NewGuid();
        string address = GetEtcdAdders(continuation);
        _requestsStore.WriteAsync(
            address,callId,
            request).Wait();
        _requestsStore.Complete(address, callId);
        var call = new AsyncServerStreamingCall<TResponse>(
            responseStream: _responsesStore.GetReader<TResponse>(address,callId),
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => new Status(
                statusCode: StatusCode.OK,
                detail: ""),
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { });
        return call;
    }
    
    
    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        ValidateCall<TRequest, TResponse>();
        Guid callId = _ignoreCallId? Guid.Empty : Guid.NewGuid();
        string address = GetEtcdAdders(continuation);
        AsyncDuplexStreamingCall<TRequest, TResponse> call = new(
            requestStream: _requestsStore.GetWriter<TRequest>(address,callId),
            responseStream: _responsesStore.GetReader<TResponse>(address,callId),
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => new Status(
                statusCode: StatusCode.OK,
                detail: ""),
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { });
        return call;
    }


    public async Task WriteResponseAsync(string address,Guid callId, TRsp rsp)
    {
        await _responsesStore.WriteAsync(address, callId,rsp);
    }

    public async Task WriteResponseAsync(string address, Guid callId, Exception exception)
    {
        await _responsesStore.WriteAsync(
            address, callId,
            null,
            exception);
    }

    public async Task CloseResponseStreamAsync(string address, Guid callId)
    {
        _responsesStore.Complete(address,callId);
    }

    public IAsyncEnumerable<TReq> ReadAllRequests(string address, Guid callId, CancellationToken cancellationToken)
    {
        return _requestsStore.GetReader<TReq>(address, callId).ReadAllAsync(cancellationToken);
    }
    
    public async IAsyncEnumerable<(string address, Guid callId, TReq message, bool closed)> ReadAllRequests(CancellationToken cancellationToken)
    {
        await foreach (var (address, callId, message, exception, closed) in _requestsStore.ReadMessages())
        {
            if (exception != null) throw exception;
            yield return (address, callId, (TReq)message!, closed);
        }
    }

    private static void ValidateCall<TRequest, TResponse>()
    {
        if (typeof(TReq) != typeof(TRequest) || typeof(TRsp) != typeof(TResponse))
            throw new Exception("Interceptor not applicable to these call");
    }

    private static string GetEtcdAdders(Delegate continuation)
    {
        object target = continuation.Target!;
        object invoker = target.GetType().GetField("invoker",BindingFlags.Instance|BindingFlags.NonPublic)!
            .GetValue(target)!;
        GrpcChannel channel = (GrpcChannel)invoker.GetType()
            .GetProperty(
                "Channel",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(invoker)!;
        return channel.Target;
    }
}