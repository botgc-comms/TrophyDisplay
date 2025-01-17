using System.Collections.Concurrent;
using System.Linq;
using TrophiesDisplay.Dtos;
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

        public TrophyDto? GetTrophyBySlug(string slug)
        {
            _logger.LogInformation($"Retrieve Trophy details for slug {slug}");

            // Retrieve the trophy
            var trophyDto = _trophyIndexer.FindTrophyBySlug(slug);
            if (trophyDto == null) return null;

            // Find the previous and next trophies
            var previousTrophy = _trophyIndexer.FindPreviousTrophyBySlug(slug);
            var nextTrophy = _trophyIndexer.FindNextTrophyBySlug(slug);

            // Map Trophy to TrophyDto
            return new TrophyDto
            {
                Slug = trophyDto.Slug,
                Name = trophyDto.Name,
                Description = trophyDto.Description,
                QRCodeSVG = trophyDto.QRCodeSVG,
                Url = trophyDto.Url,
                PreviousSlug = previousTrophy?.Slug,
                NextSlug = nextTrophy?.Slug
            };
        }
    }
}
