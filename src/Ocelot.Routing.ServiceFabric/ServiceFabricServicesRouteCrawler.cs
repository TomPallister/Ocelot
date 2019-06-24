﻿using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Common;
using Ocelot.Configuration.File;
using Ocelot.Routing.ServiceFabric.Models.Routing;
using Ocelot.Routing.ServiceFabric.Models.ServiceFabric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ocelot.Routing.ServiceFabric
{
    /// <summary>
    /// Service fabric services route crawler.
    /// </summary>
    internal class ServiceFabricServicesRouteCrawler : IServiceFabricServicesRouteCrawler
    {
        private readonly IServiceFabricClientFactory serviceFabricClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFabricServicesRouteCrawler"/> class.
        /// </summary>
        /// <param name="serviceFabricClientFactory">Service fabric client factory.</param>
        public ServiceFabricServicesRouteCrawler(IServiceFabricClientFactory serviceFabricClientFactory)
        {
            this.serviceFabricClientFactory = serviceFabricClientFactory;
        }

        /// <summary>
        /// Gets the collection <see cref="ServiceRouteInfo"/> representing all the services running on cluster which support Ocelot routing requests to them.
        /// </summary>
        /// <returns>Collection of <see cref="ServiceRouteInfo"/></returns>
        public async Task<IEnumerable<ServiceRouteInfo>> GetAggregatedServiceRouteInfoAsync()
        {
            IServiceFabricClient serviceFabricClient = await this.serviceFabricClientFactory.GetServiceFabricClientAsync().ConfigureAwait(false);
            List<ApplicationInfo> applicationCollection = await ServiceFabricServicesRouteCrawler.GetApplicationCollectionAsync(serviceFabricClient).ConfigureAwait(false);

            Dictionary<ApplicationInfo, IEnumerable<ServiceInfo>> servicesMap = new Dictionary<ApplicationInfo, IEnumerable<ServiceInfo>>();

            foreach (ApplicationInfo applicationInfo in applicationCollection)
            {
                try
                {
                    IEnumerable<ServiceInfo> services = await ServiceFabricServicesRouteCrawler.GetServiceCollectionAsync(serviceFabricClient, applicationInfo.Id).ConfigureAwait(false);
                    servicesMap[applicationInfo] = services;
                }
                catch
                {
                }
            }

            List<ServiceRouteInfo> ocelotVisibleServiceRoutes = new List<ServiceRouteInfo>();

            foreach (KeyValuePair<ApplicationInfo, IEnumerable<ServiceInfo>> servicesEntry in servicesMap)
            {
                foreach (ServiceInfo service in servicesEntry.Value)
                {
                    try
                    {
                        ServiceRouteInfo serviceRouteInfo = await ServiceFabricServicesRouteCrawler.GetServiceRouteInfoAsync(
                            serviceFabricClient,
                            servicesEntry.Key,
                            service).ConfigureAwait(false);

                        if (serviceRouteInfo != null)
                        {
                            ocelotVisibleServiceRoutes.Add(serviceRouteInfo);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return ocelotVisibleServiceRoutes;
        }

        private static async Task<List<ApplicationInfo>> GetApplicationCollectionAsync(IServiceFabricClient serviceFabricClient)
        {
            List<ApplicationInfo> applicationCollection = new List<ApplicationInfo>();

            PagedData<ApplicationInfo> applications = await serviceFabricClient.Applications.GetApplicationInfoListAsync().ConfigureAwait(false);

            applicationCollection.AddRange(applications.Data);

            while (applications.ContinuationToken.Next)
            {
                applications = await serviceFabricClient.Applications.GetApplicationInfoListAsync(continuationToken: applications.ContinuationToken).ConfigureAwait(false);

                applicationCollection.AddRange(applications.Data);
            }

            return applicationCollection;
        }

        private static async Task<List<ServiceInfo>> GetServiceCollectionAsync(IServiceFabricClient serviceFabricClient, string applicationId)
        {
            List<ServiceInfo> serviceCollection = new List<ServiceInfo>();

            PagedData<ServiceInfo> services = await serviceFabricClient.Services.GetServiceInfoListAsync(applicationId).ConfigureAwait(false);

            serviceCollection.AddRange(services.Data);

            while (services.ContinuationToken.Next)
            {
                services = await serviceFabricClient.Services.GetServiceInfoListAsync(applicationId, continuationToken: services.ContinuationToken).ConfigureAwait(false);

                serviceCollection.AddRange(services.Data);
            }

            return serviceCollection;
        }

        private static async Task<ServiceRouteInfo> GetServiceRouteInfoAsync(
            IServiceFabricClient serviceFabricClient,
            ApplicationInfo applicationInfo,
            ServiceInfo serviceInfo)
        {
            ServiceTypeInfo serviceTypeInfo = await serviceFabricClient.ServiceTypes.GetServiceTypeInfoByNameAsync(
                applicationInfo.TypeName,
                applicationInfo.TypeVersion,
                serviceInfo.TypeName).ConfigureAwait(false);

            XmlSerializer manifestSerializer = new XmlSerializer(typeof(ServiceFabricManifest));

            ServiceTypeManifest serviceManifest = await serviceFabricClient.ServiceTypes.GetServiceManifestAsync(
                applicationInfo.TypeName,
                applicationInfo.TypeVersion,
                serviceTypeInfo.ServiceManifestName).ConfigureAwait(false);

            ServiceFabricManifest manifest = manifestSerializer.Deserialize(new StringReader(serviceManifest.Manifest)) as ServiceFabricManifest;

            if (!(manifest?.ServiceTypes?.StatelessServiceType?.Extensions?.Any()).GetValueOrDefault())
            {
                return null;
            }

            if (!(manifest?.Resources?.Endpoints?.Any()).GetValueOrDefault())
            {
                return null;
            }

            ServiceFabricManifestExtension ocelotExtension = manifest.ServiceTypes.StatelessServiceType.Extensions.FirstOrDefault(extension =>
                extension.Name.Equals(OcelotRoutingLabels.OcelotRoutingExtensionName, StringComparison.OrdinalIgnoreCase));

            if (ocelotExtension == null)
            {
                return null;
            }

            ILookup<string, string> labelMap = ocelotExtension.Labels.ToLookup((label) => label.Key, (label) => label.Value, StringComparer.OrdinalIgnoreCase);

            // Get route templates.
            IEnumerable<string> routeTemplateLabelValue = labelMap[OcelotRoutingLabels.RouteTemplatesLabel];

            string[] routeEntries = string.Join(",", routeTemplateLabelValue).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (routeEntries.Length == 0)
            {
                // Nothing to route, no point in adding to the list.
                return null;
            }

            ServiceRouteInfo serviceRouteInfo = new ServiceRouteInfo
            {
                RouteTemplates = routeEntries,
            };

            // Request tracking header.
            IEnumerable<string> requestTrackingHeader = labelMap[OcelotRoutingLabels.RequestTrackingHeader];

            if (requestTrackingHeader.Any())
            {
                serviceRouteInfo.RequestTrackingHeader = requestTrackingHeader.Last();
            }

            serviceRouteInfo.ServiceName = serviceInfo.Name.GetId();
            serviceRouteInfo.DownstreamScheme = manifest.Resources.Endpoints.First().Protocol;

            return serviceRouteInfo;
        }
    }
}
