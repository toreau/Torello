namespace Torello.Application.Common.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; init; } = string.Empty;
    public int ExpiryInMinutes { get; init; }
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}
