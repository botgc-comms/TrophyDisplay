using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public interface ITrophyService
    {
        Trophy? GetTrophyBySlug(string slug);
        string? GetTrophyUrl(string slug);
    }
}
