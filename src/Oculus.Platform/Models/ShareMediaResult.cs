namespace Oculus.Platform.Models
{
    using System;

    public class ShareMediaResult
    {
        public readonly ShareMediaStatus Status;

        public ShareMediaResult(IntPtr o) => Status = CAPI.ovr_ShareMediaResult_GetStatus(o);
    }
}
