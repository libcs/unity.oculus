namespace Oculus.Platform
{
    using Description = System.ComponentModel.DescriptionAttribute;

    public enum LeaderboardStartAt : int
    {
        [Description("TOP")] Top,
        [Description("CENTERED_ON_VIEWER")] CenteredOnViewer,
        [Description("CENTERED_ON_VIEWER_OR_TOP")] CenteredOnViewerOrTop,
        [Description("UNKNOWN")] Unknown,
    }
}
