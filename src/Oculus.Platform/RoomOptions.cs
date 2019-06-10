namespace Oculus.Platform
{
    using System;

    public class RoomOptions
    {
        readonly IntPtr Handle;

        public RoomOptions() => Handle = CAPI.ovr_RoomOptions_Create();
        ~RoomOptions() { CAPI.ovr_RoomOptions_Destroy(Handle); }
        public void SetDataStore(string key, string value) => CAPI.ovr_RoomOptions_SetDataStoreString(Handle, key, value);
        public void ClearDataStore() => CAPI.ovr_RoomOptions_ClearDataStore(Handle);
        public void SetExcludeRecentlyMet(bool value) => CAPI.ovr_RoomOptions_SetExcludeRecentlyMet(Handle, value);
        public void SetMaxUserResults(uint value) => CAPI.ovr_RoomOptions_SetMaxUserResults(Handle, value);
        public void SetOrdering(UserOrdering value) => CAPI.ovr_RoomOptions_SetOrdering(Handle, value);
        public void SetRecentlyMetTimeWindow(TimeWindow value) => CAPI.ovr_RoomOptions_SetRecentlyMetTimeWindow(Handle, value);
        public void SetRoomId(ulong value) => CAPI.ovr_RoomOptions_SetRoomId(Handle, value);
        public void SetTurnOffUpdates(bool value) => CAPI.ovr_RoomOptions_SetTurnOffUpdates(Handle, value);
        // For passing to native C
        public static explicit operator IntPtr(RoomOptions options) => options != null ? options.Handle : IntPtr.Zero;
    }
}
