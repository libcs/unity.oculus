namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum MediaContentType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("PHOTO")] Photo,
    }
}
