namespace WebServer.Configuration;

public sealed class DataOptions
{
    public const string SectionName = "Data";

    public string RootPath { get; init; } = "Data";
}
