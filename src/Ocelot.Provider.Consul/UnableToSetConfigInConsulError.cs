namespace Ocelot.Provider.Consul
{
    using Ocelot.Errors;

    public class UnableToSetConfigInConsulError : Error
    {
        public UnableToSetConfigInConsulError(string s)
            : base(s, OcelotErrorCode.UnknownError, 404)
        {
        }
    }
}
