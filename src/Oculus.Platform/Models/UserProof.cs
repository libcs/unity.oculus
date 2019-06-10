namespace Oculus.Platform.Models
{
    using System;

    public class UserProof
    {
        public readonly string Value;

        public UserProof(IntPtr o) => Value = CAPI.ovr_UserProof_GetNonce(o);
    }
}
