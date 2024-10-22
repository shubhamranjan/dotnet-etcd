using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Integration.Utils;

internal class MessageStore
{
    private readonly object _locker = new();
    private readonly List<(string address, Guid callId, object? message, Exception? exception, bool closed)> _store = new();
    private readonly List<Channel<(string addres, Guid callId, object? message, Exception? exception, bool closed)>> _readers = new();


    public async Task WriteAsync(string address, Guid callId, object? message, Exception? exception = null) =>
        await WriteAsync(
            address,
            callId,
            message,
            exception,
            false);

    private async Task WriteAsync(string address, Guid callId, object? message, Exception? exception, bool closed)
    {
        await Task.Run(
            () =>
            {
                lock (_locker)
                {
                    if (_store.Any(item => item.address == address && item.callId == callId && item.closed))
                        throw new InvalidOperationException("call closed already");
                    _store.Add((address, callId, message, exception, closed));
                    foreach (Channel<(string addres, Guid callId, object? message, Exception? exception, bool closed)> channel in _readers)
                    {
                        channel.Writer.TryWrite((address, callId, message, exception, closed));
                    }
                }
            });
    }

    public void Complete(string address, Guid callId) =>
         WriteAsync(
            address,
            callId,
            null,
            null,
            true).Wait();

    public async IAsyncEnumerable<(string address, Guid callId, object? message, Exception? exception, bool closed)> ReadMessages()
    {
        Channel<(string addres, Guid callId,object? message, Exception? exception, bool closed)>? channel = null;
        try
        {
            lock (_locker)
            {
                channel = Channel.CreateUnbounded<(string addres, Guid callId, object? message, Exception? exception, bool closed)>();
                _readers.Add(channel);
                List<(string address, Guid callId, object? message, Exception? exception, bool closed)> existingMessages = new(_store);
                foreach ((string address, Guid callId, object? message, Exception? exception, bool closed) in existingMessages)
                {
                    channel.Writer.TryWrite((address, callId, message, exception, closed));
                }
            }

            await foreach ((string addres, Guid callId, object? message, Exception? exception, bool closed) in channel.Reader.ReadAllAsync())
            {
                yield return (addres, callId, message, exception, closed);
            }
        }
        finally
        {
            lock (_locker)
            {
                if (channel != null)
                {
                    _readers.Remove(channel);
                }
            }
        }
    }

    public async IAsyncEnumerable<(object? message, Exception? exception)> ReadMessages(string address, Guid callId)
    {
        await foreach (var (addr, _callId, message, exception, closed) in ReadMessages())
        {
            if (addr == address && callId == _callId)
            {
                if(closed) yield break;
                yield return (message, exception);
            }
        }
    }



    public IClientStreamWriter<T> GetWriter<T>(string address, Guid callId)
    {
        return new DelegateStreamWriter<T>(
            async message => await WriteAsync(address, callId, message, null),
            async () =>
            {
               Complete(address, callId);
            });
    }

    public IAsyncStreamReader<T> GetReader<T>(string address, Guid callId)
    {
        return new StreamReader<T>(ReadMessages(address, callId).GetAsyncEnumerator());
    }
    
    
    
    private class DelegateStreamWriter<T> : IClientStreamWriter<T>
    {
        private readonly Func<T, Task> _onWrite;
        private readonly Func<Task> _onComplete;
    
        public DelegateStreamWriter(Func<T,Task> onWrite, Func<Task> onComplete)
        {
            _onWrite = onWrite;
            _onComplete = onComplete;
        }


        public async Task WriteAsync(T message)
        {
            await _onWrite(message);
        }
    

        public WriteOptions? WriteOptions { get; set; }
        public async Task CompleteAsync()
        {
            await _onComplete();
        }
    }

    private class StreamReader<T> : IAsyncStreamReader<T>
    {
        private readonly IAsyncEnumerator<(object? message, Exception? exception)> _enumerator;
        private readonly Func<T> _getCurrentFunc;

        public StreamReader(IAsyncEnumerator<(object?, Exception?)> enumerator)
        {
            _enumerator = enumerator;
            _getCurrentFunc = () => _enumerator.Current.exception == null
                ? (T)_enumerator.Current.message!
                : throw _enumerator.Current.exception;
        }
        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return await _enumerator.MoveNextAsync();
        }

        public T Current => _getCurrentFunc();
    }
}
