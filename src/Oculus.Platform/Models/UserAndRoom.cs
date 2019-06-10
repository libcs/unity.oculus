#pragma warning disable 0618

namespace Oculus.Platform.Models
{
    using System;
    using System.Collections.Generic;

    public class UserAndRoom
    {
        // May be null. Check before using.
        public readonly Room RoomOptional;
        [Obsolete("Deprecated in favor of RoomOptional")]
        public readonly Room Room;
        public readonly User User;

        public UserAndRoom(IntPtr o)
        {
            {
                var pointer = CAPI.ovr_UserAndRoom_GetRoom(o);
                Room = new Room(pointer);
                RoomOptional = pointer == IntPtr.Zero ? null : Room;
            }
            User = new User(CAPI.ovr_UserAndRoom_GetUser(o));
        }
    }

    public class UserAndRoomList : DeserializableList<UserAndRoom>
    {
        public UserAndRoomList(IntPtr a)
        {
            var count = (int)CAPI.ovr_UserAndRoomArray_GetSize(a);
            _Data = new List<UserAndRoom>(count);
            for (var i = 0; i < count; i++)
                _Data.Add(new UserAndRoom(CAPI.ovr_UserAndRoomArray_GetElement(a, (UIntPtr)i)));
            _NextUrl = CAPI.ovr_UserAndRoomArray_GetNextUrl(a);
        }
    }
}
