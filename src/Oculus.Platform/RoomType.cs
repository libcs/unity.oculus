namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum RoomType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("MATCHMAKING")] Matchmaking,
        [Description("MODERATED")] Moderated,
        [Description("PRIVATE")] Private,
        [Description("SOLO")] Solo,
    }
}
