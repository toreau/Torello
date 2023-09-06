namespace Torello.Application.Common.Settings;

public class SwaggerSettings
{
    public const string SectionName = "SwaggerSettings";
    public string Version { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ContactName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
}
