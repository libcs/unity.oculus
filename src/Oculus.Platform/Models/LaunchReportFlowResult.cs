namespace Oculus.Platform.Models
{
    using System;

    public class LaunchReportFlowResult
    {
        public readonly bool DidCancel;
        public readonly ulong UserReportId;

        public LaunchReportFlowResult(IntPtr o)
        {
            DidCancel = CAPI.ovr_LaunchReportFlowResult_GetDidCancel(o);
            UserReportId = CAPI.ovr_LaunchReportFlowResult_GetUserReportId(o);
        }
    }
}
