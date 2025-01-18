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
        var trophyDto = _trophyService.GetTrophyBySlug(slug);
        if (trophyDto == null)
        {
            // Return a 404 status if the trophy doesn't exist
            Response.StatusCode = 404;
            Trophy = null;
            return;
        }

        // Add previous and next slugs to the response headers
        Response.Headers["X-Previous-Slug"] = trophyDto.PreviousSlug;
        Response.Headers["X-Next-Slug"] = trophyDto.NextSlug;

        // Map domain model to ViewModel
        Trophy = new TrophyViewModel
        {
            Name = trophyDto.Name,
            Description = trophyDto.Description,
            QRCodeSVG = trophyDto.QRCodeSVG,
            TrophyImage = trophyDto.TrophyImage
        };
    }
}
