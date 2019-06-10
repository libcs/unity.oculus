namespace Oculus.Platform
{
    using System;

    public class AbuseReportOptions
    {
        readonly IntPtr Handle;

        public AbuseReportOptions() => Handle = CAPI.ovr_AbuseReportOptions_Create();
        ~AbuseReportOptions() { CAPI.ovr_AbuseReportOptions_Destroy(Handle); }
        public void SetPreventPeopleChooser(bool value) => CAPI.ovr_AbuseReportOptions_SetPreventPeopleChooser(Handle, value);
        public void SetReportType(AbuseReportType value) => CAPI.ovr_AbuseReportOptions_SetReportType(Handle, value);
        // For passing to native C
        public static explicit operator IntPtr(AbuseReportOptions options) => options != null ? options.Handle : IntPtr.Zero;
    }
}
