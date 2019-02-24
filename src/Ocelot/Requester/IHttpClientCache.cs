namespace Ocelot.Requester
{
    using System;
    using System.Net.Http;
    using Configuration;

    public interface IHttpClientCache
    {
        bool Exists(string id);
        HttpClient Get(string id);
        void Remove(string id);

        void Set(string id, HttpClient handler, TimeSpan expirationTime);
    }
}
