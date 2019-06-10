namespace Oculus.Platform
{
    using System;

    public class ApplicationOptions
    {
        readonly IntPtr Handle;

        public ApplicationOptions() => Handle = CAPI.ovr_ApplicationOptions_Create();
        ~ApplicationOptions() { CAPI.ovr_ApplicationOptions_Destroy(Handle); }
        public void SetDeeplinkMessage(string value) => CAPI.ovr_ApplicationOptions_SetDeeplinkMessage(Handle, value);
        // For passing to native C
        public static explicit operator IntPtr(ApplicationOptions options) => options != null ? options.Handle : IntPtr.Zero;
    }
}
