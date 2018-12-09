using Etcdserverpb;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        ///  MemberAdd adds a member into the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberAddResponse MemberAdd(MemberAddRequest request)
        {
            MemberAddResponse response = new MemberAddResponse();
            try
            {
                response = _clusterClient.MemberAdd(request);
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
            return response;
        }

        /// <summary>
        ///  MemberAddAsync adds a member into the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request)
        {
            MemberAddResponse response = new MemberAddResponse();
            try
            {
                response = await _clusterClient.MemberAddAsync(request);
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
            return response;
        }

        /// <summary>
        /// MemberRemove removes an existing member from the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberRemoveResponse MemberRemove(MemberRemoveRequest request)
        {
            MemberRemoveResponse response = new MemberRemoveResponse();
            try
            {
                response =  _clusterClient.MemberRemove(request);
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
            return response;
        }

        /// <summary>
        /// MemberRemoveAsync removes an existing member from the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request)
        {
            MemberRemoveResponse response = new MemberRemoveResponse();
            try
            {
                response = await _clusterClient.MemberRemoveAsync(request);
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
            return response;
        }

        /// <summary>
        /// MemberUpdate updates the member configuration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberUpdateResponse MemberUpdate(MemberUpdateRequest request)
        {
            MemberUpdateResponse response = new MemberUpdateResponse();
            try
            {
                response =  _clusterClient.MemberUpdate(request);
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
            return response;
        }

        /// <summary>
        /// MemberUpdateAsync updates the member configuration in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request)
        {
            MemberUpdateResponse response = new MemberUpdateResponse();
            try
            {
                response = await _clusterClient.MemberUpdateAsync(request);
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
            return response;
        }

        /// <summary>
        /// MemberList lists all the members in the cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MemberListResponse MemberList(MemberListRequest request)
        {
            MemberListResponse response = new MemberListResponse();
            try
            {
                response =  _clusterClient.MemberList(request);
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
            return response;
        }

        /// <summary>
        /// MemberListAsync lists all the members in the cluster in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MemberListResponse> MemberListAsync(MemberListRequest request)
        {
            MemberListResponse response = new MemberListResponse();
            try
            {
                response = await _clusterClient.MemberListAsync(request);
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
            return response;
        }


    }
}
