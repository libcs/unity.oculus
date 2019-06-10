namespace Oculus.Platform.Models
{
    using System;

    public class MatchmakingEnqueueResultAndRoom
    {
        public readonly MatchmakingEnqueueResult MatchmakingEnqueueResult;
        public readonly Room Room;

        public MatchmakingEnqueueResultAndRoom(IntPtr o)
        {
            MatchmakingEnqueueResult = new MatchmakingEnqueueResult(CAPI.ovr_MatchmakingEnqueueResultAndRoom_GetMatchmakingEnqueueResult(o));
            Room = new Room(CAPI.ovr_MatchmakingEnqueueResultAndRoom_GetRoom(o));
        }
    }
}