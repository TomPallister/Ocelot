﻿using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Ocelot.Configuration.File;
using Ocelot.Errors;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Validator
{
    public class FileConfigurationFluentValidator : AbstractValidator<FileConfiguration>, IConfigurationValidator
    {
        public FileConfigurationFluentValidator(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            RuleFor(configuration => configuration.ReRoutes)
                .SetCollectionValidator(new ReRouteFluentValidator(authenticationSchemeProvider));
                
            RuleForEach(configuration => configuration.ReRoutes)
                .Must((config, reRoute) => IsNotDuplicateIn(reRoute, config.ReRoutes))
                .WithMessage((config, reRoute) => $"{nameof(reRoute)} {reRoute.UpstreamPathTemplate} has duplicate");

            RuleForEach(configuration => configuration.Aggregates)
                .Must((config, aggregateReRoute) => AllReRoutesForAggregateExist(aggregateReRoute, config.ReRoutes))
                .WithMessage((config, aggregateReRoute) => $"ReRoutes for {nameof(aggregateReRoute)} {aggregateReRoute.UpstreamPathTemplate} either do not exist or do not have correct Key property");

            RuleForEach(configuration => configuration.Aggregates)
                .Must((config, aggregateReRoute) => DoesNotContainReRoutesWithSpecificRequestIdKeys(aggregateReRoute, config.ReRoutes))
                .WithMessage((config, aggregateReRoute) => $"{nameof(aggregateReRoute)} {aggregateReRoute.UpstreamPathTemplate} contains ReRoute with specific RequestIdKey, this is not possible with Aggregates");
        }

        private bool AllReRoutesForAggregateExist(FileAggregateReRoute fileAggregateReRoute, List<FileReRoute> reRoutes)
        {
            var reRoutesForAggregate = reRoutes.Where(r => fileAggregateReRoute.ReRouteKeys.Contains(r.Key));

            return reRoutesForAggregate.Count() == fileAggregateReRoute.ReRouteKeys.Count;
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

        private static bool DoesNotContainReRoutesWithSpecificRequestIdKeys(FileAggregateReRoute fileAggregateReRoute, 
            List<FileReRoute> reRoutes)
        {
            var reRoutesForAggregate = reRoutes.Where(r => fileAggregateReRoute.ReRouteKeys.Contains(r.Key));

            return reRoutesForAggregate.All(r => string.IsNullOrEmpty(r.RequestIdKey));
        }

        private static bool IsNotDuplicateIn(FileReRoute reRoute, List<FileReRoute> reRoutes)
        {
            var matchingReRoutes = reRoutes
                .Where(r => r.UpstreamPathTemplate == reRoute.UpstreamPathTemplate && (r.UpstreamHost != reRoute.UpstreamHost || reRoute.UpstreamHost == null)).ToList();

            if(matchingReRoutes.Count == 1)
            {
                return true;
            }

            var allowAllVerbs = matchingReRoutes.Any(x => x.UpstreamHttpMethod.Count == 0);

            var duplicateAllowAllVerbs = matchingReRoutes.Count(x => x.UpstreamHttpMethod.Count == 0) > 1;

            var specificVerbs = matchingReRoutes.Any(x => x.UpstreamHttpMethod.Count != 0);

            var duplicateSpecificVerbs = matchingReRoutes.SelectMany(x => x.UpstreamHttpMethod).GroupBy(x => x.ToLower()).SelectMany(x => x.Skip(1)).Any();

            if (duplicateAllowAllVerbs || duplicateSpecificVerbs || (allowAllVerbs && specificVerbs))
            {
                return false;
            }

            return true;
        }
    }
}
