namespace Oculus.Platform
{
    using System;

    public class VoipOptions
    {
        readonly IntPtr Handle;

        public VoipOptions() => Handle = CAPI.ovr_VoipOptions_Create();
        ~VoipOptions() { CAPI.ovr_VoipOptions_Destroy(Handle); }
        public void SetBitrateForNewConnections(VoipBitrate value) => CAPI.ovr_VoipOptions_SetBitrateForNewConnections(Handle, value);
        public void SetCreateNewConnectionUseDtx(VoipDtxState value) => CAPI.ovr_VoipOptions_SetCreateNewConnectionUseDtx(Handle, value);
        // For passing to native C
        public static explicit operator IntPtr(VoipOptions options) => options != null ? options.Handle : IntPtr.Zero;
    }
}
