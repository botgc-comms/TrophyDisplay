using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public interface ITrophyFactory
    {
        Trophy? Create(TrophyMetadata metadata);
    }
}