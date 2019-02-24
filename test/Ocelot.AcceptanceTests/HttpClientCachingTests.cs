namespace Ocelot.AcceptanceTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Configuration;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Requester;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class HttpClientCachingTests : IDisposable
    {
        private readonly Steps _steps;
        private string _downstreamPath;
        private readonly ServiceHandler _serviceHandler;

        public HttpClientCachingTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_cache_one_http_client_same_re_route()
        {
            var configuration = new FileConfiguration
            {
                ReRoutes = new List<FileReRoute>
                    {
                        new FileReRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = 51879,
                                }
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            var cache = new FakeHttpClientCache();

            this.Given(x => x.GivenThereIsAServiceRunningOn("http://localhost:51879", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithFakeHttpClientCache(cache))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => cache.Count.ShouldBe(1))
                .BDDfy();
        }

        [Fact]
        public void should_cache_two_http_client_different_re_route()
        {
            var configuration = new FileConfiguration
            {
                ReRoutes = new List<FileReRoute>
                {
                    new FileReRoute
                    {
                        DownstreamPathTemplate = "/",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 51879,
                            }
                        },
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    },
                    new FileReRoute
                    {
                        DownstreamPathTemplate = "/two",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 51879,
                            }
                        },
                        UpstreamPathTemplate = "/two",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            var cache = new FakeHttpClientCache();

            this.Given(x => x.GivenThereIsAServiceRunningOn("http://localhost:51879", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithFakeHttpClientCache(cache))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/two"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/two"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/two"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => cache.Count.ShouldBe(2))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl,  int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, async context =>
            {
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(responseBody);
            });
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }

        public class FakeHttpClientCache : IHttpClientCache
        {
            private readonly ConcurrentDictionary<string, ConcurrentQueue<HttpClient>> _httpClientsCache;

            public FakeHttpClientCache()
            {
                _httpClientsCache = new ConcurrentDictionary<string, ConcurrentQueue<HttpClient>>();
            }

            public void Set(string id, HttpClient client, TimeSpan expirationTime)
            {
                ConcurrentQueue<HttpClient> connectionQueue;
                if (_httpClientsCache.TryGetValue(id, out connectionQueue))
                {
                    connectionQueue.Enqueue(client);
                }
                else
                {
                    connectionQueue = new ConcurrentQueue<HttpClient>();
                    connectionQueue.Enqueue(client);
                    _httpClientsCache.TryAdd(id, connectionQueue);
                }
            }

            public bool Exists(string id)
            {
                ConcurrentQueue<HttpClient> connectionQueue;
                return _httpClientsCache.TryGetValue(id, out connectionQueue);
            }

            public HttpClient Get(string id)
            {
                HttpClient client = null;
                ConcurrentQueue<HttpClient> connectionQueue;
                if (_httpClientsCache.TryGetValue(id, out connectionQueue))
                {
                    connectionQueue.TryDequeue(out client);
                }

                return client;
            }

            public void Remove(string id)
            {
                ConcurrentQueue<HttpClient> connectionQueue;
                _httpClientsCache.TryRemove(id, out connectionQueue);
            }

            public int Count => _httpClientsCache.Count;
        }
    }
}
