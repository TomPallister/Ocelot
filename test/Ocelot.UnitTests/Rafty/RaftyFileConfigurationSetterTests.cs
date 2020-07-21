namespace Ocelot.UnitTests.Rafty
{
    using global::Rafty.Concensus.Node;
    using global::Rafty.Infrastructure;
    using Moq;
    using Ocelot.Configuration.File;
    using Provider.Rafty;
    using Shouldly;
    using System.Threading.Tasks;
    using Xunit;

    public class RaftyFileConfigurationSetterTests
    {
        private readonly RaftyFileConfigurationSetter _setter;
        private readonly Mock<INode> _node;

        public RaftyFileConfigurationSetterTests()
        {
            _node = new Mock<INode>();
            _setter = new RaftyFileConfigurationSetter(_node.Object);
        }

        [Fact]
        public async Task should_return_ok()
        {
            var fileConfig = new FileConfiguration();

            var response = new OkResponse<UpdateFileConfiguration>(new UpdateFileConfiguration(fileConfig));

            _node.Setup(x => x.Accept(It.IsAny<UpdateFileConfiguration>()))
                .ReturnsAsync(response);

            var result = await _setter.Set(fileConfig);
            result.IsError.ShouldBeFalse();
        }

        [Fact]
        public async Task should_return_not_ok()
        {
            var fileConfig = new FileConfiguration();

            var response = new ErrorResponse<UpdateFileConfiguration>("error", new UpdateFileConfiguration(fileConfig));

            _node.Setup(x => x.Accept(It.IsAny<UpdateFileConfiguration>()))
                .ReturnsAsync(response);

            var result = await _setter.Set(fileConfig);

            result.IsError.ShouldBeTrue();
        }
    }
}
