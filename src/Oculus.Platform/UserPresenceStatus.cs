namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum UserPresenceStatus : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("ONLINE")] Online,
        [Description("OFFLINE")] Offline,
    }
}
