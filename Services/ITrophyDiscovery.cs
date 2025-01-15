using System.Collections.Generic;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public interface ITrophyDiscovery
    {
        IEnumerable<TrophyMetadata> DiscoverTrophies();
    }
}
