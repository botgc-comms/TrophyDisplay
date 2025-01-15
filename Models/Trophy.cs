namespace TrophiesDisplay.Models
{
    public class Trophy
    {
        public string Slug { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string QRCodeSVG { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int YearAwarded { get; set; }
        public string PresentedBy { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? WinnerImageUrl { get; set; }
        public string CompetitionFormat { get; set; } = string.Empty;
    }
}
