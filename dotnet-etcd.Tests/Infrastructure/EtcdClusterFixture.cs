namespace dotnet_etcd.Tests.Infrastructure;

public class EtcdClusterFixture : IAsyncLifetime
{
    public EtcdClusterFixture()
    {
        // Read the cluster type from the file
        var clusterTypeFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "cluster-type.txt");

        if (File.Exists(clusterTypeFilePath))
            ClusterType = File.ReadAllText(clusterTypeFilePath).Trim();
        else
            // Default to single node if the file doesn't exist
            ClusterType = "single";

        // Set the connection string based on the cluster type
        if (ClusterType == "3nodes")
            // For 3-node cluster, use all three endpoints
            ConnectionString = "http://localhost:2379,http://localhost:22379,http://localhost:32379";
        else
            // For single-node cluster, use the default endpoint
            ConnectionString = "http://localhost:2379";
    }

    // Connection string for the etcd cluster
    public string ConnectionString { get; private set; }

    // Cluster type (single or 3nodes)
    public string ClusterType { get; }

    public Task InitializeAsync()
    {
        // No initialization needed as etcd is managed externally
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // No cleanup needed as etcd is managed externally
        return Task.CompletedTask;
    }
}