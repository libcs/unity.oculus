namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum LivestreamingAudience : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("PUBLIC")] Public,
        [Description("FRIENDS")] Friends,
        [Description("ONLY_ME")] OnlyMe,
    }
}
