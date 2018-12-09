using Etcdserverpb;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// LeaseGrant creates a lease which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request)
        {
            try
            {
                return _leaseClient.LeaseGrant(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request)
        {
            try
            {
                return await _leaseClient.LeaseGrantAsync(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request)
        {
            try
            {
                return _leaseClient.LeaseRevoke(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request)
        {
            try
            {
                return await _leaseClient.LeaseRevokeAsync(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }
    }
}
