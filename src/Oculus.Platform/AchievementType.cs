namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum AchievementType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("SIMPLE")] Simple,
        [Description("BITFIELD")] Bitfield,
        [Description("COUNT")] Count,
    }
}
