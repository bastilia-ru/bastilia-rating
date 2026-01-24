namespace Bastilia.Rating.Database.Entities
{
    public class UserBirthdayParty
    {
        public int UserBirthdayPartyId { get; set; }
        public int JoinRpgUserId { get; set; }
        public DateOnly PartyDate { get; set; }

        public int Length { get; set; } = 1;

        public User User { get; set; } = null!;
    }
}