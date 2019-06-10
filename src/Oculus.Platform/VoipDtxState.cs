namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum VoipDtxState : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("ENABLED")] Enabled,
        [Description("DISABLED")] Disabled,
    }
}
