using Xunit;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
/// Collection definition for authentication tests
/// Tests in this collection will be run sequentially, not in parallel
/// This prevents tests from interfering with each other's auth state
/// </summary>
[CollectionDefinition("AuthTests", DisableParallelization = true)]
public class AuthTestCollection
{
    // This class is never instantiated, it's just a marker for the collection
}
