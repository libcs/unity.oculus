namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum KeyValuePairType : int
    {
        [Description("STRING")] String,
        [Description("INTEGER")] Int,
        [Description("DOUBLE")] Double,
        [Description("UNKNOWN")] Unknown,
    }
}
