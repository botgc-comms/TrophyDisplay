namespace TrophiesDisplay.Dtos
{
    public class SlugBatchDto
    {
        /// <summary>
        /// The timestamp when this batch of slugs was generated or received.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The list of slugs included in this batch.
        /// </summary>
        public List<string> Slugs { get; set; } = new();
    }
}
