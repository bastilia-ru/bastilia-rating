using System.Text.Json;

namespace JoinRpg.PrimitiveTypes.Test;

public class JsonRoundTripTests
{
    [Fact]
    public void BastiliaMemberShouldRoundTripThroughJson()
    {
        var instance = new BastiliaMember(1, "", null, "leo", true, new List<BastiliaStatusHistory>(), new List<ProjectAdminInfo>(), new List<MemberAchievement>(), new DateOnly(1985, 6, 12));
        var serialized = JsonSerializer.Serialize(instance).ShouldNotBeNull();
        var deserialized = JsonSerializer.Deserialize<BastiliaMember>(serialized).ShouldNotBeNull();
        deserialized.ShouldBeEquivalentTo(instance);
    }
}