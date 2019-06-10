namespace Oculus.Platform.Models
{
    using System;

    public class PartyID
    {
        public readonly ulong ID;

        public PartyID(IntPtr o) => ID = CAPI.ovr_PartyID_GetID(o);
    }
}
