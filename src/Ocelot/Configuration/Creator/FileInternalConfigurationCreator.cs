namespace Ocelot.Configuration.Creator
{
    using File;
    using Responses;
    using System.Linq;
    using System.Threading.Tasks;
    using Validator;

    public class FileInternalConfigurationCreator : IInternalConfigurationCreator
    {
        private readonly IConfigurationValidator _configurationValidator;
        private readonly IConfigurationCreator _configCreator;
        private readonly IDynamicsCreator _dynamicsCreator;
        private readonly IRoutesCreator _routesCreator;
        private readonly IAggregatesCreator _aggregatesCreator;

        public FileInternalConfigurationCreator(
            IConfigurationValidator configurationValidator,
            IRoutesCreator routesCreator,
            IAggregatesCreator aggregatesCreator,
            IDynamicsCreator dynamicsCreator,
            IConfigurationCreator configCreator
            )
        {
            _configCreator = configCreator;
            _dynamicsCreator = dynamicsCreator;
            _aggregatesCreator = aggregatesCreator;
            _routesCreator = routesCreator;
            _configurationValidator = configurationValidator;
        }

        public async Task<Response<IInternalConfiguration>> Create(FileConfiguration fileConfiguration)
        {
            var response = await _configurationValidator.IsValid(fileConfiguration);

            if (response.Data.IsError)
            {
                return new ErrorResponse<IInternalConfiguration>(response.Data.Errors);
            }

            var routes = _routesCreator.Create(fileConfiguration);

            var aggregates = _aggregatesCreator.Create(fileConfiguration, routes);

            var dynamicRoute = _dynamicsCreator.Create(fileConfiguration);

            var mergedRoutes = routes
                .Union(aggregates)
                .Union(dynamicRoute)
                .ToList();

            var config = _configCreator.Create(fileConfiguration, mergedRoutes);

            return new OkResponse<IInternalConfiguration>(config);
        }
    }
}
