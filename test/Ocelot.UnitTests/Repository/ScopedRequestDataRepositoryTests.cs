using Microsoft.AspNetCore.Http;
using Ocelot.Infrastructure.RequestData;
using Ocelot.Responses;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Repository
{
    public class ScopedRequestDataRepositoryTests
    {
        private IRequestScopedDataRepository _requestScopedDataRepository;
        private IHttpContextAccessor _httpContextAccesor;
        private string _key;
        private object _toAdd;
        private Response<int[]> _result;

        public ScopedRequestDataRepositoryTests()
        {
            _httpContextAccesor = new HttpContextAccessor();
            _httpContextAccesor.HttpContext = new DefaultHttpContext();
            _requestScopedDataRepository = new HttpDataRepository(_httpContextAccesor);
        }

        [Fact]
        public void should_add_item()
        {
            this.Given(x => x.GivenIHaveAnItemToAdd("blahh", new[] { 1, 2, 3, 4 }))
                .When(x => x.WhenIAddTheItem())
                .Then(x => x.ThenTheItemIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_get_item()
        {
            this.Given(x => x.GivenThereIsAnItemInTheContext("chest"))
                .When(x => x.WhenIGetTheItem())
                .Then(x => x.ThenTheItemIsReturned())
                .BDDfy();
        }

        private void ThenTheItemIsReturned()
        {
            _result.IsError.ShouldBeFalse();
            _result.Data.ShouldNotBeNull();
        }

        private void WhenIGetTheItem()
        {
            _result = _requestScopedDataRepository.Get<int[]>(_key);
        }

        private void GivenThereIsAnItemInTheContext(string key)
        {
            _key = key;
            var data = new[] { 5435345 };
            _httpContextAccesor.HttpContext.Items.Add(key, data);
        }

        private void GivenIHaveAnItemToAdd(string key, object toAdd)
        {
            _key = key;
            _toAdd = toAdd;
        }

        private void WhenIAddTheItem()
        {
            _requestScopedDataRepository.Add(_key, _toAdd);
        }

        private void ThenTheItemIsAdded()
        {
            object obj;
            _httpContextAccesor.HttpContext.Items.TryGetValue(_key, out obj).ShouldBeTrue();
        }
    }
}
