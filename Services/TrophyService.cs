using System.Collections.Concurrent;
using System.Linq;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public class TrophyService : ITrophyService
    {
        private readonly TrophyIndexer _trophyIndexer;
        private readonly ILogger<TrophyService> _logger;

        public TrophyService(ILogger<TrophyService> logger, TrophyIndexer trophyIndexer)
        {
            _trophyIndexer = trophyIndexer;
            _logger = logger;   
        }

        public Trophy? GetTrophyBySlug(string slug)
        {
            _logger.LogInformation($"Retrieve Trophy for slug {slug}");

            // Retrieve the cached trophy data from TrophyIndexer
            var trophy = _trophyIndexer.FindTrophyBySlug(slug);
            return trophy;
        }

        public string? GetTrophyUrl(string slug)
        {
            _logger.LogInformation($"Lookup Trophy URL for slug {slug}");

            var trophy = _trophyIndexer.FindTrophyBySlug(slug);
            return trophy != null ? trophy.Url : null;
        }
    }
}
