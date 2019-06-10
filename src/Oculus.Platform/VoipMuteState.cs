namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum VoipMuteState : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("MUTED")] Muted,
        [Description("UNMUTED")] Unmuted,
    }
}
