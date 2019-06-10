namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum PartyUpdateAction : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("Join")] Join,
        [Description("Leave")] Leave,
        [Description("Invite")] Invite,
        [Description("Uninvite")] Uninvite,
    }
}
