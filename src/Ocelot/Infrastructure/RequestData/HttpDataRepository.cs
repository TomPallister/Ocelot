using Microsoft.AspNetCore.Http;
using Ocelot.Responses;
using System;

namespace Ocelot.Infrastructure.RequestData
{
    public class HttpDataRepository : IRequestScopedDataRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpDataRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Response Add<T>(string key, T value)
        {
            try
            {
                _httpContextAccessor.HttpContext.Items.Add(key, value);
                return new OkResponse();
            }
            catch (Exception exception)
            {
                return new ErrorResponse(new CannotAddDataError(string.Format($"Unable to add data for key: {key}, exception: {exception.Message}")));
            }
        }

        public Response Update<T>(string key, T value)
        {
            try
            {
                _httpContextAccessor.HttpContext.Items[key] = value;
                return new OkResponse();
            }
            catch (Exception exception)
            {
                return new ErrorResponse(new CannotAddDataError(string.Format($"Unable to update data for key: {key}, exception: {exception.Message}")));
            }
        }

        public Response<T> Get<T>(string key)
        {
            object obj;

            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Items == null)
            {
                return new ErrorResponse<T>(new CannotFindDataError($"Unable to find data for key: {key} because HttpContext or HttpContext.Items is null"));
            }

            if (_httpContextAccessor.HttpContext.Items.TryGetValue(key, out obj))
            {
                var data = (T)obj;
                return new OkResponse<T>(data);
            }

            return new ErrorResponse<T>(new CannotFindDataError($"Unable to find data for key: {key}"));
        }
    }
}
