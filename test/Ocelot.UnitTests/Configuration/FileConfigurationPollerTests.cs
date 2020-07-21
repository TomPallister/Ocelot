using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Logging;
using Ocelot.Responses;
using Ocelot.UnitTests.Responder;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using TestStack.BDDfy;
using Xunit;
using static Ocelot.Infrastructure.Wait;

namespace Ocelot.UnitTests.Configuration
{
    public class FileConfigurationPollerTests : IDisposable
    {
        private readonly FileConfigurationPoller _poller;
        private Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IFileConfigurationRepository> _repo;
        private readonly FileConfiguration _fileConfig;
        private Mock<IFileConfigurationPollerOptions> _config;
        private readonly Mock<IInternalConfigurationRepository> _internalConfigRepo;
        private readonly Mock<IInternalConfigurationCreator> _internalConfigCreator;
        private IInternalConfiguration _internalConfig;

        public FileConfigurationPollerTests()
        {
            var logger = new Mock<IOcelotLogger>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _factory.Setup(x => x.CreateLogger<FileConfigurationPoller>()).Returns(logger.Object);
            _repo = new Mock<IFileConfigurationRepository>();
            _fileConfig = new FileConfiguration();
            _config = new Mock<IFileConfigurationPollerOptions>();
            _repo.Setup(x => x.Get()).ReturnsAsync(new OkResponse<FileConfiguration>(_fileConfig));
            _config.Setup(x => x.Delay).Returns(100);
            _internalConfigRepo = new Mock<IInternalConfigurationRepository>();
            _internalConfigCreator = new Mock<IInternalConfigurationCreator>();
            _internalConfigCreator.Setup(x => x.Create(It.IsAny<FileConfiguration>())).ReturnsAsync(new OkResponse<IInternalConfiguration>(_internalConfig));
            _poller = new FileConfigurationPoller(_factory.Object, _repo.Object, _config.Object, _internalConfigRepo.Object, _internalConfigCreator.Object);
        }

        [Fact]
        public void should_start()
        {
            this.Given(x => GivenPollerHasStarted())
                .Given(x => ThenTheSetterIsCalled(_fileConfig, 1))
                .BDDfy();
        }

        [Fact]
        public void should_call_setter_when_gets_new_config()
        {
            var newConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "test"
                            }
                        },
                    }
                }
            };

            this.Given(x => GivenPollerHasStarted())
                .Given(x => WhenTheConfigIsChanged(newConfig, 0))
                .Then(x => ThenTheSetterIsCalledAtLeast(newConfig, 1))
                .BDDfy();
        }

        [Fact]
        public void should_not_poll_if_already_polling()
        {
            var newConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "test"
                            }
                        },
                    }
                }
            };

            this.Given(x => GivenPollerHasStarted())
                .Given(x => WhenTheConfigIsChanged(newConfig, 10))
                .Then(x => ThenTheSetterIsCalled(newConfig, 1))
                .BDDfy();
        }

        [Fact]
        public void should_do_nothing_if_call_to_provider_fails()
        {
            var newConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "test"
                            }
                        },
                    }
                }
            };

            this.Given(x => GivenPollerHasStarted())
                .Given(x => WhenProviderErrors())
                .Then(x => ThenTheSetterIsCalled(newConfig, 0))
                .BDDfy();
        }

        [Fact]
        public void should_dispose_cleanly_without_starting()
        {
            this.When(x => WhenPollerIsDisposed())
                .BDDfy();
        }

        private void GivenPollerHasStarted()
        {
            _poller.StartAsync(CancellationToken.None);
        }

        private void WhenProviderErrors()
        {
            _repo
                .Setup(x => x.Get())
                .ReturnsAsync(new ErrorResponse<FileConfiguration>(new AnyError()));
        }

        private void WhenTheConfigIsChanged(FileConfiguration newConfig, int delay)
        {
            _repo
                .Setup(x => x.Get())
                .Callback(() => Thread.Sleep(delay))
                .ReturnsAsync(new OkResponse<FileConfiguration>(newConfig));
        }

        private void WhenPollerIsDisposed()
        {
            _poller.Dispose();
        }

        private void ThenTheSetterIsCalled(FileConfiguration fileConfig, int times)
        {
            var result = WaitFor(4000).Until(() =>
            {
                try
                {
                    _internalConfigRepo.Verify(x => x.AddOrReplace(_internalConfig), Times.Exactly(times));
                    _internalConfigCreator.Verify(x => x.Create(fileConfig), Times.Exactly(times));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            result.ShouldBeTrue();
        }

        private void ThenTheSetterIsCalledAtLeast(FileConfiguration fileConfig, int times)
        {
            var result = WaitFor(4000).Until(() =>
            {
                try
                {
                    _internalConfigRepo.Verify(x => x.AddOrReplace(_internalConfig), Times.AtLeast(times));
                    _internalConfigCreator.Verify(x => x.Create(fileConfig), Times.AtLeast(times));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            result.ShouldBeTrue();
        }

        public void Dispose()
        {
            _poller.Dispose();
        }
    }
}
