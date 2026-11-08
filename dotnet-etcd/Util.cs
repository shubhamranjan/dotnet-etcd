using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.multiplexer;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;

namespace dotnet_etcd;

public partial class EtcdClient
{
    /// <summary>
    ///     Converts RangeResponse to Dictionary
    /// </summary>
    /// <returns>IDictionary corresponding the RangeResponse</returns>
    /// <param name="resp">RangeResponse received from etcd server</param>
    private static Dictionary<string, string> RangeRespondToDictionary(RangeResponse resp)
    {
        Dictionary<string, string> resDictionary = new();
        foreach (KeyValue kv in resp.Kvs)
        {
            resDictionary.Add(kv.Key.ToStringUtf8(), kv.Value.ToStringUtf8());
        }

        return resDictionary;
    }

    /// <summary>
    ///     Gets the range end for a prefix: the smallest key greater than every key that starts with
    ///     <paramref name="prefix" />, computed on the raw bytes exactly as etcd's own client does
    ///     (increment the last byte below 0xFF and truncate after it). An empty prefix, or one made
    ///     only of 0xFF bytes, has no upper bound and yields <c>"\0"</c>, which etcd reads as
    ///     "to the end of the key space".
    /// </summary>
    /// <param name="prefix">The prefix, as the exact bytes that will be sent as the range key.</param>
    /// <returns>The range end to send alongside <paramref name="prefix" />.</returns>
    public static ByteString GetRangeEnd(ByteString prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        byte[] end = prefix.ToByteArray();
        for (int i = end.Length - 1; i >= 0; i--)
        {
            if (end[i] < 0xff)
            {
                end[i]++;
                return ByteString.CopyFrom(end, 0, i + 1);
            }
        }

        return ByteString.CopyFrom(0);
    }

    /// <summary>
    ///     String form of <see cref="GetRangeEnd(ByteString)" />. The range end is computed on the
    ///     prefix's UTF-8 bytes and returned as a string when those bytes are themselves valid UTF-8,
    ///     which is always the case for ASCII prefixes. When they are not (a prefix ending in
    ///     <c>ÿ</c>, for instance, has the range end bytes <c>C3 C0</c>) no string can carry the
    ///     result and an <see cref="ArgumentException" /> is thrown: build the request with the
    ///     <see cref="ByteString" /> overload instead. The prefix-based Get/Delete/Watch methods on
    ///     this client already do.
    /// </summary>
    /// <param name="prefixKey">Prefix key</param>
    /// <returns>The range end for prefix</returns>
    public static string GetRangeEnd(string prefixKey)
    {
        ArgumentNullException.ThrowIfNull(prefixKey);

        ByteString end = GetRangeEnd(ByteString.CopyFromUtf8(prefixKey));
        try
        {
            return new UTF8Encoding(false, true).GetString(end.Span);
        }
        catch (DecoderFallbackException ex)
        {
            throw new ArgumentException(
                $"The range end for prefix \"{prefixKey}\" is not valid UTF-8 and cannot be returned as a " +
                "string; use GetRangeEnd(ByteString) and set RangeEnd on the request directly.",
                nameof(prefixKey), ex);
        }
    }

    /// <summary>
    ///     Converts a key to the bytes sent in a range request. An empty key becomes <c>"\0"</c>,
    ///     which etcd treats as the start of the key space (used for "all keys" ranges).
    /// </summary>
    /// <param name="key">The key or prefix.</param>
    /// <returns>The key bytes to put on the request.</returns>
    public static ByteString GetStringByteForRangeRequests(string key) =>
        key.Length == 0 ? ByteString.CopyFrom(0) : ByteString.CopyFromUtf8(key);

    /// <summary>
    ///     Runs <paramref name="etcdCallFunc" /> against this client's <see cref="IConnection" />.
    ///     Retries for transient failures are configured on the gRPC channel, not here.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
    /// <param name="etcdCallFunc">The call to make with the connection's gRPC clients.</param>
    /// <returns>Whatever <paramref name="etcdCallFunc" /> returns.</returns>
    private TResponse CallEtcd<TResponse>(Func<IConnection, TResponse> etcdCallFunc) => etcdCallFunc(_connection);

    /// <summary>
    ///     Runs <paramref name="etcdCallFunc" /> against this client's <see cref="IConnection" />.
    ///     Retries for transient failures are configured on the gRPC channel, not here.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
    /// <param name="etcdCallFunc">The call to make with the connection's gRPC clients.</param>
    /// <returns>Whatever <paramref name="etcdCallFunc" /> returns.</returns>
    private Task<TResponse> CallEtcdAsync<TResponse>(Func<IConnection, Task<TResponse>> etcdCallFunc) =>
        etcdCallFunc(_connection);

    /// <summary>
    ///     Runs <paramref name="etcdCallFunc" /> against this client's <see cref="IConnection" />.
    ///     Retries for transient failures are configured on the gRPC channel, not here.
    /// </summary>
    /// <param name="etcdCallFunc">The call to make with the connection's gRPC clients.</param>
    /// <returns>Whatever <paramref name="etcdCallFunc" /> returns.</returns>
    private Task CallEtcdAsync(Func<IConnection, Task> etcdCallFunc) => etcdCallFunc(_connection);
}
