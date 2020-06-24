namespace Ocelot.UnitTests.CacheManager
{
    using global::CacheManager.Core;
    using Moq;
    using Ocelot.Cache.CacheManager;
    using System;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class OcelotCacheManagerCache
    {
        private OcelotCacheManagerCache<string> _ocelotOcelotCacheManager;
        private Mock<ICacheManager<string>> _mockCacheManager;
        private string _key;
        private string _value;
        private string _resultGet;
        private TimeSpan _ttlSeconds;
        private string _region;

        public OcelotCacheManagerCache()
        {
            _mockCacheManager = new Mock<ICacheManager<string>>();
            _ocelotOcelotCacheManager = new OcelotCacheManagerCache<string>(_mockCacheManager.Object);
        }

        [Fact]
        public void should_get_from_cache()
        {
            this.Given(x => x.GivenTheFollowingIsCached("someKey", "someRegion", "someValue"))
                .When(x => x.WhenIGetFromTheCache())
                .Then(x => x.ThenTheResultIs("someValue"))
                .BDDfy();
        }

        [Fact]
        public void should_add_to_cache()
        {
            this.When(x => x.WhenIAddToTheCache("someKey", "someValue", TimeSpan.FromSeconds(1)))
                .Then(x => x.ThenTheCacheIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_delete_key_from_cache()
        {
            this.Given(_ => GivenTheFollowingRegion("fookey"))
                .When(_ => WhenIDeleteTheRegion("fookey"))
                .Then(_ => ThenTheRegionIsDeleted("fookey"))
                .BDDfy();
        }

        private void WhenIDeleteTheRegion(string region)
        {
            _ocelotOcelotCacheManager.ClearRegion(region);
        }

        private void ThenTheRegionIsDeleted(string region)
        {
            _mockCacheManager
                .Verify(x => x.ClearRegion(region), Times.Once);
        }

        private void GivenTheFollowingRegion(string key)
        {
            _ocelotOcelotCacheManager.Add(key, "doesnt matter", TimeSpan.FromSeconds(10), "region");
        }

        private void WhenIAddToTheCache(string key, string value, TimeSpan ttlSeconds)
        {
            _key = key;
            _value = value;
            _ttlSeconds = ttlSeconds;
            _ocelotOcelotCacheManager.Add(_key, _value, _ttlSeconds, "region");
        }

        private void ThenTheCacheIsCalledCorrectly()
        {
            _mockCacheManager
                .Verify(x => x.Add(It.IsAny<CacheItem<string>>()), Times.Once);
        }

        private void ThenTheResultIs(string expected)
        {
            _resultGet.ShouldBe(expected);
        }

        private void WhenIGetFromTheCache()
        {
            _resultGet = _ocelotOcelotCacheManager.Get(_key, _region);
        }

        private void GivenTheFollowingIsCached(string key, string region, string value)
        {
            _key = key;
            _value = value;
            _region = region;
            _mockCacheManager
                .Setup(x => x.Get<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(value);
        }
    }
}
