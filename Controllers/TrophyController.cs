using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrophiesDisplay.Services;

namespace TrophiesDisplay.Controllers
{
    [ApiController]
    [Route("api/trophy")]
    public class TrophyController : ControllerBase
    {
        private readonly ITrophyService _trophyService;

        public TrophyController(ITrophyService trophyService)
        {
            _trophyService = trophyService;
        }

        [HttpGet("/{slug}")]
        public IActionResult SearchTrophy(string slug)
        {
            var trophyDto = _trophyService.GetTrophyBySlug(slug);

            if (trophyDto == null)
            {
               return NotFound(new
               {
                   error = "Trophy not found",
                   slug = "A01",
                   message = "No trophy exists with the provided slug."
               });
            }
            else
            {
                return Ok(trophyDto);
            }
        }
    }
}
