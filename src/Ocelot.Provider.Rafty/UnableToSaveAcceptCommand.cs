namespace Ocelot.Provider.Rafty
{
    using Errors;

    public class UnableToSaveAcceptCommand : Error
    {
        public UnableToSaveAcceptCommand(string message)
            : base(message, OcelotErrorCode.UnknownError, 404)
        {
        }
    }
}
