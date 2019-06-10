#pragma warning disable 0618

namespace Oculus.Platform.Models
{
    using System;

    public class Party
    {
        public readonly ulong ID;
        // May be null. Check before using.
        public readonly UserList InvitedUsersOptional;
        [Obsolete("Deprecated in favor of InvitedUsersOptional")]
        public readonly UserList InvitedUsers;
        // May be null. Check before using.
        public readonly User LeaderOptional;
        [Obsolete("Deprecated in favor of LeaderOptional")]
        public readonly User Leader;
        // May be null. Check before using.
        public readonly Room RoomOptional;
        [Obsolete("Deprecated in favor of RoomOptional")]
        public readonly Room Room;
        // May be null. Check before using.
        public readonly UserList UsersOptional;
        [Obsolete("Deprecated in favor of UsersOptional")]
        public readonly UserList Users;

        public Party(IntPtr o)
        {
            ID = CAPI.ovr_Party_GetID(o);
            {
                var pointer = CAPI.ovr_Party_GetInvitedUsers(o);
                InvitedUsers = new UserList(pointer);
                InvitedUsersOptional = pointer == IntPtr.Zero ? null : InvitedUsers;
            }
            {
                var pointer = CAPI.ovr_Party_GetLeader(o);
                Leader = new User(pointer);
                LeaderOptional = pointer == IntPtr.Zero ? null : Leader;
            }
            {
                var pointer = CAPI.ovr_Party_GetRoom(o);
                Room = new Room(pointer);
                RoomOptional = pointer == IntPtr.Zero ? null : Room;
            }
            {
                var pointer = CAPI.ovr_Party_GetUsers(o);
                Users = new UserList(pointer);
                UsersOptional = pointer == IntPtr.Zero ? null : Users;
            }
        }
    }

}
