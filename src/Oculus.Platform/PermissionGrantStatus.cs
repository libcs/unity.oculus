namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum PermissionGrantStatus : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("GRANTED")] Granted,
        [Description("DENIED")] Denied,
        [Description("BLOCKED")] Blocked,
    }
}
