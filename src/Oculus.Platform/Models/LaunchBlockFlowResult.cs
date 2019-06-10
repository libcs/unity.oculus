namespace Oculus.Platform.Models
{
    using System;

    public class LaunchBlockFlowResult
    {
        public readonly bool DidBlock;
        public readonly bool DidCancel;

        public LaunchBlockFlowResult(IntPtr o)
        {
            DidBlock = CAPI.ovr_LaunchBlockFlowResult_GetDidBlock(o);
            DidCancel = CAPI.ovr_LaunchBlockFlowResult_GetDidCancel(o);
        }
    }
}
