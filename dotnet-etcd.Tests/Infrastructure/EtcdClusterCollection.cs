namespace dotnet_etcd.Tests.Infrastructure;

[CollectionDefinition("EtcdCluster")]
public class EtcdClusterCollection : ICollectionFixture<EtcdClusterFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}