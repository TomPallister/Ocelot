using Microsoft.AspNetCore.Mvc;
using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Setter;
using Ocelot.Errors;
using Ocelot.Responses;
using Shouldly;
using System;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Controllers
{
    using Ocelot.Configuration.Repository;

    public class FileConfigurationControllerTests
    {
        private readonly FileConfigurationController _controller;
        private readonly Mock<IFileConfigurationRepository> _repo;
        private readonly Mock<IFileConfigurationSetter> _setter;
        private IActionResult _result;
        private FileConfiguration _fileConfiguration;
        private readonly Mock<IServiceProvider> _provider;

        public FileConfigurationControllerTests()
        {
            _provider = new Mock<IServiceProvider>();
            _repo = new Mock<IFileConfigurationRepository>();
            _setter = new Mock<IFileConfigurationSetter>();
            _controller = new FileConfigurationController(_repo.Object, _setter.Object, _provider.Object);
        }

        [Fact]
        public void should_get_file_configuration()
        {
            var expected = new Responses.OkResponse<FileConfiguration>(new FileConfiguration());

            this.Given(x => x.GivenTheGetConfigurationReturns(expected))
                .When(x => x.WhenIGetTheFileConfiguration())
                .Then(x => x.TheTheGetFileConfigurationIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_when_cannot_get_config()
        {
            var expected = new Responses.ErrorResponse<FileConfiguration>(It.IsAny<Error>());

            this.Given(x => x.GivenTheGetConfigurationReturns(expected))
               .When(x => x.WhenIGetTheFileConfiguration())
               .Then(x => x.TheTheGetFileConfigurationIsCalledCorrectly())
               .And(x => x.ThenTheResponseIs<BadRequestObjectResult>())
               .BDDfy();
        }

        [Fact]
        public void should_post_file_configuration()
        {
            var expected = new FileConfiguration();

            this.Given(x => GivenTheFileConfiguration(expected))
                .And(x => GivenTheConfigSetterReturns(new OkResponse()))
                .When(x => WhenIPostTheFileConfiguration())
                .Then(x => x.ThenTheConfigrationSetterIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_when_cannot_set_config()
        {
            var expected = new FileConfiguration();

            this.Given(x => GivenTheFileConfiguration(expected))
                .And(x => GivenTheConfigSetterReturns(new ErrorResponse(new FakeError())))
                .When(x => WhenIPostTheFileConfiguration())
                .Then(x => x.ThenTheConfigrationSetterIsCalledCorrectly())
                .And(x => ThenTheResponseIs<BadRequestObjectResult>())
                .BDDfy();
        }

        private void GivenTheConfigSetterReturns(Response response)
        {
            _setter
                .Setup(x => x.Set(It.IsAny<FileConfiguration>()))
                .ReturnsAsync(response);
        }

        private void ThenTheConfigrationSetterIsCalledCorrectly()
        {
            _setter
                .Verify(x => x.Set(_fileConfiguration), Times.Once);
        }

        private void WhenIPostTheFileConfiguration()
        {
            _result = _controller.Post(_fileConfiguration).Result;
        }

        private void GivenTheFileConfiguration(FileConfiguration fileConfiguration)
        {
            _fileConfiguration = fileConfiguration;
        }

        private void ThenTheResponseIs<T>()
        {
            _result.ShouldBeOfType<T>();
        }

        private void GivenTheGetConfigurationReturns(Ocelot.Responses.Response<FileConfiguration> fileConfiguration)
        {
            _repo
                .Setup(x => x.Get())
                .ReturnsAsync(fileConfiguration);
        }

        private void WhenIGetTheFileConfiguration()
        {
            _result = _controller.Get().Result;
        }

        private void TheTheGetFileConfigurationIsCalledCorrectly()
        {
            _repo
             .Verify(x => x.Get(), Times.Once);
        }

        private class FakeError : Error
        {
            public FakeError() : base(string.Empty, OcelotErrorCode.CannotAddDataError, 404)
            {
            }
        }
    }
}
