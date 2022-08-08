// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.Serialization;

namespace dotnet_etcd
{
    [Serializable]
    public class LeaseExpiredOrNotFoundException : Exception
    {
        private readonly long _leaseId;

        public LeaseExpiredOrNotFoundException(long leaseId)
        {
            _leaseId = leaseId;
        }

        public LeaseExpiredOrNotFoundException(long leaseId, string message) : base(message)
        {
            _leaseId = leaseId;
        }

        public LeaseExpiredOrNotFoundException(long leaseId, string message, Exception inner) : base(
            message,
            inner)
        {
            _leaseId = leaseId;
        }

        protected LeaseExpiredOrNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(
            info,
            context)
        {
        }
    }
}
