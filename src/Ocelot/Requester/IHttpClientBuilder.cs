using Ocelot.Middleware;
using System.Net.Http;

namespace Ocelot.Requester
{
    public interface IHttpClientBuilder
    {
        HttpClient Create(DownstreamContext request);

        void Save();
    }
}
