namespace Oculus.Platform.Models
{
    using System;

    public class LivestreamingApplicationStatus
    {
        public readonly bool StreamingEnabled;

        public LivestreamingApplicationStatus(IntPtr o) => StreamingEnabled = CAPI.ovr_LivestreamingApplicationStatus_GetStreamingEnabled(o);
    }
}
