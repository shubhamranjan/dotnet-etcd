using System.Collections.Concurrent;
using System.Threading.Channels;
using dotnet_etcd.interfaces;
using Grpc.Core;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     An <see cref="IClientStreamWriter{T}" /> that records every request written to it so that
///     tests can assert which requests (e.g. WatchCreate / WatchCancel) were sent over the stream.
/// </summary>
public class RecordingClientStreamWriter<T> : IClientStreamWriter<T>
{
    private readonly ConcurrentQueue<T> _written = new();

    /// <summary>All requests written to the stream, in order.</summary>
    public IReadOnlyCollection<T> Written => _written;

    /// <summary>True once <see cref="CompleteAsync" /> has been called.</summary>
    public bool IsCompleted { get; private set; }

    public WriteOptions? WriteOptions { get; set; }

    public Task WriteAsync(T message)
    {
        _written.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task WriteAsync(T message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _written.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task CompleteAsync()
    {
        IsCompleted = true;
        return Task.CompletedTask;
    }
}

/// <summary>
///     An <see cref="IAsyncStreamReader{T}" /> backed by an unbounded channel so that a test can
///     drive a background receive loop by enqueuing responses and completing the stream on demand.
///     Optionally configured to throw a supplied exception from <see cref="MoveNext" /> to exercise
///     stream-error handling paths.
/// </summary>
public class ChannelStreamReader<T> : IAsyncStreamReader<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();
    private T _current = default!;

    /// <summary>When set, <see cref="MoveNext" /> throws this exception instead of reading.</summary>
    public Exception? ThrowOnMoveNext { get; set; }

    public T Current => _current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        if (ThrowOnMoveNext != null)
        {
            throw ThrowOnMoveNext;
        }

        if (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false) &&
            _channel.Reader.TryRead(out T? item))
        {
            _current = item;
            return true;
        }

        return false;
    }

    public void Enqueue(T item) => _channel.Writer.TryWrite(item);

    public void Complete() => _channel.Writer.TryComplete();
}

/// <summary>
///     A controllable fake implementation of <see cref="IAsyncDuplexStreamingCall{TRequest,TResponse}" />.
///     The request stream records written requests; the response stream is driven by a channel so the
///     test can <see cref="Enqueue" /> responses and <see cref="Complete" /> the stream.
/// </summary>
public class FakeDuplexStreamingCall<TRequest, TResponse> : IAsyncDuplexStreamingCall<TRequest, TResponse>
{
    public RecordingClientStreamWriter<TRequest> Requests { get; } = new();
    public ChannelStreamReader<TResponse> Responses { get; } = new();

    /// <summary>True once <see cref="Dispose" /> has been called.</summary>
    public bool IsDisposed { get; private set; }

    public IClientStreamWriter<TRequest> RequestStream => Requests;
    public IAsyncStreamReader<TResponse> ResponseStream => Responses;

    public Task<Metadata> GetHeadersAsync() => Task.FromResult(new Metadata());

    public Status GetStatus() => Status.DefaultSuccess;

    public Metadata GetTrailers() => new();

    public void Dispose()
    {
        IsDisposed = true;
        Responses.Complete();
        GC.SuppressFinalize(this);
    }

    /// <summary>Pushes a response into the response stream for the receive loop to pick up.</summary>
    public void Enqueue(TResponse response) => Responses.Enqueue(response);

    /// <summary>Ends the response stream so the receive loop completes.</summary>
    public void Complete() => Responses.Complete();
}
