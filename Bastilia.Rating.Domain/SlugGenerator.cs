namespace Bastilia.Rating.Domain;

public static class SlugGenerator
{
    public static string Generate(int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[Random.Shared.Next(s.Length)])]);
    }
}
