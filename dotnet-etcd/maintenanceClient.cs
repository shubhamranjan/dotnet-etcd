using Etcdserverpb;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        ///  Alarm activates, deactivates, and queries alarms regarding cluster health
        /// </summary>
        /// <param name="request">Alarm request</param>
        /// <returns>Alarm Response</returns>
        public AlarmResponse Alarm(AlarmRequest request)
        {
            AlarmResponse response = new AlarmResponse();
            try
            {
                response = _maintenanceClient.Alarm(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        ///  Alarm activates, deactivates, and queries alarms regarding cluster health in async
        /// </summary>
        /// <param name="request">Alarm request</param>
        /// <returns>Alarm Response</returns>
        public async Task<AlarmResponse> AlarmAsync(AlarmRequest request)
        {
            AlarmResponse response = new AlarmResponse();
            try
            {
                response = await _maintenanceClient.AlarmAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Status gets the status of the member.
        /// </summary>
        /// <param name="request">Status Request</param>
        /// <returns>Status response</returns>
        public StatusResponse Status(StatusRequest request)
        {
            StatusResponse response = new StatusResponse();
            try
            {
                response = _maintenanceClient.Status(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Status gets the status of the member in async.
        /// </summary>
        /// <param name="request">Status Request</param>
        /// <returns>Status response</returns>
        public async Task<StatusResponse> StatusASync(StatusRequest request)
        {
            StatusResponse response = new StatusResponse();
            try
            {
                response = await _maintenanceClient.StatusAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Defragment defragments a member's backend database to recover storage space.
        /// </summary>
        /// <param name="request">Defragment Request</param>
        /// <returns>Defragment Response</returns>
        public DefragmentResponse Defragment(DefragmentRequest request)
        {
            DefragmentResponse response = new DefragmentResponse();
            try
            {
                response = _maintenanceClient.Defragment(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Defragment defragments a member's backend database to recover storage space in async.
        /// </summary>
        /// <param name="request">Defragment Request</param>
        /// <returns>Defragment Response</returns>
        public async Task<DefragmentResponse> DefragmentAsync(DefragmentRequest request)
        {
            DefragmentResponse response = new DefragmentResponse();
            try
            {
                response = await _maintenanceClient.DefragmentAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Hash computes the hash of whole backend keyspace,
        /// including key, lease, and other buckets in storage.
        /// This is designed for testing ONLY!
        /// Do not rely on this in production with ongoing transactions,
        /// since Hash operation does not hold MVCC locks.
        /// Use "HashKV" API instead for "key" bucket consistency checks.
        /// </summary>
        /// <param name="request">Hash Request</param>
        /// <returns>Hash Response</returns>
        public HashResponse Hash(HashRequest request)
        {
            HashResponse response = new HashResponse();
            try
            {
                response = _maintenanceClient.Hash(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Hash computes the hash of whole backend keyspace,
        /// including key, lease, and other buckets in storage in async.
        /// This is designed for testing ONLY!
        /// Do not rely on this in production with ongoing transactions,
        /// since Hash operation does not hold MVCC locks.
        /// Use "HashKV" API instead for "key" bucket consistency checks.
        /// </summary>
        /// <param name="request">Hash Request</param>
        /// <returns>Hash Response</returns>
        public async Task<HashResponse> HashAsync(HashRequest request)
        {
            HashResponse response = new HashResponse();
            try
            {
                response = await _maintenanceClient.HashAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// HashKV computes the hash of all MVCC keys up to a given revision.
        /// It only iterates "key" bucket in backend storage.
        /// </summary>
        /// <param name="request">HashKV Request</param>
        /// <returns>HashKV Response</returns>
        public HashKVResponse HashKV(HashKVRequest request)
        {
            HashKVResponse response = new HashKVResponse();
            try
            {
                response = _maintenanceClient.HashKV(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// HashKV computes the hash of all MVCC keys up to a given revision in async.
        /// It only iterates "key" bucket in backend storage.
        /// </summary>
        /// <param name="request">HashKV Request</param>
        /// <returns>HashKV Response</returns>
        public async Task<HashKVResponse> HashKVAsync(HashKVRequest request)
        {
            HashKVResponse response = new HashKVResponse();
            try
            {
                response = await _maintenanceClient.HashKVAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async void Snapshot(SnapshotRequest request, Action<SnapshotResponse> method, CancellationToken token)
        {
            try
            {
                using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = _maintenanceClient.Snapshot(request))
                {
                    Task snapshotTask = Task.Run(async () =>
                    {
                        while (await snapshotter.ResponseStream.MoveNext(token))
                        {
                            SnapshotResponse update = snapshotter.ResponseStream.Current;
                            method(update);
                        }
                    });

                    await snapshotTask;
                }
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async void Snapshot(SnapshotRequest request, Action<SnapshotResponse>[] methods, CancellationToken token)
        {
            try
            {
                using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = _maintenanceClient.Snapshot(request))
                {
                    Task snapshotTask = Task.Run(async () =>
                    {
                        while (await snapshotter.ResponseStream.MoveNext(token))
                        {
                            SnapshotResponse update = snapshotter.ResponseStream.Current;
                            foreach (Action<SnapshotResponse> method in methods)
                            {
                                method(update);
                            }
                        }
                    });

                    await snapshotTask;
                }
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// MoveLeader requests current leader node to transfer its leadership to transferee.
        /// </summary>
        /// <param name="request">MoveLeader Request</param>
        /// <returns>MoveLeader Response</returns>
        public MoveLeaderResponse MoveLeader(MoveLeaderRequest request)
        {
            MoveLeaderResponse response = new MoveLeaderResponse();
            try
            {
                response = _maintenanceClient.MoveLeader(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
            return response;
        }

        /// <summary>
        /// MoveLeader requests current leader node to transfer its leadership to transferee in async.
        /// </summary>
        /// <param name="request">MoveLeader Request</param>
        /// <returns>MoveLeader Response</returns>
        public async Task<MoveLeaderResponse> MoveLeaderAsync(MoveLeaderRequest request)
        {
            MoveLeaderResponse response = new MoveLeaderResponse();
            try
            {
                response = await _maintenanceClient.MoveLeaderAsync(request);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
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
