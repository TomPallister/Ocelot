using Ocelot.Values;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

// nothing in use
namespace Ocelot.UnitTests.ServiceDiscovery
{
    public class ServiceRegistryTests
    {
        private Service _service;
        private List<Service> _services;
        private ServiceRegistry _serviceRegistry;
        private ServiceRepository _serviceRepository;

        public ServiceRegistryTests()
        {
            _serviceRepository = new ServiceRepository();
            _serviceRegistry = new ServiceRegistry(_serviceRepository);
        }

        [Fact]
        public void should_register_service()
        {
            this.Given(x => x.GivenAServiceToRegister("product", "localhost:5000", 80))
            .When(x => x.WhenIRegisterTheService())
            .Then(x => x.ThenTheServiceIsRegistered())
            .BDDfy();
        }

        [Fact]
        public void should_lookup_service()
        {
            this.Given(x => x.GivenAServiceIsRegistered("product", "localhost:600", 80))
            .When(x => x.WhenILookupTheService("product"))
            .Then(x => x.ThenTheServiceDetailsAreReturned())
            .BDDfy();
        }

        private void ThenTheServiceDetailsAreReturned()
        {
            _services[0].HostAndPort.DownstreamHost.ShouldBe(_service.HostAndPort.DownstreamHost);
            _services[0].HostAndPort.DownstreamPort.ShouldBe(_service.HostAndPort.DownstreamPort);
            _services[0].Name.ShouldBe(_service.Name);
        }

        private void WhenILookupTheService(string name)
        {
            _services = _serviceRegistry.Lookup(name);
        }

        private void GivenAServiceIsRegistered(string name, string address, int port)
        {
            _service = new Service(name, new ServiceHostAndPort(address, port), string.Empty, string.Empty, new string[0]);
            _serviceRepository.Set(_service);
        }

        private void GivenAServiceToRegister(string name, string address, int port)
        {
            _service = new Service(name, new ServiceHostAndPort(address, port), string.Empty, string.Empty, new string[0]);
        }

        private void WhenIRegisterTheService()
        {
            _serviceRegistry.Register(_service);
        }

        private void ThenTheServiceIsRegistered()
        {
            var serviceNameAndAddress = _serviceRepository.Get(_service.Name);
            serviceNameAndAddress[0].HostAndPort.DownstreamHost.ShouldBe(_service.HostAndPort.DownstreamHost);
            serviceNameAndAddress[0].HostAndPort.DownstreamPort.ShouldBe(_service.HostAndPort.DownstreamPort);
            serviceNameAndAddress[0].Name.ShouldBe(_service.Name);
        }
    }

    public interface IServiceRegistry
    {
        void Register(Service serviceNameAndAddress);

        List<Service> Lookup(string name);
    }

    public class ServiceRegistry : IServiceRegistry
    {
        private readonly IServiceRepository _repository;

        public ServiceRegistry(IServiceRepository repository)
        {
            _repository = repository;
        }

        public void Register(Service serviceNameAndAddress)
        {
            _repository.Set(serviceNameAndAddress);
        }

        public List<Service> Lookup(string name)
        {
            return _repository.Get(name);
        }
    }

    public interface IServiceRepository
    {
        List<Service> Get(string serviceName);

        void Set(Service serviceNameAndAddress);
    }

    public class ServiceRepository : IServiceRepository
    {
        private Dictionary<string, List<Service>> _registeredServices;

        public ServiceRepository()
        {
            _registeredServices = new Dictionary<string, List<Service>>();
        }

        public List<Service> Get(string serviceName)
        {
            return _registeredServices[serviceName];
        }

        public void Set(Service serviceNameAndAddress)
        {
            List<Service> services;
            if (_registeredServices.TryGetValue(serviceNameAndAddress.Name, out services))
            {
                services.Add(serviceNameAndAddress);
                _registeredServices[serviceNameAndAddress.Name] = services;
            }
            else
            {
                _registeredServices[serviceNameAndAddress.Name] = new List<Service>() { serviceNameAndAddress };
            }
        }
    }
}
