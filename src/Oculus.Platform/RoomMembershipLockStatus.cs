namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum RoomMembershipLockStatus : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("LOCK")] Lock,
        [Description("UNLOCK")] Unlock,
    }
}
