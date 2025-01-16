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

        [HttpGet("search/{slug}")]
        public IActionResult SearchTrophy(string slug)
        {
            var url = _trophyService.GetTrophyUrl(slug);

            if (url != null)
            {
                return Ok(new { url });
            }
            else
            {
                return NotFound(new
                {
                    error = "Trophy not found",
                    slug = "A01",
                    message = "No trophy exists with the provided slug."
                });

            }
        }
    }
}
