namespace Oculus.Platform.Models
{
    using System;
    using System.Runtime.InteropServices;

    public class HttpTransferUpdate
    {
        public readonly ulong ID;
        public readonly byte[] Payload;
        public readonly bool IsCompleted;

        public HttpTransferUpdate(IntPtr o)
        {
            ID = CAPI.ovr_HttpTransferUpdate_GetID(o);
            IsCompleted = CAPI.ovr_HttpTransferUpdate_IsCompleted(o);
            var size = (long)CAPI.ovr_HttpTransferUpdate_GetSize(o);
            Payload = new byte[size];
            Marshal.Copy(CAPI.ovr_Packet_GetBytes(o), Payload, 0, (int)size);
        }
    }
}
