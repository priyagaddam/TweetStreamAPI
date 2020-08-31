using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TweetStream.Infrastructure.Helpers;
using TweetStream.Infrastructure.Models;

namespace TweetStream.Infrastructure
{
    public class CachedTwitterStreamService : ITwitterStreamService
    {
        private readonly IMemoryCache _cache;
        private static readonly string _tweetsKey = "realTimeTweets";

        public CachedTwitterStreamService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private List<TwitterModel> Tweets
        {
            get
            {
                var tweets = _cache?.Get<List<TwitterModel>>(_tweetsKey);
                return tweets;
            }
        }

        public void AddTweets(List<TwitterModel> tweets)
        {
            _cache.Set(_tweetsKey, tweets);
        }

        public long GetTotalTweetsCount()
        {
            var tweets = _cache.Get<List<TwitterModel>>(_tweetsKey);
            return tweets?.Count ?? 0;
        }

        public Dictionary<string, int> GetTopHashTags()
        {
            var hashtags = Tweets.Where(t => t.Data.Entities != null && t.Data.Entities.Hashtags != null)
                .SelectMany(t => t.Data.Entities.Hashtags).ToList();
            var topHashTags = hashtags.GroupBy(h => h.Tag).Select(g => new
            {
                Hashtag = g.Key,
                Count = g.Count()
            }).OrderByDescending(x => x.Count).Take(10).ToDictionary(x => x.Hashtag, x => x.Count);

            return topHashTags;
        }

        public Dictionary<string, int> GetTopEmojis()
        {
            var data = Tweets.Select(t => t.Data.Text).ToList();
            var emojiDictionary = new Dictionary<string, int>();
            foreach (var em in data)
            {
                var (emoji, isMatch) = Helper.HasEmoji(em);
                if (!isMatch) continue;
                if (!emojiDictionary.ContainsKey(emoji))
                {
                    emojiDictionary[emoji] = 1;
                }

                else
                {
                    emojiDictionary[emoji]++;
                }
            }

            return emojiDictionary.OrderByDescending(d => d.Value).Take(20).ToDictionary(d => d.Key, d => d.Value);
        }

        public double GetEmojisPercentage()
        {
            var data = Tweets.Select(t => t.Data.Text).ToList();
            var emojiCount = 0;
            foreach (var em in data)
            {
                var (emoji, isMatch) = Helper.HasEmoji(em);
                if (isMatch)
                {
                    emojiCount++;
                }
            }

            var percentage = Helper.GetPercentage(emojiCount, data.Count);
            return percentage;
        }

        public double GetTweetsWithUrlPercentage()
        {
            var urls = GetTweetWithUrls();

            if (urls.Count <= 0) return 0;
            var percentage = Helper.GetPercentage(urls.Count, Tweets.Count);
            return percentage;
        }

        public double GetTweetsWithPicsPercentage()
        {
            var urls = GetTweetWithUrls();
            var picsUrlCount = urls.Count(u =>
                u.DisplayUrl.Contains("instagram.com") || u.DisplayUrl.Contains("pic.twitter.com"));

            if (urls.Count <= 0) return 0;
            var percentage = Helper.GetPercentage(picsUrlCount, Tweets.Count);
            return percentage;
        }

        public Dictionary<string, int> GetTopDomainUrl()
        {
            var urls = GetTweetWithUrls();

            var domains = urls.Select(url =>
            {
                var myUri = new Uri(url.ExpandedUrl);
                return myUri.Host;
            }).GroupBy(d => d).Select(g => new
            {
                DomainName = g.Key,
                Count = g.Count()
            }).OrderByDescending(d => d.Count).Take(10).ToDictionary(x => x.DomainName, x => x.Count);

            return domains;
        }

        private List<Url> GetTweetWithUrls()
        {
            var urls = Tweets.Where(t => t.Data.Entities?.Urls != null)
                .SelectMany(t => t.Data.Entities.Urls).ToList();
            return urls;
        }
    }
}