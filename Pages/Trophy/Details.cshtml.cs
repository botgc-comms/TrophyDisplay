using Microsoft.AspNetCore.Mvc.RazorPages;
using TrophiesDisplay.Models;
using TrophiesDisplay.Pages.Trophy;
using TrophiesDisplay.Services;

public class TrophyDetailsModel : PageModel
{
    private readonly ITrophyService _trophyService;

    public TrophyViewModel Trophy { get; private set; }

    public TrophyDetailsModel(ITrophyService trophyService)
    {
        _trophyService = trophyService;
    }

    public void OnGet(string slug)
    {
        var trophy = _trophyService.GetTrophyBySlug(slug);
        if (trophy == null)
        {
            Trophy = null;
            return;
        }

        // Map domain model to ViewModel
        Trophy = new TrophyViewModel
        {
            Name = trophy.Name,
            Description = trophy.Description,
            QRCodeSVG = trophy.QRCodeSVG
        };
    }
}
