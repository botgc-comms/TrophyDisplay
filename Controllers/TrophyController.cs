using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Runtime;
using TrophiesDisplay.Dtos;
using TrophiesDisplay.Models;
using TrophiesDisplay.Services;

namespace TrophiesDisplay.Controllers
{
    [ApiController]
    [Route("api/trophy")]
    public class TrophyController : ControllerBase
    {
        private readonly ITrophyService _trophyService;
        private readonly ILogger<TrophyController> _logger;

        private readonly AppSettings _settings;

        public TrophyController(ILogger<TrophyController>  logger, IOptions<AppSettings> settings, ITrophyService trophyService)
        {
            _logger = logger;
            _settings = settings.Value;

            _trophyService = trophyService;
        }

        [HttpGet("/{slug}")]
        public IActionResult SearchTrophy(string slug)
        {
            _logger.LogInformation($"Request SearchTrophy with slug {slug}");
            
            var trophyDto = _trophyService.GetTrophyBySlug(slug);

            if (trophyDto == null)
            {
               return NotFound(new
               {
                   error = "Trophy not found",
                   slug = "A01",
                   message = "No trophy exists with the provided slug."
               });
            }
            else
            {
                return Ok(trophyDto);
            }
        }

        [HttpGet("next")]
        public IActionResult GetNextTrophySlugs([FromHeader(Name = "Cookie")] string cookieHeader)
        {
            _logger.LogInformation($"Request GetNextTrophySlugs");

            List<string> recentSlugs = new();

            var GetCookieValue = (string cookieHeader, string key) =>
            {
                if (string.IsNullOrEmpty(cookieHeader)) return null;

                var cookies = cookieHeader.Split(";").Select(c => c.Trim().Split("=")).ToDictionary(c => c[0], c => c[1]);
                return cookies.TryGetValue(key, out var value) ? Uri.UnescapeDataString(value) : null;
            };

            // Attempt to get and parse the recent slugs from the cookie
            try
            {
                var cookieValue = GetCookieValue(cookieHeader, "recentSlugs");
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    var recentData = JsonConvert.DeserializeObject<List<SlugBatchDto>>(cookieValue);
                    var avoidRepeatingWithinMins = _settings.AvoidRepeatingWithinMins;

                    // Filter slugs within the last `avoidRepeatingWithinMins`
                    var now = DateTime.UtcNow;
                    recentSlugs = recentData
                        .Where(batch => (now - batch.Timestamp).TotalMinutes <= avoidRepeatingWithinMins)
                        .SelectMany(batch => batch.Slugs)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                _logger.LogWarning(ex, "Failed to parse recent slugs from cookie.");
                // Fallback: Treat as if there are no recent slugs
                recentSlugs = new List<string>();
            }

            // Pass the filtered slugs to the trophy service
            var nextSlugs = _trophyService.GetNextTrophySlugs(recentSlugs);

            return Ok(nextSlugs);
        }
    }
}
