namespace Oculus.Platform.Models
{
    using System;
    using System.Collections.Generic;

    public class Purchase
    {
        public readonly DateTime ExpirationTime;
        public readonly DateTime GrantTime;
        public readonly ulong ID;
        public readonly string Sku;

        public Purchase(IntPtr o)
        {
            ExpirationTime = CAPI.ovr_Purchase_GetExpirationTime(o);
            GrantTime = CAPI.ovr_Purchase_GetGrantTime(o);
            ID = CAPI.ovr_Purchase_GetPurchaseID(o);
            Sku = CAPI.ovr_Purchase_GetSKU(o);
        }
    }

    public class PurchaseList : DeserializableList<Purchase>
    {
        public PurchaseList(IntPtr a)
        {
            var count = (int)CAPI.ovr_PurchaseArray_GetSize(a);
            _Data = new List<Purchase>(count);
            for (var i = 0; i < count; i++)
                _Data.Add(new Purchase(CAPI.ovr_PurchaseArray_GetElement(a, (UIntPtr)i)));
            _NextUrl = CAPI.ovr_PurchaseArray_GetNextUrl(a);
        }
    }
}
