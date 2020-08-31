using System.Collections.Generic;
using TweetStream.Infrastructure.Models;

namespace TweetStream.Infrastructure
{
    public interface ITwitterStreamService
    {
        void AddTweets(List<TwitterModel> tweets);
        long GetTotalTweetsCount();
        Dictionary<string, int> GetTopHashTags();
        Dictionary<string, int> GetTopEmojis();
        Dictionary<string, int> GetTopDomainUrl();
        double GetEmojisPercentage();
        double GetTweetsWithUrlPercentage();
        double GetTweetsWithPicsPercentage();
    }
}