#pragma warning disable 0618

namespace Oculus.Platform.Models
{
    using System;
    using System.Collections.Generic;

    public class AssetDetails
    {
        public readonly ulong AssetId;
        public readonly string AssetType;
        public readonly string DownloadStatus;
        public readonly string Filepath;
        public readonly string IapStatus;
        // May be null. Check before using.
        public readonly LanguagePackInfo LanguageOptional;
        [Obsolete("Deprecated in favor of LanguageOptional")]
        public readonly LanguagePackInfo Language;

        public AssetDetails(IntPtr o)
        {
            AssetId = CAPI.ovr_AssetDetails_GetAssetId(o);
            AssetType = CAPI.ovr_AssetDetails_GetAssetType(o);
            DownloadStatus = CAPI.ovr_AssetDetails_GetDownloadStatus(o);
            Filepath = CAPI.ovr_AssetDetails_GetFilepath(o);
            IapStatus = CAPI.ovr_AssetDetails_GetIapStatus(o);
            {
                var pointer = CAPI.ovr_AssetDetails_GetLanguage(o);
                Language = new LanguagePackInfo(pointer);
                LanguageOptional = pointer == IntPtr.Zero ? null : Language;
            }
        }
    }

    public class AssetDetailsList : DeserializableList<AssetDetails>
    {
        public AssetDetailsList(IntPtr a)
        {
            var count = (int)CAPI.ovr_AssetDetailsArray_GetSize(a);
            _Data = new List<AssetDetails>(count);
            for (var i = 0; i < count; i++)
                _Data.Add(new AssetDetails(CAPI.ovr_AssetDetailsArray_GetElement(a, (UIntPtr)i)));
        }
    }
}
