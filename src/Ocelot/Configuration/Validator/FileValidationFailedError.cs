namespace Ocelot.Configuration.Validator
{
    using Errors;

    public class FileValidationFailedError : Error
    {
        public FileValidationFailedError(string message)
            : base(message, OcelotErrorCode.FileValidationFailedError, 404)
        {
        }
    }
}
