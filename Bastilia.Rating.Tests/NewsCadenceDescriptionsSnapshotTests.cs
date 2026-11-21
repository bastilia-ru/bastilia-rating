using VerifyXunit;

namespace Bastilia.Rating.Tests;

public class NewsCadenceDescriptionsSnapshotTests : VerifyBase
{
    public NewsCadenceDescriptionsSnapshotTests() : base()
    {
    }

    [Fact]
    public Task GetNewsCadenceDescription_AllValues_ProducesStableTexts()
        => Verify(Enum.GetValues<NewsCadence>().Select(c => c.GetNewsCadenceDescription()).ToList());
}
