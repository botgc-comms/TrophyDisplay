using TrophiesDisplay.Dtos;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public interface ITrophyService
    {
        TrophyDto? GetTrophyBySlug(string slug);
        List<string> GetNextTrophySlugs(List<string>? lastSlugs = null);
    }
}
