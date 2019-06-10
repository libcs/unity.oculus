namespace Oculus.Platform.Models
{
    using System;

    public class CalApplicationProposed
    {
        public readonly ulong ID;

        public CalApplicationProposed(IntPtr o) => ID = CAPI.ovr_CalApplicationProposed_GetID(o);
    }
}
