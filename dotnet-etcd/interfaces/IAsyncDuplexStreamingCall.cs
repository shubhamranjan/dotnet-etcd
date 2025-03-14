using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace dotnet_etcd.interfaces
{
    /// <summary>
    /// Interface for AsyncDuplexStreamingCall to make it mockable for testing
    /// </summary>
    /// <typeparam name="TRequest">The type of the request messages.</typeparam>
    /// <typeparam name="TResponse">The type of the response messages.</typeparam>
    public interface IAsyncDuplexStreamingCall<TRequest, TResponse> : IDisposable
    {
        /// <summary>
        /// Gets the request stream.
        /// </summary>
        IClientStreamWriter<TRequest> RequestStream { get; }

        /// <summary>
        /// Gets the response stream.
        /// </summary>
        IAsyncStreamReader<TResponse> ResponseStream { get; }

        /// <summary>
        /// Gets the call headers.
        /// </summary>
        Metadata Headers { get; }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        Status Status { get; }

        /// <summary>
        /// Gets the call trailing metadata.
        /// </summary>
        Metadata Trailers { get; }
    }
}
