namespace Bastilia.Rating.Database.Entities
{
    public class UserBirthdayParty
    {
        public int UserBirthdayPartyId { get; set; }
        public int JoinrpgUserId { get; set; }
        public DateOnly PartyDate { get; set; }

        public User User { get; set; } = null!;
    }
}