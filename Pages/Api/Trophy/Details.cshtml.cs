using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using TrophiesDisplay.Services;

[Route("api/trophy/details/{slug}")]
public class TrophyDetailsModel : PageModel
{
    private readonly ITrophyService _trophyService;

    public TrophyDetailsModel(ITrophyService trophyService)
    {
        _trophyService = trophyService;
    }

    public IActionResult OnGet(string slug)
    {
        var trophy = _trophyService.GetTrophyBySlug(slug);
        if (trophy == null)
        {
            return NotFound();
        }

        return Partial("_TrophyDetails", trophy);
    }
}
