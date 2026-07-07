using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using dotnet_etcd;
using Etcdserverpb;
using System.Collections.Generic;

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

        public WatchResilienceTests()
        {
            _client = new EtcdClient(EtcdUrl);
        }

        public void Dispose()
        {
            _client.Dispose();
            // Ensure container is unpaused in case test failed mid-way
            RunDockerCommand($"unpause {ContainerName}");
            // Ensure container is running (if restart failed or left it in weird state)
            // But 'unpause' handles pause. 'restart' handles restart.
            // If restart was pending, it should be fine.
        }

        [Fact]
        public async Task Watch_ShouldRecover_AfterNetworkPause()
        {
            var testKey = $"{_testKeyPrefix}pause";
            var events = new List<WatchEvent>();

            // 1. Start Watch
            _client.Watch(testKey, (response) =>
            {
                foreach (var evt in response.Events)
                {
                    events.Add(new WatchEvent
                    {
                        Type = evt.Type,
                        Key = evt.Kv.Key.ToStringUtf8(),
                        Value = evt.Kv.Value.ToStringUtf8()
                    });
                }
            });

            // 2. Initial Put
            await _client.PutAsync(testKey, "initial");
            await Task.Delay(1000); 
            Assert.NotEmpty(events);
            events.Clear();

            // 3. Simulate Network Disconnect (Pause Container)
            Console.WriteLine($"Pausing {ContainerName}...");
            RunDockerCommand($"pause {ContainerName}");

            // 4. Wait for > 10s (simulating standard timeout/keepalive expiry)
            Console.WriteLine("Waiting 12s...");
            await Task.Delay(12000);

            // 5. Resume Network
            Console.WriteLine($"Unpausing {ContainerName}...");
            RunDockerCommand($"unpause {ContainerName}");
            
            // Allow client to attempt reconnection
            await Task.Delay(3000);

            // 6. Put post-disconnect
            Console.WriteLine("Putting value after reconnect...");
            await _client.PutAsync(testKey, "recovered-pause");

            // 7. Verification
            await Task.Delay(2000);
            
            Assert.Single(events);
            Assert.Equal("recovered-pause", events[0].Value);
        }

        [Fact]
        public async Task Watch_ShouldRecover_AfterServerRestart()
        {
            var testKey = $"{_testKeyPrefix}restart";
            var events = new List<WatchEvent>();

            // 1. Start Watch
            _client.Watch(testKey, (response) =>
            {
                foreach (var evt in response.Events)
                {
                    events.Add(new WatchEvent
                    {
                        Type = evt.Type,
                        Key = evt.Kv.Key.ToStringUtf8(),
                        Value = evt.Kv.Value.ToStringUtf8()
                    });
                }
            });

            // 2. Initial Put
            await _client.PutAsync(testKey, "initial");
            await Task.Delay(1000); 
            Assert.NotEmpty(events);
            events.Clear();

            // 3. Simulate Server Restart (Forces Connection Drop)
            Console.WriteLine($"Restarting {ContainerName}...");
            RunDockerCommand($"restart {ContainerName}");

            // 4. Poll until etcd is serving again instead of a fixed sleep (restart time varies,
            //    especially on loaded CI runners).
            Console.WriteLine("Waiting for etcd to become ready...");
            var readySw = Stopwatch.StartNew();
            while (readySw.Elapsed.TotalSeconds < 60)
            {
                try
                {
                    await _client.GetAsync("watch-resilience-health-probe");
                    break; // etcd responded
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }

            // 5. Put post-reconnect, retrying until an event is received or timeout.
            //    The watch resumes from the last observed revision + 1, so even if a put races ahead
            //    of the watch re-registration the event is replayed rather than missed. The retry
            //    loop also absorbs the brief window where etcd is up but the stream is mid-reconnect.
            Console.WriteLine("Putting value after reconnect (retrying until event received)...");
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < 30 && events.Count == 0)
            {
                try
                {
                    await _client.PutAsync(testKey, "recovered-restart");
                }
                catch
                {
                    // etcd may still be starting up; keep retrying
                }
                await Task.Delay(2000);
            }

            // 7. Verification
            Assert.NotEmpty(events);
            Assert.Equal("recovered-restart", events[0].Value);
        }

        private void RunDockerCommand(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                 Console.WriteLine("Failed to start docker process");
                 return;
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                // Don't throw for unpause in Dispose if already unpaused
                if (!args.Contains("unpause") || !error.Contains("is not paused"))
                {
                    Console.WriteLine($"Docker command failed: {error}");
                }
            }
        }
    }
}
