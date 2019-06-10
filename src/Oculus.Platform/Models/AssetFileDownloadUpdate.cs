namespace Oculus.Platform.Models
{
    using System;

    public class AssetFileDownloadUpdate
    {
        public readonly ulong AssetFileId;
        public readonly ulong AssetId;
        public readonly uint BytesTotal;
        public readonly int BytesTransferred;
        public readonly bool Completed;

        public AssetFileDownloadUpdate(IntPtr o)
        {
            AssetFileId = CAPI.ovr_AssetFileDownloadUpdate_GetAssetFileId(o);
            AssetId = CAPI.ovr_AssetFileDownloadUpdate_GetAssetId(o);
            BytesTotal = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTotal(o);
            BytesTransferred = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTransferred(o);
            Completed = CAPI.ovr_AssetFileDownloadUpdate_GetCompleted(o);
        }
    }
}
