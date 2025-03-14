using System;
using dotnet_etcd.interfaces;
using Grpc.Core;

namespace dotnet_etcd
{
    /// <summary>
    /// Adapter for AsyncDuplexStreamingCall that implements IAsyncDuplexStreamingCall
    /// </summary>
    /// <typeparam name="TRequest">The type of the request messages.</typeparam>
    /// <typeparam name="TResponse">The type of the response messages.</typeparam>
    public class AsyncDuplexStreamingCallAdapter<TRequest, TResponse> : IAsyncDuplexStreamingCall<TRequest, TResponse>
    {
        private readonly AsyncDuplexStreamingCall<TRequest, TResponse> _call;

        /// <summary>
        /// Creates a new AsyncDuplexStreamingCallAdapter
        /// </summary>
        /// <param name="call">The call to wrap</param>
        public AsyncDuplexStreamingCallAdapter(AsyncDuplexStreamingCall<TRequest, TResponse> call)
        {
            _call = call ?? throw new ArgumentNullException(nameof(call));
        }

        /// <summary>
        /// Gets the request stream.
        /// </summary>
        public IClientStreamWriter<TRequest> RequestStream => _call.RequestStream;

        /// <summary>
        /// Gets the response stream.
        /// </summary>
        public IAsyncStreamReader<TResponse> ResponseStream => _call.ResponseStream;

        /// <summary>
        /// Gets the call headers.
        /// </summary>
        public Metadata Headers => _call.ResponseHeadersAsync.Result;

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status Status => _call.GetStatus();

        /// <summary>
        /// Gets the call trailing metadata.
        /// </summary>
        public Metadata Trailers => _call.GetTrailers();

        /// <summary>
        /// Disposes the call.
        /// </summary>
        public void Dispose()
        {
            _call.Dispose();
        }
    }
}
