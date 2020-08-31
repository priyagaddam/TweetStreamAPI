using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TweetStream.Infrastructure;

namespace TweetStreamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwitterController : ControllerBase
    {
        private readonly ITwitterStreamService _twitterStreamService;

        public TwitterController(ITwitterStreamService cachedTwitterStreamService)
        {
            _twitterStreamService = cachedTwitterStreamService;
        }

        [HttpGet]
        [Route("count")]
        public IActionResult GetTotalNumberOfTweets()
        {
            var tweets = _twitterStreamService.GetTotalTweetsCount();
            return tweets == 0
                ? Ok(new {status = "Please wait few moments for cache to be updated!"})
                : Ok(new {totalTweets = tweets});
        }


        [HttpGet]
        [Route("hashtags")]
        public IActionResult GetTopHashTags()
        {
            var tweets = _twitterStreamService.GetTopHashTags();
            return Ok(tweets);
        }

        [HttpGet]
        [Route("emojis")]
        public IActionResult GetEmojis()
        {
            var tweets = _twitterStreamService.GetTopEmojis();

            return Ok(tweets);
        }

        [HttpGet]
        [Route("emojiPercentage")]
        public IActionResult GetEmojisPercentage()
        {
            var emojiPercentage = _twitterStreamService.GetEmojisPercentage();

            return Ok(new {emojiPercentage});
        }

        [HttpGet]
        [Route("url")]
        public IActionResult GetTweetsWithUrl()
        {
            var urlPercentage = _twitterStreamService.GetTweetsWithUrlPercentage();

            return Ok(new {urlPercentage});
        }

        [HttpGet]
        [Route("pics")]
        public IActionResult GetTweetsWithPics()
        {
            var picsPercentage = _twitterStreamService.GetTweetsWithPicsPercentage();
            return Ok(new {picsPercentage});
        }

        [HttpGet]
        [Route("domain")]
        public IActionResult GetTopDomainUrl()
        {
            var tweets = _twitterStreamService.GetTopDomainUrl();

            return Ok(tweets);
        }
    }
}