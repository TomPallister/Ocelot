namespace Ocelot.Provider.Rafty
{
    using global::Rafty.FiniteStateMachine;

    public class FakeCommand : ICommand
    {
        public FakeCommand(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }
    }
}
