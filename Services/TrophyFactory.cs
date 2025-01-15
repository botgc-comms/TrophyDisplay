using Microsoft.Extensions.Options;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public class TrophyFactory : ITrophyFactory
    {
        private readonly IQRCodeGenerator _qRCodeGenerator;
        
        private readonly ILogger<TrophyFactory> _logger;
        private readonly AppSettings _settings;

        public TrophyFactory(ILogger<TrophyFactory> logger, IOptions<AppSettings> settings, IQRCodeGenerator qrCodeGenerator)
        {
            _logger = logger;
            _settings = settings.Value;
            
            _qRCodeGenerator = qrCodeGenerator;
        }

        /// <summary>
        /// Creates a Trophy object from metadata.
        /// </summary>
        /// <param name="metadata">The metadata describing the trophy.</param>
        /// <returns>A fully constructed Trophy object.</returns>
        public Trophy? Create(TrophyMetadata metadata)
        {
            try
            {
                var trophyUrl = $"{_settings.BaseUrl}/trophy/{metadata.Slug}";

                // Read additional metadata or interpret raw metadata
                return new Trophy
                {
                    Slug = metadata.Slug,
                    Url = trophyUrl,
                    Name = metadata.Name,
                    Description = metadata.Description,
                    ImageUrl = metadata.ImageUrl,
                    WinnerImageUrl = metadata.WinnerImageUrl,
                    QRCodeSVG = _qRCodeGenerator.GetSVG(trophyUrl)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Trophy: {ex.Message}");
                return null;
            }
        }
    }
}
