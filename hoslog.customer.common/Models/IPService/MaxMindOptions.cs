namespace hoslog.customer.common.Models.IPService;


public sealed class MaxMindOptions
{
    public const string sectionName = "MaxMind";
    public string databasePath { get; init; } = string.Empty;
}