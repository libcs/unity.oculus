namespace Oculus.Platform
{
    using System;

    public class UserOptions
    {
        readonly IntPtr Handle;

        public UserOptions() => Handle = CAPI.ovr_UserOptions_Create();
        ~UserOptions() { CAPI.ovr_UserOptions_Destroy(Handle); }
        public void SetMaxUsers(uint value) => CAPI.ovr_UserOptions_SetMaxUsers(Handle, value);
        public void AddServiceProvider(ServiceProvider value) => CAPI.ovr_UserOptions_AddServiceProvider(Handle, value);
        public void ClearServiceProviders() => CAPI.ovr_UserOptions_ClearServiceProviders(Handle);
        public void SetTimeWindow(TimeWindow value) => CAPI.ovr_UserOptions_SetTimeWindow(Handle, value);
        // For passing to native C
        public static explicit operator IntPtr(UserOptions options) => options != null ? options.Handle : IntPtr.Zero;
    }
}
