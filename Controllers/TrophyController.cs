using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime;
using System.Runtime.CompilerServices;
using TrophiesDisplay.Dtos;
using TrophiesDisplay.Models;
using TrophiesDisplay.Services;

namespace TrophiesDisplay.Controllers
{
    [ApiController]
    [Route("api/trophy")]
    public class TrophyController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ITrophyService _trophyService;
        private readonly ILogger<TrophyController> _logger;

        private readonly IFaceClient _faceClient;

        private readonly AppSettings _settings;

        public TrophyController(ILogger<TrophyController>  logger, IOptions<AppSettings> settings, IWebHostEnvironment env, ITrophyService trophyService)
        {
            _env = env;
            _logger = logger;
            _settings = settings.Value;
            
            _trophyService = trophyService;

            _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_settings.AzureFaceApi.SubscriptionKey))
            {
                Endpoint = _settings.AzureFaceApi.EndPoint
            };
        }

        [HttpGet("winnerImage/{slug}")]
        public async Task<IActionResult> GetImageWithFaceRectangles(string slug)
        {
            var trophy = _trophyService.GetTrophyBySlug(slug);
            if (trophy == null || string.IsNullOrEmpty(trophy.WinnerImage))
            {
                return NotFound($"No winner image could be found for trophy {slug}");
            }

            var imagePath = Path.Combine(_env.WebRootPath, trophy.WinnerImage.TrimStart('\\', '/'));

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound($"The winner image file {imagePath} was not found");
            }

            try
            {
                // Load the original image
                using var originalImage = Image.FromFile(imagePath);
                var image = CorrectOrientation(originalImage);

                using var graphics = Graphics.FromImage(image);

                // Detect faces
                var detectedFaces = await _faceClient.Face.DetectWithStreamAsync(
                    System.IO.File.OpenRead(imagePath),
                    returnFaceId: false,
                    returnFaceLandmarks: true,
                    returnFaceAttributes: null, 
                    recognitionModel: "recognition_04", 
                    returnRecognitionModel: true, 
                    detectionModel: "detection_03"
                );

                if (!detectedFaces.Any())
                {
                    return Ok("No faces detected in the image.");
                }

                // Process faces and identify primary subjects
                var processor = new FaceProcessor();
                var primaryFaces = processor.GetPrimaryFaces(detectedFaces, image.Width, image.Height);

                // Draw rectangles for all faces
                foreach (var face in detectedFaces)
                {
                    DrawRectangle(graphics, face.FaceRectangle, Color.Red, 2); // Red for all faces
                }

                // Draw rectangles for primary faces in green
                foreach (var primaryFace in primaryFaces)
                {
                    DrawRectangle(graphics, primaryFace.FaceRectangle, Color.Green, 4); // Green for primary faces
                }

                // Calculate and draw the crop box
                var cropBox = CalculateCropBox(primaryFaces, image.Width, image.Height);
                DrawRectangle(graphics, cropBox, Color.Blue, 4); // Blue for the crop box

                var finalImage = RestoreOriginalOrientation(image, originalImage);

                // Create a new MemoryStream for returning the image
                var memoryStream = new MemoryStream();
                finalImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Return the image stream as a response
                return File(memoryStream, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image for trophy {Slug}", slug);
                return StatusCode(500, "An error occurred while processing the image.");
            }
        }

        private Image RestoreOriginalOrientation(Image image, Image originalImage)
        {
            const int ExifOrientationTag = 0x112; // Orientation tag
            var orientationProperty = originalImage.PropertyItems.FirstOrDefault(p => p.Id == ExifOrientationTag);

            if (orientationProperty != null)
            {
                int orientation = BitConverter.ToUInt16(orientationProperty.Value, 0);
                switch (orientation)
                {
                    case 3: // Rotated 180 degrees
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 6: // Rotated 90 degrees counter-clockwise
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    case 8: // Rotated 90 degrees clockwise
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                }
            }

            return image;
        }

        private Image CorrectOrientation(Image image)
        {
            const int ExifOrientationTag = 0x112; // Orientation tag
            var orientationProperty = image.PropertyItems.FirstOrDefault(p => p.Id == ExifOrientationTag);

            if (orientationProperty != null)
            {
                int orientation = BitConverter.ToUInt16(orientationProperty.Value, 0);
                switch (orientation)
                {
                    case 3: // Rotated 180 degrees
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 6: // Rotated 90 degrees clockwise
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 8: // Rotated 90 degrees counter-clockwise
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
            }

            return image;
        }

        // Helper method to draw a rectangle on the image
        private void DrawRectangle(Graphics graphics, FaceRectangle faceRectangle, Color color, int thickness)
        {
            var rect = new Rectangle(faceRectangle.Left, faceRectangle.Top, faceRectangle.Width, faceRectangle.Height);
            using var pen = new Pen(color, thickness);
            graphics.DrawRectangle(pen, rect);
        }

        private void DrawRectangle(Graphics graphics, Rectangle rectangle, Color color, int thickness)
        {
            using var pen = new Pen(color, thickness);
            graphics.DrawRectangle(pen, rectangle);
        }

        private Rectangle CalculateCropBox(IList<DetectedFace> primaryFaces, int imageWidth, int imageHeight)
        {
            // Aspect ratio of the crop box: 800x920
            const double aspectRatio = 800.0 / 920.0;

            // Calculate the bounding rectangle for primary faces
            int minX = primaryFaces.Min(face => face.FaceRectangle.Left);
            int maxX = primaryFaces.Max(face => face.FaceRectangle.Left + face.FaceRectangle.Width);
            int minY = primaryFaces.Min(face => face.FaceRectangle.Top);
            int maxY = primaryFaces.Max(face => face.FaceRectangle.Top + face.FaceRectangle.Height);

            // Add padding around the faces (10% of the width and height of the bounding box)
            int faceBoxWidth = maxX - minX;
            int faceBoxHeight = maxY - minY;
            int paddingX = (int)(faceBoxWidth * 0.1); // 10% horizontal padding
            int paddingY = (int)(faceBoxHeight * 0.1); // 10% vertical padding

            // Expand the bounding box with padding
            minX = Math.Max(0, minX - paddingX);
            maxX = Math.Min(imageWidth, maxX + paddingX);
            minY = Math.Max(0, minY - paddingY);
            maxY = Math.Min(imageHeight, maxY + paddingY);

            // Adjust dimensions to maintain the aspect ratio
            int cropWidth = maxX - minX;
            int cropHeight = maxY - minY;

            if (cropWidth / (double)cropHeight < aspectRatio)
            {
                // Adjust width to match aspect ratio
                int adjustedWidth = (int)(cropHeight * aspectRatio);
                int widthDiff = adjustedWidth - cropWidth;
                minX = Math.Max(0, minX - widthDiff / 2);
                maxX = Math.Min(imageWidth, maxX + widthDiff / 2);
            }
            else
            {
                // Adjust height to match aspect ratio
                int adjustedHeight = (int)(cropWidth / aspectRatio);
                int heightDiff = adjustedHeight - cropHeight;
                minY = Math.Max(0, minY - heightDiff / 2);
                maxY = Math.Min(imageHeight, maxY + heightDiff / 2);
            }

            // Final crop dimensions
            cropWidth = maxX - minX;
            cropHeight = maxY - minY;

            // Vertically align faces at 1/3 of the crop box height from the top
            int faceCenterY = minY + (maxY - minY) / 2;
            int cropTop = Math.Max(0, faceCenterY - (int)(cropHeight * (1.0 / 3.0)));
            int cropBottom = cropTop + cropHeight;

            if (cropBottom > imageHeight)
            {
                cropTop = imageHeight - cropHeight;
                cropBottom = imageHeight;
            }

            return new Rectangle(minX, cropTop, cropWidth, cropHeight);
        }


        [HttpGet("/{slug}")]
        public IActionResult SearchTrophy(string slug)
        {
            _logger.LogInformation($"Request SearchTrophy with slug {slug}");
            
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

        [HttpGet("next")]
        public IActionResult GetNextTrophySlugs([FromHeader(Name = "Cookie")] string cookieHeader)
        {
            _logger.LogInformation($"Request GetNextTrophySlugs");

            List<string> recentSlugs = new();

            var GetCookieValue = (string cookieHeader, string key) =>
            {
                if (string.IsNullOrEmpty(cookieHeader)) return null;

                var cookies = cookieHeader.Split(";").Select(c => c.Trim().Split("=")).ToDictionary(c => c[0], c => c[1]);
                return cookies.TryGetValue(key, out var value) ? Uri.UnescapeDataString(value) : null;
            };

            // Attempt to get and parse the recent slugs from the cookie
            try
            {
                var cookieValue = GetCookieValue(cookieHeader, "recentSlugs");
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    var recentData = JsonConvert.DeserializeObject<List<SlugBatchDto>>(cookieValue);
                    var avoidRepeatingWithinMins = _settings.AvoidRepeatingWithinMins;

                    // Filter slugs within the last `avoidRepeatingWithinMins`
                    var now = DateTime.UtcNow;
                    recentSlugs = recentData
                        .Where(batch => (now - batch.Timestamp).TotalMinutes <= avoidRepeatingWithinMins)
                        .SelectMany(batch => batch.Slugs)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                _logger.LogWarning(ex, "Failed to parse recent slugs from cookie.");
                // Fallback: Treat as if there are no recent slugs
                recentSlugs = new List<string>();
            }

            // Pass the filtered slugs to the trophy service
            var nextSlugs = _trophyService.GetNextTrophySlugs(recentSlugs);

            return Ok(nextSlugs);
        }
    }
}
