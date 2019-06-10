namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum LivestreamingMicrophoneStatus : int
    {
        [Description("UNKNOWN")] Unknown,
        [Description("MICROPHONE_ON")] MicrophoneOn,
        [Description("MICROPHONE_OFF")] MicrophoneOff,
    }
}
