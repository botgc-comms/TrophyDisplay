using Microsoft.Extensions.Options;
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
        private readonly AppSettings _settings;

        public TrophyService(ILogger<TrophyService> logger, IOptions<AppSettings> settings, TrophyIndexer trophyIndexer)
        {
            _logger = logger;
            _settings = settings.Value;

            _trophyIndexer = trophyIndexer;
        }

        public TrophyDto? GetTrophyBySlug(string slug)
        {
            _logger.LogInformation($"Retrieve Trophy details for slug {slug}");

            // Retrieve the trophy
            var trophy = _trophyIndexer.FindTrophyBySlug(slug);
            if (trophy == null) return null;

            // Find the previous and next trophies
            var previousTrophy = _trophyIndexer.FindPreviousTrophyBySlug(slug);
            var nextTrophy = _trophyIndexer.FindNextTrophyBySlug(slug);

            // Map Trophy to TrophyDto
            return new TrophyDto
            {
                Slug = trophy.Slug,
                Name = trophy.Name,
                Description = trophy.Description,
                QRCodeSVG = trophy.QRCodeSVG,
                TrophyImage = trophy.ImageUrl,
                WinnerImage = trophy.WinnerImage,
                Url = trophy.Url,
                PreviousSlug = previousTrophy?.Slug,
                NextSlug = nextTrophy?.Slug
            };
        }

        public List<string> GetNextTrophySlugs(List<string>? lastSlugs = null)
        {
            var now = DateTime.UtcNow;

            // Calculate the number of trophies to return based on display duration
            var displayDurationPerTrophy = TimeSpan.FromSeconds(_settings.TrophyAirtimeSecs);
            var batchSize = (int)(TimeSpan.FromMinutes(_settings.AvoidRepeatingWithinMins).TotalSeconds / displayDurationPerTrophy.TotalSeconds);

            // Fetch the latest trophies from the indexer
            var allTrophies = _trophyIndexer!.Trophies.ToList();

            // Filter out recently displayed trophies based on `lastSlugs`
            var recentSlugsSet = lastSlugs?.ToHashSet() ?? new HashSet<string>();
            var eligibleTrophies = allTrophies
                .Where(t => !recentSlugsSet.Contains(t.Value.Slug))
                .ToList();

            // Get unique letters from eligible trophies
            var letterGroups = eligibleTrophies
                .GroupBy(t => t.Value.Slug[0])
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Value.YearAwarded).ToList());

            // Random number generator
            var random = new Random();

            // Result list and tracking for recently selected letters
            var resultSlugs = new List<string>();
            char? lastSelectedLetter = null;

            while (resultSlugs.Count < batchSize)
            {
                // If all eligible trophies are exhausted, start using the full list
                if (!letterGroups.Any(g => g.Value.Count > 0))
                {
                    eligibleTrophies = allTrophies.Where(t => !resultSlugs.Contains(t.Value.Slug)).ToList();
                    letterGroups = eligibleTrophies
                        .GroupBy(t => t.Value.Slug[0])
                        .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Value.YearAwarded).ToList());
                }

                // Select a random letter that isn't the same as the last selected one
                var availableLetters = letterGroups.Keys.Where(l => l != lastSelectedLetter).ToList();
                if (availableLetters.Count == 0)
                {
                    availableLetters = letterGroups.Keys.ToList();
                }
                var selectedLetter = availableLetters[random.Next(availableLetters.Count)];
                lastSelectedLetter = selectedLetter;

                // Randomly select a trophy from the chosen letter group
                var groupTrophies = letterGroups[selectedLetter];
                if (groupTrophies.Count > 0)
                {
                    var selectedIndex = random.Next(groupTrophies.Count);
                    var selectedTrophy = groupTrophies[selectedIndex];

                    // Add the selected trophy and its neighbours
                    var cluster = groupTrophies
                        .Skip(Math.Max(0, selectedIndex - 1))
                        .Take(3) // Selected trophy + up to 2 neighbours
                        .ToList();

                    foreach (var trophy in cluster)
                    {
                        if (!resultSlugs.Contains(trophy.Value.Slug))
                        {
                            resultSlugs.Add(trophy.Value.Slug);
                        }
                    }

                    // Remove the selected trophies from the group
                    letterGroups[selectedLetter] = groupTrophies
                        .Where(t => !cluster.Contains(t))
                        .ToList();
                }
            }

            // Ensure the result matches the batch size
            return resultSlugs.Take(batchSize).ToList();
        }

    }
}
