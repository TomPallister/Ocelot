namespace Ocelot.UnitTests.Cache
{
    using Ocelot.Cache;
    using Shouldly;
    using System;
    using System.Threading;
    using Xunit;
    using Microsoft.Extensions.Caching.Memory;

    public class AspMemoryCacheTests
    {
        private readonly AspMemoryCache<Fake> _cache;

        public AspMemoryCacheTests()
        {
            _cache = new AspMemoryCache<Fake>(new MemoryCache(new MemoryCacheOptions()));
        }

        [Fact]
        public void should_cache()
        {
            var fake = new Fake(1);
            _cache.Add("1", fake, TimeSpan.FromSeconds(100), "region");
            var result = _cache.Get("1", "region");
            result.ShouldBe(fake);
            fake.Value.ShouldBe(1);
        }

        [Fact]
        public void doesnt_exist()
        {
            var result = _cache.Get("1", "region");
            result.ShouldBeNull();
        }

        [Fact]
        public void should_add_and_delete()
        {
            var fake = new Fake(1);
            _cache.Add("1", fake, TimeSpan.FromSeconds(100), "region");
            var newFake = new Fake(1);
            _cache.AddAndDelete("1", newFake, TimeSpan.FromSeconds(100), "region");
            var result = _cache.Get("1", "region");
            result.ShouldBe(newFake);
            newFake.Value.ShouldBe(1);
        }

        [Fact]
        public void should_clear_region()
        {
            var fake1 = new Fake(1);
            var fake2 = new Fake(2);
            _cache.Add("1", fake1, TimeSpan.FromSeconds(100), "region");
            _cache.Add("2", fake2, TimeSpan.FromSeconds(100), "region");
            _cache.ClearRegion("region");
            var result1 = _cache.Get("1", "region");
            result1.ShouldBeNull();
            var result2 = _cache.Get("2", "region");
            result2.ShouldBeNull();
        }

        [Fact]
        public void should_clear_key_if_ttl_expired()
        {
            var fake = new Fake(1);
            _cache.Add("1", fake, TimeSpan.FromMilliseconds(50), "region");
            Thread.Sleep(200);
            var result = _cache.Get("1", "region");
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void should_not_add_to_cache_if_timespan_empty(int ttl)
        {
            var fake = new Fake(1);
            _cache.Add("1", fake, TimeSpan.FromSeconds(ttl), "region");
            var result = _cache.Get("1", "region");
            result.ShouldBeNull();
        }

        private class Fake
        {
            public Fake(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
