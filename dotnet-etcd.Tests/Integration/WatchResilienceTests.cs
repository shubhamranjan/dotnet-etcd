using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using dotnet_etcd;

namespace dotnet_etcd.Tests.Integration
{
    // Use the isolated collection for resilience tests to prevent parallel execution interference
    [Collection("WatchResilienceTests")]
    [Trait("Category", "Integration")]
    public class WatchResilienceTests : IDisposable
    {
        private readonly EtcdClient _client;
        private readonly string _testKeyPrefix = "watch-resilience-";

        // Dedicated single-node etcd (port 2409) so pausing/restarting the server here never
        // disrupts the shared etcd1 cluster used by the other integration tests.
        private const string EtcdUrl = "http://localhost:2409";
        private const string ContainerName = "etcd-resilience";

        // Watch delivery is asynchronous and its latency is unbounded in practice: on a loaded CI
        // runner the receive loop can be starved for far longer than any sleep we would care to
        // hardcode. Every wait below therefore polls up to this budget and returns as soon as the
        // condition holds, so the test is fast when things are fast and only slow when it must be.
        private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(20);

        // Diagnostics captured from the watch stream. If this test ever fails again, the assertion
        // message carries the Created ack and the revisions, which is what distinguishes "the event
        // was never delivered" from "the watch was never registered".
        private readonly List<string> _trace = [];
        private readonly Stopwatch _sw = Stopwatch.StartNew();

        public WatchResilienceTests() => _client = new EtcdClient(EtcdUrl);

        public void Dispose()
        {
            _client.Dispose();
            // Ensure container is unpaused in case the test failed mid-way.
            RunDockerCommand($"unpause {ContainerName}");
        }

        [Fact]
        public async Task Watch_ShouldRecover_AfterNetworkPause()
        {
            string testKey = $"{_testKeyPrefix}pause";
            ConcurrentQueue<WatchEvent> events = StartWatch(testKey);

            // 1. Establish the watch by observing the first event.
            long putRev = (await _client.PutAsync(testKey, "initial")).Header.Revision;
            Trace($"PUT initial rev={putRev}");
            await WaitForEvents(events, 1, "initial watch establishment");
            events.Clear();

            // 2. Simulate a network partition by pausing the container.
            Trace($"pausing {ContainerName}");
            RunDockerCommand($"pause {ContainerName}");

            // Stay paused past the keepalive timeout so the client really sees the connection die.
            await Task.Delay(12000);

            Trace($"unpausing {ContainerName}");
            RunDockerCommand($"unpause {ContainerName}");

            // 3. Write through the recovered connection. The put is retried because the client may
            //    still be re-establishing the stream, and the watch resumes from the revision after
            //    the last one it saw, so the event is delivered even if it lands mid-reconnect.
            await PutUntilSucceeds(testKey, "recovered-pause");

            // 4. The watch must deliver the post-partition event.
            await WaitForEvents(events, 1, "event after network pause");
            Assert.Equal("recovered-pause", events.First().Value);
        }

        [Fact]
        public async Task Watch_ShouldRecover_AfterServerRestart()
        {
            string testKey = $"{_testKeyPrefix}restart";
            ConcurrentQueue<WatchEvent> events = StartWatch(testKey);

            // 1. Establish the watch by observing the first event.
            long putRev = (await _client.PutAsync(testKey, "initial")).Header.Revision;
            Trace($"PUT initial rev={putRev}");
            await WaitForEvents(events, 1, "initial watch establishment");
            events.Clear();

            // 2. Restart the server, which forces the stream to drop.
            Trace($"restarting {ContainerName}");
            RunDockerCommand($"restart {ContainerName}");

            // 3. Wait for etcd to serve again (restart time varies wildly on loaded CI runners).
            await WaitUntil(async () =>
            {
                try
                {
                    await _client.GetAsync("watch-resilience-health-probe");
                    return true;
                }
                catch
                {
                    return false;
                }
            }, TimeSpan.FromSeconds(60), "etcd to become ready after restart");

            // 4. Write through the recovered connection and require the watch to deliver it.
            await PutUntilSucceeds(testKey, "recovered-restart");
            await WaitForEvents(events, 1, "event after server restart");
            Assert.Equal("recovered-restart", events.First().Value);
        }

        /// <summary>
        ///     Registers the watch and returns the (thread-safe) collection its events land in. The
        ///     callback runs on the gRPC receive loop, so the collection must not be a plain List that
        ///     the test thread reads concurrently.
        /// </summary>
        private ConcurrentQueue<WatchEvent> StartWatch(string testKey)
        {
            ConcurrentQueue<WatchEvent> events = new();

            _client.Watch(testKey, response =>
            {
                if (response.Created)
                {
                    Trace($"CREATED watchId={response.WatchId} rev={response.Header?.Revision}");
                }

                if (response.Events == null)
                {
                    return;
                }

                foreach (Mvccpb.Event evt in response.Events)
                {
                    Trace($"EVENT {evt.Type} rev={evt.Kv.ModRevision} value={evt.Kv.Value.ToStringUtf8()}");
                    events.Enqueue(new WatchEvent
                    {
                        Type = evt.Type,
                        Key = evt.Kv.Key.ToStringUtf8(),
                        Value = evt.Kv.Value.ToStringUtf8()
                    });
                }
            });

            Trace("Watch() returned");
            return events;
        }

        /// <summary>
        ///     Puts until the write is accepted — etcd may still be coming back up — so that a
        ///     transient write failure is not misreported as the watch losing the event.
        /// </summary>
        private async Task PutUntilSucceeds(string key, string value)
        {
            Exception? last = null;
            await WaitUntil(async () =>
            {
                try
                {
                    long rev = (await _client.PutAsync(key, value)).Header.Revision;
                    Trace($"PUT {value} rev={rev}");
                    return true;
                }
                catch (Exception ex)
                {
                    last = ex;
                    return false;
                }
            }, TimeSpan.FromSeconds(30), $"put '{value}' to succeed (last error: {last?.Message})");
        }

        private async Task WaitForEvents(ConcurrentQueue<WatchEvent> events, int expected, string what) =>
            await WaitUntil(() => Task.FromResult(events.Count >= expected), EventTimeout,
                $"{what}: expected >= {expected} watch event(s), got {events.Count}");

        /// <summary>
        ///     Polls until the condition holds, failing with the captured watch-stream trace so a
        ///     timeout says why it timed out rather than just "collection was empty".
        /// </summary>
        private async Task WaitUntil(Func<Task<bool>> condition, TimeSpan timeout, string what)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                if (await condition())
                {
                    return;
                }

                await Task.Delay(200);
            }

            if (await condition())
            {
                return;
            }

            Assert.Fail($"Timed out after {timeout.TotalSeconds:0}s waiting for {what}.\nWatch stream trace:\n  " +
                        string.Join("\n  ", GetTrace()));
        }

        private void Trace(string message)
        {
            lock (_trace)
            {
                _trace.Add($"[{_sw.ElapsedMilliseconds,6}ms] {message}");
            }
        }

        private string[] GetTrace()
        {
            lock (_trace)
            {
                return _trace.Count == 0 ? ["(no watch responses received at all)"] : [.. _trace];
            }
        }

        private void RunDockerCommand(string args)
        {
            ProcessStartInfo psi = new()
            {
                FileName = "docker",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine("Failed to start docker process");
                return;
            }

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                // Don't complain when Dispose unpauses a container that was never paused.
                if (!args.Contains("unpause") || !error.Contains("is not paused"))
                {
                    Console.WriteLine($"Docker command failed: {error}");
                }
            }
        }
    }
}
