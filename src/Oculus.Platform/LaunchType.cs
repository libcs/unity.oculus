namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum LaunchType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("NORMAL")] Normal,
        [Description("INVITE")] Invite,
        [Description("COORDINATED")] Coordinated,
        [Description("DEEPLINK")] Deeplink,
    }
}
