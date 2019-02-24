using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Responses;

namespace Ocelot.Requester
{
    public class HttpClientHttpRequester : IHttpRequester
    {
        private readonly IHttpClientCache _cacheHandlers;
        private readonly IOcelotLogger _logger;
        private readonly IDelegatingHandlerHandlerFactory _factory;
        private readonly IExceptionToErrorMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientHttpRequester(IOcelotLoggerFactory loggerFactory,
            IHttpClientCache cacheHandlers,
            IDelegatingHandlerHandlerFactory factory, 
            IExceptionToErrorMapper mapper,
            IHttpClientFactory clientFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpClientHttpRequester>();
            _cacheHandlers = cacheHandlers;
            _factory = factory;
            _mapper = mapper;
            _clientFactory = clientFactory;
        }

        public async Task<Response<HttpResponseMessage>> GetResponse(DownstreamContext context)
        {
            var builder = new HttpClientBuilder(_factory, _cacheHandlers, _logger, _clientFactory);

            var httpClient = builder.Create(context);

            try
            {
                var response = await httpClient.SendAsync(context.DownstreamRequest.ToHttpRequestMessage());
                return new OkResponse<HttpResponseMessage>(response);
            }
            catch (Exception exception)
            {
                var error = _mapper.Map(exception);
                return new ErrorResponse<HttpResponseMessage>(error);
            }
            finally
            {
                builder.Save();
            }
        }
    }
}
