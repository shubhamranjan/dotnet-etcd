// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;

using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {

        internal Balancer(List<Uri> nodes, HttpClientHandler handler = null, bool ssl = false,
            bool useLegacyRpcExceptionForCancellation = false, Interceptor[] interceptors = null, MethodConfig grpcMethodConfig = null, RetryThrottlingPolicy grpcRetryThrottlingPolicy = null)
        {
            foreach (Uri node in nodes)
            {






            }
        }

    }
}
