namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum MatchmakingCriterionImportance : int
    {
        [Description("REQUIRED")] Required,
        [Description("HIGH")] High,
        [Description("MEDIUM")] Medium,
        [Description("LOW")] Low,
        [Description("UNKNOWN")] Unknown,
    }
}
