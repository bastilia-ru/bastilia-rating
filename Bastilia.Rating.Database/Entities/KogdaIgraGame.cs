namespace Bastilia.Rating.Database.Entities
{
    public class KogdaIgraGame
    {
        public required int KogdaIgraGameId { get; set; }
        public required string Name { get; set; }
        public required DateOnly StartDate { get; set; }
        public required DateOnly EndDate { get; set; }
        public required DateTimeOffset LastUpdatedAt { get; set; }
    }
}
