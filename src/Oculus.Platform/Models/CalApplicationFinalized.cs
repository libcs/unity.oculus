namespace Oculus.Platform.Models
{
    using System;

    public class CalApplicationFinalized
    {
        public readonly int CountdownMS;
        public readonly ulong ID;
        public readonly string LaunchDetails;

        public CalApplicationFinalized(IntPtr o)
        {
            CountdownMS = CAPI.ovr_CalApplicationFinalized_GetCountdownMS(o);
            ID = CAPI.ovr_CalApplicationFinalized_GetID(o);
            LaunchDetails = CAPI.ovr_CalApplicationFinalized_GetLaunchDetails(o);
        }
    }
}
