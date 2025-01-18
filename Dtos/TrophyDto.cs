namespace TrophiesDisplay.Dtos
{
    public class TrophyDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QRCodeSVG { get; set; } = string.Empty;
        public string TrophyImage { get; set; } = string.Empty;

        // Optional Links for HATEOAS
        public string? PreviousSlug { get; set; }
        public string? NextSlug { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
