#pragma warning disable 0618

namespace Oculus.Platform.Models
{
    using System;

    public class MatchmakingEnqueueResult
    {
        // May be null. Check before using.
        public readonly MatchmakingAdminSnapshot AdminSnapshotOptional;
        [Obsolete("Deprecated in favor of AdminSnapshotOptional")]
        public readonly MatchmakingAdminSnapshot AdminSnapshot;
        public readonly uint AverageWait;
        public readonly uint MatchesInLastHourCount;
        public readonly uint MaxExpectedWait;
        public readonly string Pool;
        public readonly uint RecentMatchPercentage;
        public readonly string RequestHash;

        public MatchmakingEnqueueResult(IntPtr o)
        {
            {
                var pointer = CAPI.ovr_MatchmakingEnqueueResult_GetAdminSnapshot(o);
                AdminSnapshot = new MatchmakingAdminSnapshot(pointer);
                AdminSnapshotOptional = pointer == IntPtr.Zero ? null : AdminSnapshot;
            }
            AverageWait = CAPI.ovr_MatchmakingEnqueueResult_GetAverageWait(o);
            MatchesInLastHourCount = CAPI.ovr_MatchmakingEnqueueResult_GetMatchesInLastHourCount(o);
            MaxExpectedWait = CAPI.ovr_MatchmakingEnqueueResult_GetMaxExpectedWait(o);
            Pool = CAPI.ovr_MatchmakingEnqueueResult_GetPool(o);
            RecentMatchPercentage = CAPI.ovr_MatchmakingEnqueueResult_GetRecentMatchPercentage(o);
            RequestHash = CAPI.ovr_MatchmakingEnqueueResult_GetRequestHash(o);
        }
    }
}