namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum ShareMediaStatus : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("SHARED")] Shared,
        [Description("CANCELED")] Canceled,
    }
}
