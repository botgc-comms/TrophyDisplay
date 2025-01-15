using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public class TrophyIndexer : IHostedService
    {
        private readonly ILogger<TrophyIndexer> _logger;
        private readonly ITrophyDiscovery _discovery;
        private readonly ITrophyFactory _factory;
        private readonly string _id;
        private ConcurrentDictionary<string, Trophy> _trophiesCache;
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

        private void IndexTrophies()
        {
            _logger.LogInformation("Indexing trophies...");

            var newCache = new ConcurrentDictionary<string, Trophy>();

            foreach (var metadata in _discovery.DiscoverTrophies())
            {
                var trophy = _factory.Create(metadata);
                if (trophy != null)
                {
                    newCache[trophy.Slug.ToUpper()] = trophy;
                }
            }

            if (!newCache.IsEmpty)
                Interlocked.Exchange(ref _trophiesCache, newCache);

            _logger.LogInformation("Trophy indexing completed. {Count} trophies indexed.", newCache.Count);
        }
    }
}
