// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.Serialization;

namespace dotnet_etcd
{
    [Serializable]
    public class LeaseExpiredException : Exception
    {
        public LeaseExpiredException()
        {
        }

        public LeaseExpiredException(string message) : base(message)
        {
        }

        public LeaseExpiredException(string message, Exception inner) : base(
            message,
            inner)
        {
        }

        protected LeaseExpiredException(
            SerializationInfo info,
            StreamingContext context) : base(
            info,
            context)
        {
        }
    }
}
