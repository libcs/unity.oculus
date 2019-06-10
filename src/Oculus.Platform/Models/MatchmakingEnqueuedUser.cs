#pragma warning disable 0618

namespace Oculus.Platform.Models
{
    using System;
    using System.Collections.Generic;

    public class MatchmakingEnqueuedUser
    {
        public readonly Dictionary<string, string> CustomData;
        // May be null. Check before using.
        public readonly User UserOptional;
        [Obsolete("Deprecated in favor of UserOptional")]
        public readonly User User;

        public MatchmakingEnqueuedUser(IntPtr o)
        {
            CustomData = CAPI.DataStoreFromNative(CAPI.ovr_MatchmakingEnqueuedUser_GetCustomData(o));
            {
                var pointer = CAPI.ovr_MatchmakingEnqueuedUser_GetUser(o);
                User = new User(pointer);
                UserOptional = pointer == IntPtr.Zero ? null : User;
            }
        }
    }

    public class MatchmakingEnqueuedUserList : DeserializableList<MatchmakingEnqueuedUser>
    {
        public MatchmakingEnqueuedUserList(IntPtr a)
        {
            var count = (int)CAPI.ovr_MatchmakingEnqueuedUserArray_GetSize(a);
            _Data = new List<MatchmakingEnqueuedUser>(count);
            for (var i = 0; i < count; i++)
                _Data.Add(new MatchmakingEnqueuedUser(CAPI.ovr_MatchmakingEnqueuedUserArray_GetElement(a, (UIntPtr)i)));
        }
    }
}
