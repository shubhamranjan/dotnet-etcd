using System;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        ///  MemberAdd adds a member into the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberAddResponse MemberAdd(MemberAddRequest request, Metadata headers = null)
        {
            MemberAddResponse response = new MemberAddResponse();

            response = _balancer.GetConnection().clusterClient.MemberAdd(request, headers);

            return response;
        }

        /// <summary>
        ///  MemberAddAsync adds a member into the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Metadata headers = null)
        {
            MemberAddResponse response = new MemberAddResponse();

            response = await _balancer.GetConnection().clusterClient.MemberAddAsync(request, headers);

            return response;
        }

        /// <summary>
        /// MemberRemove removes an existing member from the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Metadata headers = null)
        {
            MemberRemoveResponse response = new MemberRemoveResponse();

            response = _balancer.GetConnection().clusterClient.MemberRemove(request, headers);

            return response;
        }

        /// <summary>
        /// MemberRemoveAsync removes an existing member from the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request, Metadata headers = null)
        {
            MemberRemoveResponse response = new MemberRemoveResponse();

            response = await _balancer.GetConnection().clusterClient.MemberRemoveAsync(request, headers);

            return response;
        }

        /// <summary>
        /// MemberUpdate updates the member configuration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Metadata headers = null)
        {
            MemberUpdateResponse response = new MemberUpdateResponse();

            response = _balancer.GetConnection().clusterClient.MemberUpdate(request, headers);

            return response;
        }

        /// <summary>
        /// MemberUpdateAsync updates the member configuration in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request, Metadata headers = null)
        {
            MemberUpdateResponse response = new MemberUpdateResponse();

            response = await _balancer.GetConnection().clusterClient.MemberUpdateAsync(request, headers);

            return response;
        }

        /// <summary>
        /// MemberList lists all the members in the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberListResponse MemberList(MemberListRequest request, Metadata headers = null)
        {
            MemberListResponse response = new MemberListResponse();

            response = _balancer.GetConnection().clusterClient.MemberList(request, headers);

            return response;
        }

        /// <summary>
        /// MemberListAsync lists all the members in the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberListResponse> MemberListAsync(MemberListRequest request, Metadata headers = null)
        {
            MemberListResponse response = new MemberListResponse();

            response = await _balancer.GetConnection().clusterClient.MemberListAsync(request, headers);

            return response;
        }


    }
}
