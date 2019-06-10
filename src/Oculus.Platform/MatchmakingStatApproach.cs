namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum MatchmakingStatApproach : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("TRAILING")] Trailing,
        [Description("SWINGY")] Swingy,
    }
}
