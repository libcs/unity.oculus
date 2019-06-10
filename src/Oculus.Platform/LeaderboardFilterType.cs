namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum LeaderboardFilterType : int
    {
        [Description("NONE")] None,
        [Description("FRIENDS")] Friends,
        [Description("UNKNOWN")] Unknown,
    }
}
