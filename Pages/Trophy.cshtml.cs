using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using System.IO;
using TrophiesDisplay.Models;
using TrophiesDisplay.Services;

namespace TrophiesDisplay.Pages
{
    public class TrophyPageModel : PageModel
    {
        private readonly ITrophyService _trophyService;

        public TrophyPageModel(ITrophyService trophyService)
        {
            _trophyService = trophyService;
        }

       // public Trophy? Trophy { get; private set; }
        public string? QRCodeImageBase64 { get; private set; }

        public IActionResult OnGet(string slug)
        {
           // Trophy = _trophyService.GetTrophyBySlug(slug);
           // if (Trophy == null)
           // {
           //     return NotFound(); // Return a 404 if the trophy is not found
           // }

            // Generate QR Code
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode($"https://example.com/trophy/{slug}", QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            // Convert to Base64 string
            QRCodeImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";

            return Page();
        }
    }
}
