namespace Ocelot.Configuration.Validator
{
    using Ocelot.Configuration.File;
    using Ocelot.Responses;
    using System.Threading.Tasks;

    public interface IConfigurationValidator
    {
        Task<Response<ConfigurationValidationResult>> IsValid(FileConfiguration configuration);
    }
}
