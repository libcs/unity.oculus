namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum PermissionType : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("MICROPHONE")] Microphone,
        [Description("WRITE_EXTERNAL_STORAGE")] WriteExternalStorage,
    }
}
