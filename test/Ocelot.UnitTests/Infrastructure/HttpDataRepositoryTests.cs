using Microsoft.AspNetCore.Http;
using Ocelot.Infrastructure.RequestData;
using Ocelot.Responses;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Infrastructure
{
    public class HttpDataRepositoryTests
    {
        private readonly HttpContext _httpContext;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly HttpDataRepository _httpDataRepository;
        private object _result;

        public HttpDataRepositoryTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor { HttpContext = _httpContext };
            _httpDataRepository = new HttpDataRepository(_httpContextAccessor);
        }

        /*
        TODO - Additional tests -> Type mistmatch aka Add string, request int
        TODO - Additional tests -> HttpContent null. This should never happen
        */

        [Fact]
        public void get_returns_correct_key_from_http_context()
        {
            this.Given(x => x.GivenAHttpContextContaining("key", "string"))
                .When(x => x.GetIsCalledWithKey<string>("key"))
                .Then(x => x.ThenTheResultIsAnOkResponse<string>("string"))
                .BDDfy();
        }

        [Fact]
        public void get_returns_error_response_if_the_key_is_not_found() //Therefore does not return null
        {
            this.Given(x => x.GivenAHttpContextContaining("key", "string"))
                .When(x => x.GetIsCalledWithKey<string>("keyDoesNotExist"))
                .Then(x => x.ThenTheResultIsAnErrorReposnse<string>("string1"))
                .BDDfy();
        }

        [Fact]
        public void should_update()
        {
            this.Given(x => x.GivenAHttpContextContaining("key", "string"))
                .And(x => x.UpdateIsCalledWith<string>("key", "new string"))
                .When(x => x.GetIsCalledWithKey<string>("key"))
                .Then(x => x.ThenTheResultIsAnOkResponse<string>("new string"))
                .BDDfy();
        }

        private void UpdateIsCalledWith<T>(string key, string value)
        {
            _httpDataRepository.Update(key, value);
        }

        private void GivenAHttpContextContaining(string key, object o)
        {
            _httpContext.Items.Add(key, o);
        }

        private void GetIsCalledWithKey<T>(string key)
        {
            _result = _httpDataRepository.Get<T>(key);
        }

        private void ThenTheResultIsAnErrorReposnse<T>(object resultValue)
        {
            _result.ShouldBeOfType<ErrorResponse<T>>();
            ((ErrorResponse<T>)_result).Data.ShouldBeNull();
            ((ErrorResponse<T>)_result).IsError.ShouldBe(true);
            ((ErrorResponse<T>)_result).Errors.ShouldHaveSingleItem()
                .ShouldBeOfType<CannotFindDataError>()
                .Message.ShouldStartWith("Unable to find data for key: ");
        }

        private void ThenTheResultIsAnOkResponse<T>(object resultValue)
        {
            _result.ShouldBeOfType<OkResponse<T>>();
            ((OkResponse<T>)_result).Data.ShouldBe(resultValue);
        }
    }
}
