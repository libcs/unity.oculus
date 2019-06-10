namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum SdkAccountType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("OCULUS")] Oculus,
        [Description("FACEBOOK_GAMEROOM")] FacebookGameroom,
    }
}
