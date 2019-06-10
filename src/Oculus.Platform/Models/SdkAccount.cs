namespace Oculus.Platform.Models
{
    using System;
    using System.Collections.Generic;

    public class SdkAccount
    {
        public readonly SdkAccountType AccountType;
        public readonly ulong UserId;

        public SdkAccount(IntPtr o)
        {
            AccountType = CAPI.ovr_SdkAccount_GetAccountType(o);
            UserId = CAPI.ovr_SdkAccount_GetUserId(o);
        }
    }

    public class SdkAccountList : DeserializableList<SdkAccount>
    {
        public SdkAccountList(IntPtr a)
        {
            var count = (int)CAPI.ovr_SdkAccountArray_GetSize(a);
            _Data = new List<SdkAccount>(count);
            for (var i = 0; i < count; i++)
                _Data.Add(new SdkAccount(CAPI.ovr_SdkAccountArray_GetElement(a, (UIntPtr)i)));
        }
    }
}
