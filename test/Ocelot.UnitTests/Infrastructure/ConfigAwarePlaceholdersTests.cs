namespace Ocelot.UnitTests.Infrastructure
{
    using System;
    using Moq;
    using Ocelot.Infrastructure;
    using Responses;
    using Shouldly;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class ConfigAwarePlaceholdersTests
    {
        private readonly IPlaceholders _placeholders;
        private readonly Mock<IPlaceholders> _basePlaceholders;

        public ConfigAwarePlaceholdersTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();

            _basePlaceholders = new Mock<IPlaceholders>();
            _placeholders = new ConfigAwarePlaceholders(configuration, _basePlaceholders.Object);
        }

        [Fact]
        public void should_return_value_from_underlying_placeholders()
        {
            var baseUrl = "http://www.bbc.co.uk";
            const string key = "{BaseUrl}";
            
            _basePlaceholders.Setup(x => x.Get(key)).Returns(new OkResponse<string>(baseUrl));
            var result = _placeholders.Get(key);
            result.Data.ShouldBe(baseUrl);
        }

        [Fact]
        public void should_return_value_from_config_with_same_name_as_placeholder_if_underlying_placeholder_not_found()
        {
            const string expected = "http://foo-bar.co.uk";
            var baseUrl = "http://www.bbc.co.uk";
            const string key = "{BaseUrl}";
            
            _basePlaceholders.Setup(x => x.Get(key)).Returns(new ErrorResponse<string>(new FakeError()));
            var result = _placeholders.Get(key);
            result.Data.ShouldBe(expected);
        }

        [Theory]
        [InlineData("{TestConfig}")]
        [InlineData("{TestConfigNested:Child}")]
        public void should_return_value_from_config(string key)
        {
            const string expected = "foo";
            
            _basePlaceholders.Setup(x => x.Get(key)).Returns(new ErrorResponse<string>(new FakeError()));
            var result = _placeholders.Get(key);
            result.Data.ShouldBe(expected);
        }

        [Fact]
        public void should_call_underyling_when_added()
        {
            const string key = "{Test}";
            Func<Response<string>> func = () => new OkResponse<string>("test)");
            _placeholders.Add(key, func);
            _basePlaceholders.Verify(p => p.Add(key, func), Times.Once);
        }

        [Fact]
        public void should_call_underyling_when_removed()
        {
            const string key = "{Test}";
            _placeholders.Remove(key);
            _basePlaceholders.Verify(p => p.Remove(key), Times.Once);
        }
    }
}
