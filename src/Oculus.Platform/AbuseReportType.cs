namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum AbuseReportType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("OBJECT")] Object,
        [Description("USER")] User,
    }
}
