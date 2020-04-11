﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using dotnet_etcd.multiplexer;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// Converts RangeResponse to Dictionary
        /// </summary>
        /// <returns>IDictionary corresponding the RangeResponse</returns>
        /// <param name="resp">RangeResponse received from etcd server</param>
        private static IDictionary<string, string> RangeRespondToDictionary(RangeResponse resp)
        {
            Dictionary<string, string> resDictionary = new Dictionary<string, string>();
            foreach (Mvccpb.KeyValue kv in resp.Kvs)
            {
                resDictionary.Add(kv.Key.ToStringUtf8(), kv.Value.ToStringUtf8());
            }

            return resDictionary;
        }

        /// <summary>
        /// Gets the range end for prefix
        /// </summary>
        /// <returns>The range end for prefix</returns>
        /// <param name="prefixKey">Prefix key</param>
        public static string GetRangeEnd(string prefixKey)
        {
            StringBuilder rangeEnd = new StringBuilder(prefixKey);
            rangeEnd[rangeEnd.Length - 1] = ++rangeEnd[rangeEnd.Length - 1];
            return rangeEnd.ToString();
        }

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private TResponse CallEtcd<TResponse>(Func<Connection, TResponse> etcdCallFunc)
        {
            TResponse response;
            var retryCount = 0;
            while (true)
            {
                try
                {
                    response = etcdCallFunc.Invoke(_balancer.GetConnection());
                    break;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private async Task<TResponse> CallEtcdAsync<TResponse>(Func<Connection, Task<TResponse>> etcdCallFunc)
        {
            TResponse response;
            var retryCount = 0;
            while (true)
            {
                try
                {
                    response = await etcdCallFunc.Invoke(_balancer.GetConnection());
                    break;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private async Task CallEtcdAsync(Func<Connection, Task> etcdCallFunc)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    await etcdCallFunc.Invoke(_balancer.GetConnection());
                    break;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }
        }
    }
}