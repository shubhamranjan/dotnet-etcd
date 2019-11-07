using dotnet_etcd;
using Etcdserverpb;
using NUnit.Framework;

namespace Tests
{
    public class DiscoverySrvTests
    {
        private EtcdClient _client = null;

        [SetUp]
        public void Setup()
        {
            _client = new EtcdClient($"discovery-srv://{System.Environment.GetEnvironmentVariable("ETCD_DOMAIN")}/");
        }
        
        [Test]
        public void PutGetDelete()
        {
            _client.Put("foo/bar","barfoo");
            var getResult = _client.GetVal("foo/bar");
            Assert.AreEqual(getResult, "barfoo");
            _client.Delete("foo/bar");
        }
        
        [Test]
        public void DescribeCluster()
        {
            var request = new MemberListRequest();
            var result = _client.MemberList(request);
            Assert.NotZero(result.Members.Count);
        }
    }
}