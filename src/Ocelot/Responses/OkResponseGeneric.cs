namespace Ocelot.Responses
{
#pragma warning disable SA1649 // File name must match first type name

    public class OkResponse<T> : Response<T>
#pragma warning restore SA1649 // File name must match first type name
    {
        public OkResponse(T data) : base(data)
        {
        }
    }
}
