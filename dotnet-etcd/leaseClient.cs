using System;
using System.Threading.Tasks;
using Etcdserverpb;

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
            catch (Grpc.Core.RpcException)
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
            catch (Grpc.Core.RpcException)
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
