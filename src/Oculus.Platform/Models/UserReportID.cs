namespace Oculus.Platform.Models
{
    using System;

    public class UserReportID
    {
        public readonly bool DidCancel;
        public readonly ulong ID;

        public UserReportID(IntPtr o)
        {
            DidCancel = CAPI.ovr_UserReportID_GetDidCancel(o);
            ID = CAPI.ovr_UserReportID_GetID(o);
        }
    }
}
