namespace TrophiesDisplay.Models
{
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TrophyAirtimeSecs { get; set; } = 5;
        public int AvoidRepeatingWithinMins { get; set; } = 30;
    }
}
