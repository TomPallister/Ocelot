namespace Ocelot.Configuration.Validator
{
    using Errors;
    using File;
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;
    using Responses;
    using ServiceDiscovery;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class FileConfigurationFluentValidator : AbstractValidator<FileConfiguration>, IConfigurationValidator
    {
        private readonly List<ServiceDiscoveryFinderDelegate> _serviceDiscoveryFinderDelegates;

        public FileConfigurationFluentValidator(IServiceProvider provider, RouteFluentValidator routeFluentValidator, FileGlobalConfigurationFluentValidator fileGlobalConfigurationFluentValidator)
        {
            _serviceDiscoveryFinderDelegates = provider
                .GetServices<ServiceDiscoveryFinderDelegate>()
                .ToList();

            RuleForEach(configuration => configuration.Routes)
                .SetValidator(routeFluentValidator);

            RuleFor(configuration => configuration.GlobalConfiguration)
                .SetValidator(fileGlobalConfigurationFluentValidator);

            RuleForEach(configuration => configuration.Routes)
                .Must((config, route) => IsNotDuplicateIn(route, config.Routes))
                .WithMessage((config, route) => $"{nameof(route)} {route.UpstreamPathTemplate} has duplicate");

            RuleForEach(configuration => configuration.Routes)
                .Must((config, route) => HaveServiceDiscoveryProviderRegistered(route, config.GlobalConfiguration.ServiceDiscoveryProvider))
                .WithMessage((config, route) => $"Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using ServiceDiscoveryOptions but no ServiceDiscoveryFinderDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Consul and services.AddConsul() or Ocelot.Provider.Eureka and services.AddEureka()?");

            RuleForEach(configuration => configuration.Routes)
                .Must((config, route) => IsPlaceholderNotDuplicatedIn(route.UpstreamPathTemplate))
                .WithMessage((config, route) => $"{nameof(route)} {route.UpstreamPathTemplate} has duplicated placeholder");

            RuleFor(configuration => configuration.GlobalConfiguration.ServiceDiscoveryProvider)
                .Must(HaveServiceDiscoveryProviderRegistered)
                .WithMessage((config, route) => $"Unable to start Ocelot, errors are: Unable to start Ocelot because either a Route or GlobalConfiguration are using ServiceDiscoveryOptions but no ServiceDiscoveryFinderDelegate has been registered in dependency injection container. Are you missing a package like Ocelot.Provider.Consul and services.AddConsul() or Ocelot.Provider.Eureka and services.AddEureka()?");

            RuleForEach(configuration => configuration.Routes)
                .Must((config, route) => IsNotDuplicateIn(route, config.Aggregates))
                .WithMessage((config, route) => $"{nameof(route)} {route.UpstreamPathTemplate} has duplicate aggregate");

            RuleForEach(configuration => configuration.Aggregates)
                .Must((config, aggregateRoute) => IsNotDuplicateIn(aggregateRoute, config.Aggregates))
                .WithMessage((config, aggregate) => $"{nameof(aggregate)} {aggregate.UpstreamPathTemplate} has duplicate aggregate");

            RuleForEach(configuration => configuration.Aggregates)
                .Must((config, aggregateRoute) => AllRoutesForAggregateExist(aggregateRoute, config.Routes))
                .WithMessage((config, aggregateRoute) => $"Routes for {nameof(aggregateRoute)} {aggregateRoute.UpstreamPathTemplate} either do not exist or do not have correct ServiceName property");

            RuleForEach(configuration => configuration.Aggregates)
                .Must((config, aggregateRoute) => DoesNotContainRoutesWithSpecificRequestIdKeys(aggregateRoute, config.Routes))
                .WithMessage((config, aggregateRoute) => $"{nameof(aggregateRoute)} {aggregateRoute.UpstreamPathTemplate} contains Route with specific RequestIdKey, this is not possible with Aggregates");
        }

        private bool HaveServiceDiscoveryProviderRegistered(FileRoute route, FileServiceDiscoveryProvider serviceDiscoveryProvider)
        {
            if (string.IsNullOrEmpty(route.ServiceName))
            {
                return true;
            }

            if (serviceDiscoveryProvider?.Type?.ToLower() == "servicefabric")
            {
                return true;
            }

            return _serviceDiscoveryFinderDelegates.Any();
        }

        private bool HaveServiceDiscoveryProviderRegistered(FileServiceDiscoveryProvider serviceDiscoveryProvider)
        {
            if (serviceDiscoveryProvider == null)
            {
                return true;
            }

            if (serviceDiscoveryProvider?.Type?.ToLower() == "servicefabric")
            {
                return true;
            }

            return string.IsNullOrEmpty(serviceDiscoveryProvider.Type) || _serviceDiscoveryFinderDelegates.Any();
        }

        public async Task<Response<ConfigurationValidationResult>> IsValid(FileConfiguration configuration)
        {
            var validateResult = await ValidateAsync(configuration);

            if (validateResult.IsValid)
            {
                return new OkResponse<ConfigurationValidationResult>(new ConfigurationValidationResult(false));
            }

            var errors = validateResult.Errors.Select(failure => new FileValidationFailedError(failure.ErrorMessage));

            var result = new ConfigurationValidationResult(true, errors.Cast<Error>().ToList());

            return new OkResponse<ConfigurationValidationResult>(result);
        }

        private bool AllRoutesForAggregateExist(FileAggregateRoute fileAggregateRoute, List<FileRoute> routes)
        {
            var routesForAggregate = routes.Where(r => fileAggregateRoute.RouteKeys.Contains(r.Key));

            return routesForAggregate.Count() == fileAggregateRoute.RouteKeys.Count;
        }

        private bool IsPlaceholderNotDuplicatedIn(string upstreamPathTemplate)
        {
            Regex regExPlaceholder = new Regex("{[^}]+}");
            var matches = regExPlaceholder.Matches(upstreamPathTemplate);
            var upstreamPathPlaceholders = matches.Select(m => m.Value);
            return upstreamPathPlaceholders.Count() == upstreamPathPlaceholders.Distinct().Count();
        }

        private static bool DoesNotContainRoutesWithSpecificRequestIdKeys(FileAggregateRoute fileAggregateRoute,
            List<FileRoute> routes)
        {
            var routesForAggregate = routes.Where(r => fileAggregateRoute.RouteKeys.Contains(r.Key));

            return routesForAggregate.All(r => string.IsNullOrEmpty(r.RequestIdKey));
        }

        private static bool IsNotDuplicateIn(FileRoute route,
            List<FileRoute> routes)
        {
            var matchingRoutes = routes
                .Where(r => r.UpstreamPathTemplate == route.UpstreamPathTemplate
                            && r.UpstreamHost == route.UpstreamHost)
                .ToList();

            if (matchingRoutes.Count == 1)
            {
                return true;
            }

            var allowAllVerbs = matchingRoutes.Any(x => x.UpstreamHttpMethod.Count == 0);

            var duplicateAllowAllVerbs = matchingRoutes.Count(x => x.UpstreamHttpMethod.Count == 0) > 1;

            var specificVerbs = matchingRoutes.Any(x => x.UpstreamHttpMethod.Count != 0);

            var duplicateSpecificVerbs = matchingRoutes.SelectMany(x => x.UpstreamHttpMethod).GroupBy(x => x.ToLower()).SelectMany(x => x.Skip(1)).Any();

            if (duplicateAllowAllVerbs || duplicateSpecificVerbs || (allowAllVerbs && specificVerbs))
            {
                return false;
            }

            return true;
        }

        private static bool IsNotDuplicateIn(FileRoute route,
            List<FileAggregateRoute> aggregateRoutes)
        {
            var duplicate = aggregateRoutes
                .Any(a => a.UpstreamPathTemplate == route.UpstreamPathTemplate
                            && a.UpstreamHost == route.UpstreamHost
                            && route.UpstreamHttpMethod.Select(x => x.ToLower()).Contains("get"));

            return !duplicate;
        }

        private static bool IsNotDuplicateIn(FileAggregateRoute route,
            List<FileAggregateRoute> aggregateRoutes)
        {
            var matchingRoutes = aggregateRoutes
                .Where(r => r.UpstreamPathTemplate == route.UpstreamPathTemplate
                            && r.UpstreamHost == route.UpstreamHost)
                .ToList();

            return matchingRoutes.Count <= 1;
        }
    }
}
