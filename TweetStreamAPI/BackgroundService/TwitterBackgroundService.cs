using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TweetStream.Infrastructure;
using TweetStream.Infrastructure.Models;

namespace TweetStreamAPI.BackgroundService
{
    public class TwitterBackgroundService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TwitterBackgroundService> _logger;
        private Timer _timer;
        public IServiceProvider Services { get; }

        public TwitterBackgroundService(IHttpClientFactory clientFactory, ILogger<TwitterBackgroundService> logger,
            IServiceProvider services)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            Services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(300));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization",
                "Bearer AAAAAAAAAAAAAAAAAAAAACG7HAEAAAAA%2FMaypJNkzRqrzyOZ%2BLQIn2Kv6II%3DjxHfLzVSTA89z2GvIdSR0aKyb14RYLaH6SVkV9Lrv3ic4fm5l9");
            var response = await client.GetAsync(
                "https://api.twitter.com/2/tweets/sample/stream?tweet.fields=created_at,entities,lang,author_id",
                HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var tweets = new List<TwitterModel>();
            try
            {
                if (response.Content != null)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    // Store latest 2000 items in the cache
                    using (var sr = new StreamReader(stream))
                    {
                        string line;
                        int i = 0;
                        while ((line = sr.ReadLine()) != null && i < 2000)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                tweets.Add(JsonConvert.DeserializeObject<TwitterModel>(line));
                                i++;
                            }
                        }
                    }
                }

                using (var scope = Services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<ITwitterStreamService>();

                    scopedProcessingService.AddTweets(tweets);
                }
            }

            finally
            {
                response.Dispose();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}