namespace Ocelot.UnitTests.Rafty
{
    using Moq;
    using Ocelot.Configuration.Setter;
    using Provider.Rafty;
    using TestStack.BDDfy;
    using Xunit;

    public class OcelotFiniteStateMachineTests
    {
        private UpdateFileConfiguration _command;
        private readonly OcelotFiniteStateMachine _fsm;
        private readonly Mock<IFileConfigurationSetter> _setter;

        public OcelotFiniteStateMachineTests()
        {
            _setter = new Mock<IFileConfigurationSetter>();
            _fsm = new OcelotFiniteStateMachine(_setter.Object);
        }

        [Fact]
        public void should_handle_update_file_configuration_command()
        {
            this.Given(x => GivenACommand(new UpdateFileConfiguration(new Ocelot.Configuration.File.FileConfiguration())))
                .When(x => WhenTheCommandIsHandled())
                .Then(x => ThenTheStateIsUpdated())
                .BDDfy();
        }

        private void GivenACommand(UpdateFileConfiguration command)
        {
            _command = command;
        }

        private void WhenTheCommandIsHandled()
        {
            _fsm.Handle(new global::Rafty.Log.LogEntry(_command, _command.GetType(), 0)).Wait();
        }

        private void ThenTheStateIsUpdated()
        {
            _setter.Verify(x => x.Set(_command.Configuration), Times.Once);
        }
    }
}
