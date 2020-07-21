using Ocelot.Responses;

namespace Ocelot.Infrastructure.RequestData
{
    public interface IRequestScopedDataRepository
    {
        Response Add<T>(string key, T value);

        Response Update<T>(string key, T value);

        Response<T> Get<T>(string key);
    }
}
