using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TrophiesDisplay.Models;
using TrophiesDisplay.Services;

public class TrophyIndexer : IHostedService
{
    private readonly ILogger<TrophyIndexer> _logger;
    private readonly ITrophyDiscovery _discovery;
    private readonly ITrophyFactory _factory;
    private readonly string _id;
    private ConcurrentDictionary<string, Trophy> _trophiesCache;
    private List<Trophy> _sortedTrophies; // Sorted list of trophies
    private Timer? _refreshTimer;

    public TrophyIndexer(
        ILogger<TrophyIndexer> logger,
        ITrophyDiscovery discovery,
        ITrophyFactory factory)
    {
        _logger = logger;
        _discovery = discovery;
        _factory = factory;
        _trophiesCache = new ConcurrentDictionary<string, Trophy>();
        _sortedTrophies = new List<Trophy>();

        _id = Guid.NewGuid().ToString();
    }

    public ConcurrentDictionary<string, Trophy> Trophies => _trophiesCache;
    public string Id => _id;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Initial indexing
        IndexTrophies();

        // Set up periodic refresh
        _refreshTimer = new Timer(_ => IndexTrophies(), null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _refreshTimer?.Dispose();
        return Task.CompletedTask;
    }

    public Trophy? FindTrophyBySlug(string slug)
    {
        _trophiesCache.TryGetValue(slug.ToUpper(), out var trophy);
        return trophy;
    }

    public Trophy? FindPreviousTrophyBySlug(string slug)
    {
        lock (_sortedTrophies) // Ensure thread safety
        {
            var index = _sortedTrophies.FindIndex(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            if (index == -1) return null; // Slug not found

            // Wrap around to the last trophy if the current trophy is the first
            var previousIndex = (index - 1 + _sortedTrophies.Count) % _sortedTrophies.Count;
            return _sortedTrophies[previousIndex];
        }
    }

    public Trophy? FindNextTrophyBySlug(string slug)
    {
        lock (_sortedTrophies) // Ensure thread safety
        {
            var index = _sortedTrophies.FindIndex(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            if (index == -1) return null; // Slug not found

            // Wrap around to the first trophy if the current trophy is the last
            var nextIndex = (index + 1) % _sortedTrophies.Count;
            return _sortedTrophies[nextIndex];
        }
    }

    private void IndexTrophies()
    {
        _logger.LogInformation("Indexing trophies...");

        var newCache = new ConcurrentDictionary<string, Trophy>();
        var newSortedTrophies = new List<Trophy>();

        foreach (var metadata in _discovery.DiscoverTrophies())
        {
            var trophy = _factory.Create(metadata);
            if (trophy != null)
            {
                newCache[trophy.Slug.ToUpper()] = trophy;
                newSortedTrophies.Add(trophy);
            }
        }

        if (!newCache.IsEmpty)
        {
            Interlocked.Exchange(ref _trophiesCache, newCache);
            lock (_sortedTrophies)
            {
                _sortedTrophies = newSortedTrophies.OrderBy(t => t.Slug, StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        _logger.LogInformation("Trophy indexing completed. {Count} trophies indexed.", newCache.Count);
    }
}
