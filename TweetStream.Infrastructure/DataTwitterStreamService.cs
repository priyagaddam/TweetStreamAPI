using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FastMember;
using Microsoft.Extensions.Configuration;
using TweetStream.Infrastructure.Helpers;
using TweetStream.Infrastructure.Models;

namespace TweetStream.Infrastructure
{
    public class DataTwitterStreamService : ITwitterStreamService
    {
        private readonly IConfiguration _configuration;

        public DataTwitterStreamService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection => new SqlConnection(_configuration.GetConnectionString("TwitterDb"));

        public SqlConnection CreateConnection()
        {
            var connection = Connection;
            if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
                connection.Open();

            return connection;
        }

        public void AddTweets(List<TwitterModel> tweets)
        {
            using (var bulk = new SqlBulkCopy(CreateConnection()))
            {
                var tweetsTable = new List<Tweet>();
                foreach (var t in tweets)
                {
                    var item = new Tweet();
                    item.Id = t.Data.Id;
                    item.Text = t.Data.Text;
                    item.Language = t.Data.Language;
                    item.CreatedAt = t.Data.CreatedAt;
                    item.AuthorId = t.Data.AuthorId;

                    if (t.Data.Entities != null)
                    {
                        item.Entities = new Entities();

                        if (t.Data.Entities.Urls != null)
                        {
                            item.Entities.Urls = new List<Url>();
                            foreach (var url in t.Data.Entities.Urls)
                            {
                                item.Entities.Urls.Add(new Url
                                {
                                    ExpandedUrl = url.ExpandedUrl,
                                    DisplayUrl = url.DisplayUrl,
                                    TweetId = t.Data.Id,
                                    CreatedAt = t.Data.CreatedAt
                                });
                            }
                        }

                        if (t.Data.Entities.Hashtags != null)
                        {
                            item.Entities.Hashtags = new List<Hashtag>();
                            foreach (var hash in t.Data.Entities.Hashtags)
                            {
                                item.Entities.Hashtags.Add(new Hashtag
                                {
                                    Tag = hash.Tag,
                                    TweetId = t.Data.Id,
                                    CreatedAt = t.Data.CreatedAt
                                });
                            }
                        }

                        if (t.Data.Entities.Urls == null && t.Data.Entities.Hashtags == null)
                        {
                            item.Entities = null;
                        }
                    }

                    tweetsTable.Add(item);
                }

                using (var reader = ObjectReader.Create(tweetsTable, "Id", "Text", "CreatedAt", "Language", "AuthorId"))
                {
                    bulk.DestinationTableName = "Tweet";
                    bulk.WriteToServer(reader);
                }

                using (var reader =
                    ObjectReader.Create(
                        tweetsTable.Where(t => t.Entities?.Urls != null).SelectMany(t => t.Entities?.Urls).ToList(),
                        "ExpandedUrl", "DisplayUrl", "TweetId", "CreatedAt"))
                {
                    bulk.DestinationTableName = "Url";
                    bulk.WriteToServer(reader);
                }

                using (var reader =
                    ObjectReader.Create(
                        tweetsTable.Where(t => t.Entities?.Hashtags != null).SelectMany(t => t.Entities?.Hashtags)
                            .ToList(), "Tag", "TweetId", "CreatedAt"))
                {
                    bulk.DestinationTableName = "Hashtag";
                    bulk.WriteToServer(reader);
                }
            }
        }

        public long GetTotalTweetsCount()
        {
            using (var con = CreateConnection())
            {
                var sql = @" SELECT count(*)   FROM [TwitterStreamDb].[dbo].[Tweet] ";

                var count = con.ExecuteScalar<long>(sql);
                return count;
            }
        }

        public Dictionary<string, int> GetTopHashTags()
        {
            using (var con = CreateConnection())
            {
                var sql = @"SELECT top 10 Tag, count(*) as Count  FROM [TwitterStreamDb].[dbo].[Hashtag]  where CreatedAt>= DATEADD(day, -1, GETUTCDATE())   group by tag   ORDER by count(*) desc";

                var data = con.Query(sql);
                return data.ToDictionary(x => (string)x.Tag, x => (int)x.Count);
            }
        }

        public Dictionary<string, int> GetTopEmojis()
        {
            using (var con = CreateConnection())
            {
                var sql = @"SELECT text   FROM [TwitterStreamDb].[dbo].[Tweet] where CreatedAt>= DATEADD(day, -1, GETUTCDATE()) order by CreatedAt desc";
                var data = con.Query<string>(sql);
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

        }

        public Dictionary<string, int> GetTopDomainUrl()
        {
            using (var con = CreateConnection())
            {
                var sql =
                    @" SELECT ExpandedUrl from [TwitterStreamDb].[dbo].[Url] where CreatedAt>= DATEADD(day, -1, GETUTCDATE()) order by CreatedAt desc";
                var data = con.Query<string>(sql);

                var domains = data.Select(url =>
                {
                    var myUri = new Uri(url);
                    return myUri.Host;
                }).GroupBy(d => d).Select(g => new
                {
                    DomainName = g.Key,
                    Count = g.Count()
                }).OrderByDescending(d => d.Count).Take(10).ToDictionary(x => x.DomainName, x => x.Count);

                return domains;
            }
        }

        public double GetEmojisPercentage()
        {
            using (var con = CreateConnection())
            {
                var sql = @"SELECT text   FROM [TwitterStreamDb].[dbo].[Tweet] where CreatedAt>= DATEADD(day, -1, GETUTCDATE()) order by CreatedAt desc";
                var data = con.Query<string>(sql).ToList();

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
        }

        public double GetTweetsWithUrlPercentage()
        {
            using (var con = CreateConnection())
            {
                var sql =
                    @" SELECT ExpandedUrl from [TwitterStreamDb].[dbo].[Url] where CreatedAt>= DATEADD(day, -1, GETUTCDATE()) order by CreatedAt desc";
                var data = con.Query<string>(sql).ToList();
                if (!data.Any()) return 0;
                var percentage = Helper.GetPercentage(data.Count(), GetTotalTweetsCount());
                return percentage;
            }
        }

        public double GetTweetsWithPicsPercentage()
        {
            using (var con = CreateConnection())
            {
                var sql =
                    @" SELECT ExpandedUrl from [TwitterStreamDb].[dbo].[Url] where CreatedAt>= DATEADD(day, -1, GETUTCDATE()) order by CreatedAt desc";
                var data = con.Query<string>(sql).ToList();
                var dataWithPics = data.Count(u => u.Contains("instagram.com") || u.Contains("pic.twitter.com"));
                if (dataWithPics <= 0) return 0;
                var percentage = Helper.GetPercentage(dataWithPics, GetTotalTweetsCount());
                return percentage;
            }
        }
    }
}