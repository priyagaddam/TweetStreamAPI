using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TweetStream.Infrastructure.Models
{
    public class TwitterModel
    {
        public Tweet Data { get; set; }
    }

    public class Tweet
    {
        public string Id { get; set; }
        public string Text { get; set; }

        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        public Entities Entities { get; set; }
    }

    public class Entities
    {
        public List<Url> Urls { get; set; }
        public List<Hashtag> Hashtags { get; set; }
    }

    public class Url
    {
        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("created_at")]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public string TweetId { get; set; }
    }

    public class Hashtag
    {
        public string Tag { get; set; }

        [JsonIgnore]
        public string TweetId { get; set; }

        [JsonProperty("created_at")]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
    }
}
