namespace Oculus.Platform
{
    using Oculus.Platform.Models;
    using System;
    using UnityEngine;

    public abstract class Message<T> : Message
    {
        public new delegate void Callback(Message<T> message);
        public Message(IntPtr c_message) : base(c_message)
        {
            if (!IsError)
                Data = GetDataFromMessage(c_message);
        }

        public T Data { get; }
        protected abstract T GetDataFromMessage(IntPtr c_message);
    }

    public class Message
    {
        public delegate void Callback(Message message);
        public Message(IntPtr c_message)
        {
            Type = CAPI.ovr_Message_GetType(c_message);
            var isError = CAPI.ovr_Message_IsError(c_message);
            RequestID = CAPI.ovr_Message_GetRequestID(c_message);
            if (isError)
            {
                var errorHandle = CAPI.ovr_Message_GetError(c_message);
                error = new Error(
                  CAPI.ovr_Error_GetCode(errorHandle),
                  CAPI.ovr_Error_GetMessage(errorHandle),
                  CAPI.ovr_Error_GetHttpCode(errorHandle));
            }
            else if (Core.LogMessages)
            {
                var message = CAPI.ovr_Message_GetString(c_message);
                Debug.Log(message ?? string.Format("null message string {0}", c_message));
            }
        }
        ~Message() { }

        // Keep this enum in sync with ovrMessageType in OVR_Platform.h
        public enum MessageType : uint
        {
            Unknown,
            Achievements_AddCount = 0x03E76231,
            Achievements_AddFields = 0x14AA2129,
            Achievements_GetAllDefinitions = 0x03D3458D,
            Achievements_GetAllProgress = 0x4F9FDE1D,
            Achievements_GetDefinitionsByName = 0x629101BC,
            Achievements_GetNextAchievementDefinitionArrayPage = 0x2A7DD255,
            Achievements_GetNextAchievementProgressArrayPage = 0x2F42E727,
            Achievements_GetProgressByName = 0x152663B1,
            Achievements_Unlock = 0x593CCBDD,
            ApplicationLifecycle_GetRegisteredPIDs = 0x04E5CF62,
            ApplicationLifecycle_GetSessionKey = 0x3AAF591D,
            ApplicationLifecycle_RegisterSessionKey = 0x4DB6AFF8,
            Application_GetVersion = 0x68670A0E,
            Application_LaunchOtherApp = 0x54E2D1F8,
            AssetFile_Delete = 0x6D5D7886,
            AssetFile_DeleteById = 0x5AE8CD52,
            AssetFile_DeleteByName = 0x420AC1CF,
            AssetFile_Download = 0x11449FC5,
            AssetFile_DownloadById = 0x2D008992,
            AssetFile_DownloadByName = 0x6336CEFA,
            AssetFile_DownloadCancel = 0x080AD3C7,
            AssetFile_DownloadCancelById = 0x51659514,
            AssetFile_DownloadCancelByName = 0x446AECFA,
            AssetFile_GetList = 0x4AFC6F74,
            AssetFile_Status = 0x02D32F60,
            AssetFile_StatusById = 0x5D955D38,
            AssetFile_StatusByName = 0x41CFDA50,
            CloudStorage_Delete = 0x28DA456D,
            CloudStorage_GetNextCloudStorageMetadataArrayPage = 0x5C07A2EF,
            CloudStorage_Load = 0x40846B41,
            CloudStorage_LoadBucketMetadata = 0x7327A50D,
            CloudStorage_LoadConflictMetadata = 0x445A52F2,
            CloudStorage_LoadHandle = 0x326ADA36,
            CloudStorage_LoadMetadata = 0x03E6A292,
            CloudStorage_ResolveKeepLocal = 0x30588D05,
            CloudStorage_ResolveKeepRemote = 0x7525A306,
            CloudStorage_Save = 0x4BBB5C2E,
            Entitlement_GetIsViewerEntitled = 0x186B58B1,
            IAP_ConsumePurchase = 0x1FBB72D9,
            IAP_GetNextProductArrayPage = 0x1BD94AAF,
            IAP_GetNextPurchaseArrayPage = 0x47570A95,
            IAP_GetProductsBySKU = 0x7E9ACAF5,
            IAP_GetViewerPurchases = 0x3A0F8419,
            IAP_LaunchCheckoutFlow = 0x3F9B0D0D,
            LanguagePack_GetCurrent = 0x1F90F0D5,
            LanguagePack_SetCurrent = 0x5B4FBBE0,
            Leaderboard_GetEntries = 0x5DB3474C,
            Leaderboard_GetEntriesAfterRank = 0x18378BEF,
            Leaderboard_GetNextEntries = 0x4E207CD9,
            Leaderboard_GetPreviousEntries = 0x4901DAC0,
            Leaderboard_WriteEntry = 0x117FC8FE,
            Livestreaming_GetStatus = 0x489A6995,
            Livestreaming_PauseStream = 0x369C7683,
            Livestreaming_ResumeStream = 0x22526D8F,
            Matchmaking_Browse = 0x1E6532C8,
            Matchmaking_Browse2 = 0x66429E5B,
            Matchmaking_Cancel = 0x206849AF,
            Matchmaking_Cancel2 = 0x10FE8DD4,
            Matchmaking_CreateAndEnqueueRoom = 0x604C5DC8,
            Matchmaking_CreateAndEnqueueRoom2 = 0x295BEADB,
            Matchmaking_CreateRoom = 0x033B132A,
            Matchmaking_CreateRoom2 = 0x496DA384,
            Matchmaking_Enqueue = 0x40C16C71,
            Matchmaking_Enqueue2 = 0x121212B5,
            Matchmaking_EnqueueRoom = 0x708A4064,
            Matchmaking_EnqueueRoom2 = 0x5528DBA4,
            Matchmaking_GetAdminSnapshot = 0x3C215F94,
            Matchmaking_GetStats = 0x42FC9438,
            Matchmaking_JoinRoom = 0x4D32D7FD,
            Matchmaking_ReportResultInsecure = 0x1A36D18D,
            Matchmaking_StartMatch = 0x44D40945,
            Media_ShareToFacebook = 0x00E38AEF,
            Notification_GetNextRoomInviteNotificationArrayPage = 0x0621FB77,
            Notification_GetRoomInvites = 0x6F916B92,
            Notification_MarkAsRead = 0x717259E3,
            Party_GetCurrent = 0x47933760,
            Room_CreateAndJoinPrivate = 0x75D6E377,
            Room_CreateAndJoinPrivate2 = 0x5A3A6243,
            Room_Get = 0x659A8FB8,
            Room_GetCurrent = 0x09A6A504,
            Room_GetCurrentForUser = 0x0E0017E5,
            Room_GetInvitableUsers = 0x1E325792,
            Room_GetInvitableUsers2 = 0x4F53E8B0,
            Room_GetModeratedRooms = 0x0983FD77,
            Room_GetNextRoomArrayPage = 0x4E8379C6,
            Room_InviteUser = 0x4129EC13,
            Room_Join = 0x16CA8F09,
            Room_Join2 = 0x4DAB1C42,
            Room_KickUser = 0x49835736,
            Room_LaunchInvitableUserFlow = 0x323FE273,
            Room_Leave = 0x72382475,
            Room_SetDescription = 0x3044852F,
            Room_UpdateDataStore = 0x026E4028,
            Room_UpdateMembershipLockStatus = 0x370BB7AC,
            Room_UpdateOwner = 0x32B63D1D,
            Room_UpdatePrivateRoomJoinPolicy = 0x1141029B,
            User_Get = 0x6BCF9E47,
            User_GetAccessToken = 0x06A85ABE,
            User_GetLoggedInUser = 0x436F345D,
            User_GetLoggedInUserFriends = 0x587C2A8D,
            User_GetLoggedInUserFriendsAndRooms = 0x5E870B87,
            User_GetLoggedInUserRecentlyMetUsersAndRooms = 0x295FBA30,
            User_GetNextUserAndRoomArrayPage = 0x7FBDD2DF,
            User_GetNextUserArrayPage = 0x267CF743,
            User_GetOrgScopedID = 0x18F0B01B,
            User_GetSdkAccounts = 0x67526A83,
            User_GetUserProof = 0x22810483,
            User_LaunchFriendRequestFlow = 0x0904B598,
            User_LaunchProfile = 0x0A397297,
            Voip_SetSystemVoipSuppressed = 0x453FC9AA,

            /// Sent when a launch intent is received (for both cold and warm starts). The
            /// payload is the type of the intent. ApplicationLifecycle.GetLaunchDetails()
            /// should be called to get the other details.
            Notification_ApplicationLifecycle_LaunchIntentChanged = 0x04B34CA3,

            /// Sent to indicate download progress for asset files.
            Notification_AssetFile_DownloadUpdate = 0x2FDD0CCD,

            /// Result of a leader picking an application for CAL launch.
            Notification_Cal_FinalizeApplication = 0x750C5099,

            /// Application that the group leader has proposed for a CAL launch.
            Notification_Cal_ProposeApplication = 0x2E7451F5,

            /// Sent to indicate that more data has been read or an error occured.
            Notification_HTTP_Transfer = 0x7DD46E2F,

            /// Indicates that the livestreaming session has been updated. You can use this
            /// information to throttle your game performance or increase CPU/GPU
            /// performance. Use Message.GetLivestreamingStatus() to extract the updated
            /// livestreaming status.
            Notification_Livestreaming_StatusChange = 0x2247596E,

            /// Indicates that a match has been found, for example after calling
            /// Matchmaking.Enqueue(). Use Message.GetRoom() to extract the matchmaking
            /// room.
            Notification_Matchmaking_MatchFound = 0x0BC3FCD7,

            /// Indicates that a connection has been established or there's been an error.
            /// Use NetworkingPeer.GetState() to get the result; as above,
            /// NetworkingPeer.GetID() returns the ID of the peer this message is for.
            Notification_Networking_ConnectionStateChange = 0x5E02D49A,

            /// Indicates that another user is attempting to establish a P2P connection
            /// with us. Use NetworkingPeer.GetID() to extract the ID of the peer.
            Notification_Networking_PeerConnectRequest = 0x4D31E2CF,

            /// Generated in response to Net.Ping(). Either contains ping time in
            /// microseconds or indicates that there was a timeout.
            Notification_Networking_PingResult = 0x51153012,

            /// Indicates that party has been updated
            Notification_Party_PartyUpdate = 0x1D118AB2,

            /// Indicates that the user has accepted an invitation, for example in Oculus
            /// Home. Use Message.GetString() to extract the ID of the room that the user
            /// has been inivted to as a string. Then call ovrID_FromString() to parse it
            /// into an ovrID.
            ///
            /// Note that you must call Room.Join() if you want to actually join the room.
            Notification_Room_InviteAccepted = 0x6D1071B1,

            /// Handle this to notify the user when they've received an invitation to join
            /// a room in your game. You can use this in lieu of, or in addition to,
            /// polling for room invitations via Notification.GetRoomInviteNotifications().
            Notification_Room_InviteReceived = 0x6A499D54,

            /// Indicates that the current room has been updated. Use Message.GetRoom() to
            /// extract the updated room.
            Notification_Room_RoomUpdate = 0x60EC3C2F,

            /// Sent when another user is attempting to establish a VoIP connection. Use
            /// Message.GetNetworkingPeer() to extract information about the user, and
            /// Voip.Accept() to accept the connection.
            Notification_Voip_ConnectRequest = 0x36243816,

            /// Sent to indicate that the state of the VoIP connection changed. Use
            /// Message.GetNetworkingPeer() and NetworkingPeer.GetState() to extract the
            /// current state.
            Notification_Voip_StateChange = 0x34EFA660,

            /// Sent to indicate that some part of the overall state of SystemVoip has
            /// changed. Use Message.GetSystemVoipState() and the properties of
            /// SystemVoipState to extract the state that triggered the notification.
            ///
            /// Note that the state may have changed further since the notification was
            /// generated, and that you may call the `GetSystemVoip...()` family of
            /// functions at any time to get the current state directly.
            Notification_Voip_SystemVoipState = 0x58D254A5,

            Platform_InitializeWithAccessToken = 0x35692F2B,
            Platform_InitializeStandaloneOculus = 0x51F8CE0C,
            Platform_InitializeAndroidAsynchronous = 0x1AD307B4,
            Platform_InitializeWindowsAsynchronous = 0x6DA7BA8F,
        };

        public MessageType Type { get; }
        public bool IsError => error != null;
        public ulong RequestID { get; }
        Error error;

        public virtual Error GetError() => error;
        public virtual PingResult GetPingResult() => null;
        public virtual NetworkingPeer GetNetworkingPeer() => null;
        public virtual HttpTransferUpdate GetHttpTransferUpdate() => null;

        public virtual PlatformInitialize GetPlatformInitialize() => null;

        public virtual AbuseReportRecording GetAbuseReportRecording() => null;
        public virtual AchievementDefinitionList GetAchievementDefinitions() => null;
        public virtual AchievementProgressList GetAchievementProgressList() => null;
        public virtual AchievementUpdate GetAchievementUpdate() => null;
        public virtual ApplicationVersion GetApplicationVersion() => null;
        public virtual AssetDetails GetAssetDetails() => null;
        public virtual AssetDetailsList GetAssetDetailsList() => null;
        public virtual AssetFileDeleteResult GetAssetFileDeleteResult() => null;
        public virtual AssetFileDownloadCancelResult GetAssetFileDownloadCancelResult() => null;
        public virtual AssetFileDownloadResult GetAssetFileDownloadResult() => null;
        public virtual AssetFileDownloadUpdate GetAssetFileDownloadUpdate() => null;
        public virtual CalApplicationFinalized GetCalApplicationFinalized() => null;
        public virtual CalApplicationProposed GetCalApplicationProposed() => null;
        public virtual CalApplicationSuggestionList GetCalApplicationSuggestionList() => null;
        public virtual CloudStorageConflictMetadata GetCloudStorageConflictMetadata() => null;
        public virtual CloudStorageData GetCloudStorageData() => null;
        public virtual CloudStorageMetadata GetCloudStorageMetadata() => null;
        public virtual CloudStorageMetadataList GetCloudStorageMetadataList() => null;
        public virtual CloudStorageUpdateResponse GetCloudStorageUpdateResponse() => null;
        public virtual InstalledApplicationList GetInstalledApplicationList() => null;
        public virtual LaunchBlockFlowResult GetLaunchBlockFlowResult() => null;
        public virtual LaunchFriendRequestFlowResult GetLaunchFriendRequestFlowResult() => null;
        public virtual LaunchReportFlowResult GetLaunchReportFlowResult() => null;
        public virtual LaunchUnblockFlowResult GetLaunchUnblockFlowResult() => null;
        public virtual bool GetLeaderboardDidUpdate() => false;
        public virtual LeaderboardEntryList GetLeaderboardEntryList() => null;
        public virtual LinkedAccountList GetLinkedAccountList() => null;
        public virtual LivestreamingApplicationStatus GetLivestreamingApplicationStatus() => null;
        public virtual LivestreamingStartResult GetLivestreamingStartResult() => null;
        public virtual LivestreamingStatus GetLivestreamingStatus() => null;
        public virtual LivestreamingVideoStats GetLivestreamingVideoStats() => null;
        public virtual MatchmakingAdminSnapshot GetMatchmakingAdminSnapshot() => null;
        public virtual MatchmakingBrowseResult GetMatchmakingBrowseResult() => null;
        public virtual MatchmakingEnqueueResult GetMatchmakingEnqueueResult() => null;
        public virtual MatchmakingEnqueueResultAndRoom GetMatchmakingEnqueueResultAndRoom() => null;
        public virtual MatchmakingStats GetMatchmakingStats() => null;
        public virtual OrgScopedID GetOrgScopedID() => null;
        public virtual Party GetParty() => null;
        public virtual PartyID GetPartyID() => null;
        public virtual PartyUpdateNotification GetPartyUpdateNotification() => null;
        public virtual PidList GetPidList() => null;
        public virtual ProductList GetProductList() => null;
        public virtual Purchase GetPurchase() => null;
        public virtual PurchaseList GetPurchaseList() => null;
        public virtual Room GetRoom() => null;
        public virtual RoomInviteNotification GetRoomInviteNotification() => null;
        public virtual RoomInviteNotificationList GetRoomInviteNotificationList() => null;
        public virtual RoomList GetRoomList() => null;
        public virtual SdkAccountList GetSdkAccountList() => null;
        public virtual ShareMediaResult GetShareMediaResult() => null;
        public virtual string GetString() => null;
        public virtual SystemPermission GetSystemPermission() => null;
        public virtual SystemVoipState GetSystemVoipState() => null;
        public virtual User GetUser() => null;
        public virtual UserAndRoomList GetUserAndRoomList() => null;
        public virtual UserList GetUserList() => null;
        public virtual UserProof GetUserProof() => null;
        public virtual UserReportID GetUserReportID() => null;

        internal static Message ParseMessageHandle(IntPtr messageHandle)
        {
            if (messageHandle.ToInt64() == 0)
                return null;
            Message message = null;
            var message_type = CAPI.ovr_Message_GetType(messageHandle);
            switch (message_type)
            {
                // OVR_MESSAGE_TYPE_START
                case MessageType.Achievements_GetAllDefinitions:
                case MessageType.Achievements_GetDefinitionsByName:
                case MessageType.Achievements_GetNextAchievementDefinitionArrayPage: message = new MessageWithAchievementDefinitions(messageHandle); break;
                case MessageType.Achievements_GetAllProgress:
                case MessageType.Achievements_GetNextAchievementProgressArrayPage:
                case MessageType.Achievements_GetProgressByName: message = new MessageWithAchievementProgressList(messageHandle); break;
                case MessageType.Achievements_AddCount:
                case MessageType.Achievements_AddFields:
                case MessageType.Achievements_Unlock: message = new MessageWithAchievementUpdate(messageHandle); break;
                case MessageType.Application_GetVersion: message = new MessageWithApplicationVersion(messageHandle); break;
                case MessageType.AssetFile_Status:
                case MessageType.AssetFile_StatusById:
                case MessageType.AssetFile_StatusByName:
                case MessageType.LanguagePack_GetCurrent: message = new MessageWithAssetDetails(messageHandle); break;
                case MessageType.AssetFile_GetList: message = new MessageWithAssetDetailsList(messageHandle); break;
                case MessageType.AssetFile_Delete:
                case MessageType.AssetFile_DeleteById:
                case MessageType.AssetFile_DeleteByName: message = new MessageWithAssetFileDeleteResult(messageHandle); break;
                case MessageType.AssetFile_DownloadCancel:
                case MessageType.AssetFile_DownloadCancelById:
                case MessageType.AssetFile_DownloadCancelByName: message = new MessageWithAssetFileDownloadCancelResult(messageHandle); break;
                case MessageType.AssetFile_Download:
                case MessageType.AssetFile_DownloadById:
                case MessageType.AssetFile_DownloadByName:
                case MessageType.LanguagePack_SetCurrent: message = new MessageWithAssetFileDownloadResult(messageHandle); break;
                case MessageType.Notification_AssetFile_DownloadUpdate: message = new MessageWithAssetFileDownloadUpdate(messageHandle); break;
                case MessageType.Notification_Cal_FinalizeApplication: message = new MessageWithCalApplicationFinalized(messageHandle); break;
                case MessageType.Notification_Cal_ProposeApplication: message = new MessageWithCalApplicationProposed(messageHandle); break;
                case MessageType.CloudStorage_LoadConflictMetadata: message = new MessageWithCloudStorageConflictMetadata(messageHandle); break;
                case MessageType.CloudStorage_Load:
                case MessageType.CloudStorage_LoadHandle: message = new MessageWithCloudStorageData(messageHandle); break;
                case MessageType.CloudStorage_LoadMetadata: message = new MessageWithCloudStorageMetadataUnderLocal(messageHandle); break;
                case MessageType.CloudStorage_GetNextCloudStorageMetadataArrayPage:
                case MessageType.CloudStorage_LoadBucketMetadata: message = new MessageWithCloudStorageMetadataList(messageHandle); break;
                case MessageType.CloudStorage_Delete:
                case MessageType.CloudStorage_ResolveKeepLocal:
                case MessageType.CloudStorage_ResolveKeepRemote:
                case MessageType.CloudStorage_Save: message = new MessageWithCloudStorageUpdateResponse(messageHandle); break;
                case MessageType.ApplicationLifecycle_RegisterSessionKey:
                case MessageType.Entitlement_GetIsViewerEntitled:
                case MessageType.IAP_ConsumePurchase:
                case MessageType.Matchmaking_Cancel:
                case MessageType.Matchmaking_Cancel2:
                case MessageType.Matchmaking_ReportResultInsecure:
                case MessageType.Matchmaking_StartMatch:
                case MessageType.Notification_MarkAsRead:
                case MessageType.Room_LaunchInvitableUserFlow:
                case MessageType.Room_UpdateOwner:
                case MessageType.User_LaunchProfile: message = new Message(messageHandle); break;
                case MessageType.User_LaunchFriendRequestFlow: message = new MessageWithLaunchFriendRequestFlowResult(messageHandle); break;
                case MessageType.Leaderboard_GetEntries:
                case MessageType.Leaderboard_GetEntriesAfterRank:
                case MessageType.Leaderboard_GetNextEntries:
                case MessageType.Leaderboard_GetPreviousEntries: message = new MessageWithLeaderboardEntryList(messageHandle); break;
                case MessageType.Leaderboard_WriteEntry: message = new MessageWithLeaderboardDidUpdate(messageHandle); break;
                case MessageType.Livestreaming_GetStatus:
                case MessageType.Livestreaming_PauseStream:
                case MessageType.Livestreaming_ResumeStream:
                case MessageType.Notification_Livestreaming_StatusChange: message = new MessageWithLivestreamingStatus(messageHandle); break;
                case MessageType.Matchmaking_GetAdminSnapshot: message = new MessageWithMatchmakingAdminSnapshot(messageHandle); break;
                case MessageType.Matchmaking_Browse:
                case MessageType.Matchmaking_Browse2: message = new MessageWithMatchmakingBrowseResult(messageHandle); break;
                case MessageType.Matchmaking_Enqueue:
                case MessageType.Matchmaking_Enqueue2:
                case MessageType.Matchmaking_EnqueueRoom:
                case MessageType.Matchmaking_EnqueueRoom2: message = new MessageWithMatchmakingEnqueueResult(messageHandle); break;
                case MessageType.Matchmaking_CreateAndEnqueueRoom:
                case MessageType.Matchmaking_CreateAndEnqueueRoom2: message = new MessageWithMatchmakingEnqueueResultAndRoom(messageHandle); break;
                case MessageType.Matchmaking_GetStats: message = new MessageWithMatchmakingStatsUnderMatchmakingStats(messageHandle); break;
                case MessageType.User_GetOrgScopedID: message = new MessageWithOrgScopedID(messageHandle); break;
                case MessageType.Party_GetCurrent: message = new MessageWithPartyUnderCurrentParty(messageHandle); break;
                case MessageType.Notification_Party_PartyUpdate: message = new MessageWithPartyUpdateNotification(messageHandle); break;
                case MessageType.ApplicationLifecycle_GetRegisteredPIDs: message = new MessageWithPidList(messageHandle); break;
                case MessageType.IAP_GetNextProductArrayPage:
                case MessageType.IAP_GetProductsBySKU: message = new MessageWithProductList(messageHandle); break;
                case MessageType.IAP_LaunchCheckoutFlow: message = new MessageWithPurchase(messageHandle); break;
                case MessageType.IAP_GetNextPurchaseArrayPage:
                case MessageType.IAP_GetViewerPurchases: message = new MessageWithPurchaseList(messageHandle); break;
                case MessageType.Room_Get: message = new MessageWithRoom(messageHandle); break;
                case MessageType.Room_GetCurrent:
                case MessageType.Room_GetCurrentForUser: message = new MessageWithRoomUnderCurrentRoom(messageHandle); break;
                case MessageType.Matchmaking_CreateRoom:
                case MessageType.Matchmaking_CreateRoom2:
                case MessageType.Matchmaking_JoinRoom:
                case MessageType.Notification_Room_RoomUpdate:
                case MessageType.Room_CreateAndJoinPrivate:
                case MessageType.Room_CreateAndJoinPrivate2:
                case MessageType.Room_InviteUser:
                case MessageType.Room_Join:
                case MessageType.Room_Join2:
                case MessageType.Room_KickUser:
                case MessageType.Room_Leave:
                case MessageType.Room_SetDescription:
                case MessageType.Room_UpdateDataStore:
                case MessageType.Room_UpdateMembershipLockStatus:
                case MessageType.Room_UpdatePrivateRoomJoinPolicy: message = new MessageWithRoomUnderViewerRoom(messageHandle); break;
                case MessageType.Room_GetModeratedRooms:
                case MessageType.Room_GetNextRoomArrayPage: message = new MessageWithRoomList(messageHandle); break;
                case MessageType.Notification_Room_InviteReceived: message = new MessageWithRoomInviteNotification(messageHandle); break;
                case MessageType.Notification_GetNextRoomInviteNotificationArrayPage:
                case MessageType.Notification_GetRoomInvites: message = new MessageWithRoomInviteNotificationList(messageHandle); break;
                case MessageType.User_GetSdkAccounts: message = new MessageWithSdkAccountList(messageHandle); break;
                case MessageType.Media_ShareToFacebook: message = new MessageWithShareMediaResult(messageHandle); break;
                case MessageType.ApplicationLifecycle_GetSessionKey:
                case MessageType.Application_LaunchOtherApp:
                case MessageType.Notification_ApplicationLifecycle_LaunchIntentChanged:
                case MessageType.Notification_Room_InviteAccepted:
                case MessageType.User_GetAccessToken: message = new MessageWithString(messageHandle); break;
                case MessageType.Voip_SetSystemVoipSuppressed: message = new MessageWithSystemVoipState(messageHandle); break;
                case MessageType.User_Get:
                case MessageType.User_GetLoggedInUser: message = new MessageWithUser(messageHandle); break;
                case MessageType.User_GetLoggedInUserFriendsAndRooms:
                case MessageType.User_GetLoggedInUserRecentlyMetUsersAndRooms:
                case MessageType.User_GetNextUserAndRoomArrayPage: message = new MessageWithUserAndRoomList(messageHandle); break;
                case MessageType.Room_GetInvitableUsers:
                case MessageType.Room_GetInvitableUsers2:
                case MessageType.User_GetLoggedInUserFriends:
                case MessageType.User_GetNextUserArrayPage: message = new MessageWithUserList(messageHandle); break;
                case MessageType.User_GetUserProof: message = new MessageWithUserProof(messageHandle); break;
                case MessageType.Notification_Networking_ConnectionStateChange:
                case MessageType.Notification_Networking_PeerConnectRequest: message = new MessageWithNetworkingPeer(messageHandle); break;
                case MessageType.Notification_Networking_PingResult: message = new MessageWithPingResult(messageHandle); break;
                case MessageType.Notification_Matchmaking_MatchFound: message = new MessageWithMatchmakingNotification(messageHandle); break;
                case MessageType.Notification_Voip_ConnectRequest:
                case MessageType.Notification_Voip_StateChange: message = new MessageWithNetworkingPeer(messageHandle); break;
                case MessageType.Notification_Voip_SystemVoipState: message = new MessageWithSystemVoipState(messageHandle); break;
                case MessageType.Notification_HTTP_Transfer: message = new MessageWithHttpTransferUpdate(messageHandle); break;
                case MessageType.Platform_InitializeWithAccessToken:
                case MessageType.Platform_InitializeStandaloneOculus:
                case MessageType.Platform_InitializeAndroidAsynchronous:
                case MessageType.Platform_InitializeWindowsAsynchronous: message = new MessageWithPlatformInitialize(messageHandle); break;
                default:
                    message = PlatformInternal.ParseMessageHandle(messageHandle, message_type);
                    if (message == null)
                        Debug.LogError(string.Format("Unrecognized message type {0}\n", message_type));
                    break;
                    // OVR_MESSAGE_TYPE_END
            }
            return message;
        }

        public static Message PopMessage()
        {
            if (!Core.IsInitialized())
                return null;
            var messageHandle = CAPI.ovr_PopMessage();
            var message = ParseMessageHandle(messageHandle);
            CAPI.ovr_FreeMessage(messageHandle);
            return message;
        }

        internal delegate Message ExtraMessageTypesHandler(IntPtr messageHandle, MessageType message_type);
        internal static ExtraMessageTypesHandler HandleExtraMessageTypes { set; private get; }
    }

    public class MessageWithAbuseReportRecording : Message<AbuseReportRecording>
    {
        public MessageWithAbuseReportRecording(IntPtr c_message) : base(c_message) { }
        public override AbuseReportRecording GetAbuseReportRecording() => Data;
        protected override AbuseReportRecording GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAbuseReportRecording(msg);
            return new AbuseReportRecording(obj);
        }
    }
    public class MessageWithAchievementDefinitions : Message<AchievementDefinitionList>
    {
        public MessageWithAchievementDefinitions(IntPtr c_message) : base(c_message) { }
        public override AchievementDefinitionList GetAchievementDefinitions() => Data;
        protected override AchievementDefinitionList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAchievementDefinitionArray(msg);
            return new AchievementDefinitionList(obj);
        }
    }
    public class MessageWithAchievementProgressList : Message<AchievementProgressList>
    {
        public MessageWithAchievementProgressList(IntPtr c_message) : base(c_message) { }
        public override AchievementProgressList GetAchievementProgressList() => Data;
        protected override AchievementProgressList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAchievementProgressArray(msg);
            return new AchievementProgressList(obj);
        }
    }
    public class MessageWithAchievementUpdate : Message<AchievementUpdate>
    {
        public MessageWithAchievementUpdate(IntPtr c_message) : base(c_message) { }
        public override AchievementUpdate GetAchievementUpdate() => Data;
        protected override AchievementUpdate GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAchievementUpdate(msg);
            return new AchievementUpdate(obj);
        }
    }
    public class MessageWithApplicationVersion : Message<ApplicationVersion>
    {
        public MessageWithApplicationVersion(IntPtr c_message) : base(c_message) { }
        public override ApplicationVersion GetApplicationVersion() => Data;
        protected override ApplicationVersion GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetApplicationVersion(msg);
            return new ApplicationVersion(obj);
        }
    }
    public class MessageWithAssetDetails : Message<AssetDetails>
    {
        public MessageWithAssetDetails(IntPtr c_message) : base(c_message) { }
        public override AssetDetails GetAssetDetails() => Data;
        protected override AssetDetails GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetDetails(msg);
            return new AssetDetails(obj);
        }
    }
    public class MessageWithAssetDetailsList : Message<AssetDetailsList>
    {
        public MessageWithAssetDetailsList(IntPtr c_message) : base(c_message) { }
        public override AssetDetailsList GetAssetDetailsList() => Data;
        protected override AssetDetailsList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetDetailsArray(msg);
            return new AssetDetailsList(obj);
        }
    }
    public class MessageWithAssetFileDeleteResult : Message<AssetFileDeleteResult>
    {
        public MessageWithAssetFileDeleteResult(IntPtr c_message) : base(c_message) { }
        public override AssetFileDeleteResult GetAssetFileDeleteResult() => Data;
        protected override AssetFileDeleteResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetFileDeleteResult(msg);
            return new AssetFileDeleteResult(obj);
        }
    }
    public class MessageWithAssetFileDownloadCancelResult : Message<AssetFileDownloadCancelResult>
    {
        public MessageWithAssetFileDownloadCancelResult(IntPtr c_message) : base(c_message) { }
        public override AssetFileDownloadCancelResult GetAssetFileDownloadCancelResult() => Data;
        protected override AssetFileDownloadCancelResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetFileDownloadCancelResult(msg);
            return new AssetFileDownloadCancelResult(obj);
        }
    }
    public class MessageWithAssetFileDownloadResult : Message<AssetFileDownloadResult>
    {
        public MessageWithAssetFileDownloadResult(IntPtr c_message) : base(c_message) { }
        public override AssetFileDownloadResult GetAssetFileDownloadResult() => Data;
        protected override AssetFileDownloadResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetFileDownloadResult(msg);
            return new AssetFileDownloadResult(obj);
        }
    }
    public class MessageWithAssetFileDownloadUpdate : Message<AssetFileDownloadUpdate>
    {
        public MessageWithAssetFileDownloadUpdate(IntPtr c_message) : base(c_message) { }
        public override AssetFileDownloadUpdate GetAssetFileDownloadUpdate() => Data;
        protected override AssetFileDownloadUpdate GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetAssetFileDownloadUpdate(msg);
            return new AssetFileDownloadUpdate(obj);
        }
    }
    public class MessageWithCalApplicationFinalized : Message<CalApplicationFinalized>
    {
        public MessageWithCalApplicationFinalized(IntPtr c_message) : base(c_message) { }
        public override CalApplicationFinalized GetCalApplicationFinalized() => Data;
        protected override CalApplicationFinalized GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCalApplicationFinalized(msg);
            return new CalApplicationFinalized(obj);
        }
    }
    public class MessageWithCalApplicationProposed : Message<CalApplicationProposed>
    {
        public MessageWithCalApplicationProposed(IntPtr c_message) : base(c_message) { }
        public override CalApplicationProposed GetCalApplicationProposed() => Data;
        protected override CalApplicationProposed GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCalApplicationProposed(msg);
            return new CalApplicationProposed(obj);
        }
    }
    public class MessageWithCalApplicationSuggestionList : Message<CalApplicationSuggestionList>
    {
        public MessageWithCalApplicationSuggestionList(IntPtr c_message) : base(c_message) { }
        public override CalApplicationSuggestionList GetCalApplicationSuggestionList() => Data;
        protected override CalApplicationSuggestionList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCalApplicationSuggestionArray(msg);
            return new CalApplicationSuggestionList(obj);
        }
    }
    public class MessageWithCloudStorageConflictMetadata : Message<CloudStorageConflictMetadata>
    {
        public MessageWithCloudStorageConflictMetadata(IntPtr c_message) : base(c_message) { }
        public override CloudStorageConflictMetadata GetCloudStorageConflictMetadata() => Data;
        protected override CloudStorageConflictMetadata GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCloudStorageConflictMetadata(msg);
            return new CloudStorageConflictMetadata(obj);
        }
    }
    public class MessageWithCloudStorageData : Message<CloudStorageData>
    {
        public MessageWithCloudStorageData(IntPtr c_message) : base(c_message) { }
        public override CloudStorageData GetCloudStorageData() => Data;
        protected override CloudStorageData GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCloudStorageData(msg);
            return new CloudStorageData(obj);
        }
    }
    public class MessageWithCloudStorageMetadataUnderLocal : Message<CloudStorageMetadata>
    {
        public MessageWithCloudStorageMetadataUnderLocal(IntPtr c_message) : base(c_message) { }
        public override CloudStorageMetadata GetCloudStorageMetadata() => Data;
        protected override CloudStorageMetadata GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCloudStorageMetadata(msg);
            return new CloudStorageMetadata(obj);
        }
    }
    public class MessageWithCloudStorageMetadataList : Message<CloudStorageMetadataList>
    {
        public MessageWithCloudStorageMetadataList(IntPtr c_message) : base(c_message) { }
        public override CloudStorageMetadataList GetCloudStorageMetadataList() => Data;
        protected override CloudStorageMetadataList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCloudStorageMetadataArray(msg);
            return new CloudStorageMetadataList(obj);
        }
    }
    public class MessageWithCloudStorageUpdateResponse : Message<CloudStorageUpdateResponse>
    {
        public MessageWithCloudStorageUpdateResponse(IntPtr c_message) : base(c_message) { }
        public override CloudStorageUpdateResponse GetCloudStorageUpdateResponse() => Data;
        protected override CloudStorageUpdateResponse GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetCloudStorageUpdateResponse(msg);
            return new CloudStorageUpdateResponse(obj);
        }
    }
    public class MessageWithInstalledApplicationList : Message<InstalledApplicationList>
    {
        public MessageWithInstalledApplicationList(IntPtr c_message) : base(c_message) { }
        public override InstalledApplicationList GetInstalledApplicationList() => Data;
        protected override InstalledApplicationList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetInstalledApplicationArray(msg);
            return new InstalledApplicationList(obj);
        }
    }
    public class MessageWithLaunchBlockFlowResult : Message<LaunchBlockFlowResult>
    {
        public MessageWithLaunchBlockFlowResult(IntPtr c_message) : base(c_message) { }
        public override LaunchBlockFlowResult GetLaunchBlockFlowResult() => Data;
        protected override LaunchBlockFlowResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLaunchBlockFlowResult(msg);
            return new LaunchBlockFlowResult(obj);
        }
    }
    public class MessageWithLaunchFriendRequestFlowResult : Message<LaunchFriendRequestFlowResult>
    {
        public MessageWithLaunchFriendRequestFlowResult(IntPtr c_message) : base(c_message) { }
        public override LaunchFriendRequestFlowResult GetLaunchFriendRequestFlowResult() => Data;
        protected override LaunchFriendRequestFlowResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLaunchFriendRequestFlowResult(msg);
            return new LaunchFriendRequestFlowResult(obj);
        }
    }
    public class MessageWithLaunchReportFlowResult : Message<LaunchReportFlowResult>
    {
        public MessageWithLaunchReportFlowResult(IntPtr c_message) : base(c_message) { }
        public override LaunchReportFlowResult GetLaunchReportFlowResult() => Data;
        protected override LaunchReportFlowResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLaunchReportFlowResult(msg);
            return new LaunchReportFlowResult(obj);
        }
    }
    public class MessageWithLaunchUnblockFlowResult : Message<LaunchUnblockFlowResult>
    {
        public MessageWithLaunchUnblockFlowResult(IntPtr c_message) : base(c_message) { }
        public override LaunchUnblockFlowResult GetLaunchUnblockFlowResult() => Data;
        protected override LaunchUnblockFlowResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLaunchUnblockFlowResult(msg);
            return new LaunchUnblockFlowResult(obj);
        }
    }
    public class MessageWithLeaderboardEntryList : Message<LeaderboardEntryList>
    {
        public MessageWithLeaderboardEntryList(IntPtr c_message) : base(c_message) { }
        public override LeaderboardEntryList GetLeaderboardEntryList() => Data;
        protected override LeaderboardEntryList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLeaderboardEntryArray(msg);
            return new LeaderboardEntryList(obj);
        }
    }
    public class MessageWithLinkedAccountList : Message<LinkedAccountList>
    {
        public MessageWithLinkedAccountList(IntPtr c_message) : base(c_message) { }
        public override LinkedAccountList GetLinkedAccountList() => Data;
        protected override LinkedAccountList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLinkedAccountArray(msg);
            return new LinkedAccountList(obj);
        }
    }
    public class MessageWithLivestreamingApplicationStatus : Message<LivestreamingApplicationStatus>
    {
        public MessageWithLivestreamingApplicationStatus(IntPtr c_message) : base(c_message) { }
        public override LivestreamingApplicationStatus GetLivestreamingApplicationStatus() => Data;
        protected override LivestreamingApplicationStatus GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLivestreamingApplicationStatus(msg);
            return new LivestreamingApplicationStatus(obj);
        }
    }
    public class MessageWithLivestreamingStartResult : Message<LivestreamingStartResult>
    {
        public MessageWithLivestreamingStartResult(IntPtr c_message) : base(c_message) { }
        public override LivestreamingStartResult GetLivestreamingStartResult() => Data;
        protected override LivestreamingStartResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLivestreamingStartResult(msg);
            return new LivestreamingStartResult(obj);
        }
    }
    public class MessageWithLivestreamingStatus : Message<LivestreamingStatus>
    {
        public MessageWithLivestreamingStatus(IntPtr c_message) : base(c_message) { }
        public override LivestreamingStatus GetLivestreamingStatus() => Data;
        protected override LivestreamingStatus GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLivestreamingStatus(msg);
            return new LivestreamingStatus(obj);
        }
    }
    public class MessageWithLivestreamingVideoStats : Message<LivestreamingVideoStats>
    {
        public MessageWithLivestreamingVideoStats(IntPtr c_message) : base(c_message) { }
        public override LivestreamingVideoStats GetLivestreamingVideoStats() => Data;
        protected override LivestreamingVideoStats GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLivestreamingVideoStats(msg);
            return new LivestreamingVideoStats(obj);
        }
    }
    public class MessageWithMatchmakingAdminSnapshot : Message<MatchmakingAdminSnapshot>
    {
        public MessageWithMatchmakingAdminSnapshot(IntPtr c_message) : base(c_message) { }
        public override MatchmakingAdminSnapshot GetMatchmakingAdminSnapshot() => Data;
        protected override MatchmakingAdminSnapshot GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetMatchmakingAdminSnapshot(msg);
            return new MatchmakingAdminSnapshot(obj);
        }
    }
    public class MessageWithMatchmakingEnqueueResult : Message<MatchmakingEnqueueResult>
    {
        public MessageWithMatchmakingEnqueueResult(IntPtr c_message) : base(c_message) { }
        public override MatchmakingEnqueueResult GetMatchmakingEnqueueResult() => Data;
        protected override MatchmakingEnqueueResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetMatchmakingEnqueueResult(msg);
            return new MatchmakingEnqueueResult(obj);
        }
    }
    public class MessageWithMatchmakingEnqueueResultAndRoom : Message<MatchmakingEnqueueResultAndRoom>
    {
        public MessageWithMatchmakingEnqueueResultAndRoom(IntPtr c_message) : base(c_message) { }
        public override MatchmakingEnqueueResultAndRoom GetMatchmakingEnqueueResultAndRoom() => Data;
        protected override MatchmakingEnqueueResultAndRoom GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetMatchmakingEnqueueResultAndRoom(msg);
            return new MatchmakingEnqueueResultAndRoom(obj);
        }
    }
    public class MessageWithMatchmakingStatsUnderMatchmakingStats : Message<MatchmakingStats>
    {
        public MessageWithMatchmakingStatsUnderMatchmakingStats(IntPtr c_message) : base(c_message) { }
        public override MatchmakingStats GetMatchmakingStats() => Data;
        protected override MatchmakingStats GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetMatchmakingStats(msg);
            return new MatchmakingStats(obj);
        }
    }
    public class MessageWithOrgScopedID : Message<OrgScopedID>
    {
        public MessageWithOrgScopedID(IntPtr c_message) : base(c_message) { }
        public override OrgScopedID GetOrgScopedID() => Data;
        protected override OrgScopedID GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetOrgScopedID(msg);
            return new OrgScopedID(obj);
        }
    }
    public class MessageWithParty : Message<Party>
    {
        public MessageWithParty(IntPtr c_message) : base(c_message) { }
        public override Party GetParty() => Data;
        protected override Party GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetParty(msg);
            return new Party(obj);
        }
    }
    public class MessageWithPartyUnderCurrentParty : Message<Party>
    {
        public MessageWithPartyUnderCurrentParty(IntPtr c_message) : base(c_message) { }
        public override Party GetParty() => Data;
        protected override Party GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetParty(msg);
            return new Party(obj);
        }
    }
    public class MessageWithPartyID : Message<PartyID>
    {
        public MessageWithPartyID(IntPtr c_message) : base(c_message) { }
        public override PartyID GetPartyID() => Data;
        protected override PartyID GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPartyID(msg);
            return new PartyID(obj);
        }
    }
    public class MessageWithPartyUpdateNotification : Message<PartyUpdateNotification>
    {
        public MessageWithPartyUpdateNotification(IntPtr c_message) : base(c_message) { }
        public override PartyUpdateNotification GetPartyUpdateNotification() => Data;
        protected override PartyUpdateNotification GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPartyUpdateNotification(msg);
            return new PartyUpdateNotification(obj);
        }
    }
    public class MessageWithPidList : Message<PidList>
    {
        public MessageWithPidList(IntPtr c_message) : base(c_message) { }
        public override PidList GetPidList() => Data;
        protected override PidList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPidArray(msg);
            return new PidList(obj);
        }
    }
    public class MessageWithProductList : Message<ProductList>
    {
        public MessageWithProductList(IntPtr c_message) : base(c_message) { }
        public override ProductList GetProductList() => Data;
        protected override ProductList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetProductArray(msg);
            return new ProductList(obj);
        }
    }
    public class MessageWithPurchase : Message<Purchase>
    {
        public MessageWithPurchase(IntPtr c_message) : base(c_message) { }
        public override Purchase GetPurchase() => Data;
        protected override Purchase GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPurchase(msg);
            return new Purchase(obj);
        }
    }
    public class MessageWithPurchaseList : Message<PurchaseList>
    {
        public MessageWithPurchaseList(IntPtr c_message) : base(c_message) { }
        public override PurchaseList GetPurchaseList() => Data;
        protected override PurchaseList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPurchaseArray(msg);
            return new PurchaseList(obj);
        }
    }
    public class MessageWithRoom : Message<Room>
    {
        public MessageWithRoom(IntPtr c_message) : base(c_message) { }
        public override Room GetRoom() => Data;
        protected override Room GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoom(msg);
            return new Room(obj);
        }
    }
    public class MessageWithRoomUnderCurrentRoom : Message<Room>
    {
        public MessageWithRoomUnderCurrentRoom(IntPtr c_message) : base(c_message) { }
        public override Room GetRoom() => Data;
        protected override Room GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoom(msg);
            return new Room(obj);
        }
    }
    public class MessageWithRoomUnderViewerRoom : Message<Room>
    {
        public MessageWithRoomUnderViewerRoom(IntPtr c_message) : base(c_message) { }
        public override Room GetRoom() => Data;
        protected override Room GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoom(msg);
            return new Room(obj);
        }
    }
    public class MessageWithRoomList : Message<RoomList>
    {
        public MessageWithRoomList(IntPtr c_message) : base(c_message) { }
        public override RoomList GetRoomList() => Data;
        protected override RoomList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoomArray(msg);
            return new RoomList(obj);
        }
    }
    public class MessageWithRoomInviteNotification : Message<RoomInviteNotification>
    {
        public MessageWithRoomInviteNotification(IntPtr c_message) : base(c_message) { }
        public override RoomInviteNotification GetRoomInviteNotification() => Data;
        protected override RoomInviteNotification GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoomInviteNotification(msg);
            return new RoomInviteNotification(obj);
        }
    }
    public class MessageWithRoomInviteNotificationList : Message<RoomInviteNotificationList>
    {
        public MessageWithRoomInviteNotificationList(IntPtr c_message) : base(c_message) { }
        public override RoomInviteNotificationList GetRoomInviteNotificationList() => Data;
        protected override RoomInviteNotificationList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoomInviteNotificationArray(msg);
            return new RoomInviteNotificationList(obj);
        }
    }
    public class MessageWithSdkAccountList : Message<SdkAccountList>
    {
        public MessageWithSdkAccountList(IntPtr c_message) : base(c_message) { }
        public override SdkAccountList GetSdkAccountList() => Data;
        protected override SdkAccountList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetSdkAccountArray(msg);
            return new SdkAccountList(obj);
        }
    }
    public class MessageWithShareMediaResult : Message<ShareMediaResult>
    {
        public MessageWithShareMediaResult(IntPtr c_message) : base(c_message) { }
        public override ShareMediaResult GetShareMediaResult() => Data;
        protected override ShareMediaResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetShareMediaResult(msg);
            return new ShareMediaResult(obj);
        }
    }
    public class MessageWithString : Message<string>
    {
        public MessageWithString(IntPtr c_message) : base(c_message) { }
        public override string GetString() => Data;
        protected override string GetDataFromMessage(IntPtr c_message)
        {
            return CAPI.ovr_Message_GetString(c_message);
        }
    }
    public class MessageWithSystemPermission : Message<SystemPermission>
    {
        public MessageWithSystemPermission(IntPtr c_message) : base(c_message) { }
        public override SystemPermission GetSystemPermission() => Data;
        protected override SystemPermission GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetSystemPermission(msg);
            return new SystemPermission(obj);
        }
    }
    public class MessageWithSystemVoipState : Message<SystemVoipState>
    {
        public MessageWithSystemVoipState(IntPtr c_message) : base(c_message) { }
        public override SystemVoipState GetSystemVoipState() => Data;
        protected override SystemVoipState GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetSystemVoipState(msg);
            return new SystemVoipState(obj);
        }
    }
    public class MessageWithUser : Message<User>
    {
        public MessageWithUser(IntPtr c_message) : base(c_message) { }
        public override User GetUser() => Data;
        protected override User GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetUser(msg);
            return new User(obj);
        }
    }
    public class MessageWithUserAndRoomList : Message<UserAndRoomList>
    {
        public MessageWithUserAndRoomList(IntPtr c_message) : base(c_message) { }
        public override UserAndRoomList GetUserAndRoomList() => Data;
        protected override UserAndRoomList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetUserAndRoomArray(msg);
            return new UserAndRoomList(obj);
        }
    }
    public class MessageWithUserList : Message<UserList>
    {
        public MessageWithUserList(IntPtr c_message) : base(c_message) { }
        public override UserList GetUserList() => Data;
        protected override UserList GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetUserArray(msg);
            return new UserList(obj);
        }
    }
    public class MessageWithUserProof : Message<UserProof>
    {
        public MessageWithUserProof(IntPtr c_message) : base(c_message) { }
        public override UserProof GetUserProof() => Data;
        protected override UserProof GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetUserProof(msg);
            return new UserProof(obj);
        }
    }
    public class MessageWithUserReportID : Message<UserReportID>
    {
        public MessageWithUserReportID(IntPtr c_message) : base(c_message) { }
        public override UserReportID GetUserReportID() => Data;
        protected override UserReportID GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetUserReportID(msg);
            return new UserReportID(obj);
        }
    }
    public class MessageWithNetworkingPeer : Message<NetworkingPeer>
    {
        public MessageWithNetworkingPeer(IntPtr c_message) : base(c_message) { }
        public override NetworkingPeer GetNetworkingPeer() => Data;
        protected override NetworkingPeer GetDataFromMessage(IntPtr c_message)
        {
            var peer = CAPI.ovr_Message_GetNetworkingPeer(c_message);
            return new NetworkingPeer(
              CAPI.ovr_NetworkingPeer_GetID(peer),
              CAPI.ovr_NetworkingPeer_GetState(peer)
            );
        }
    }
    public class MessageWithPingResult : Message<PingResult>
    {
        public MessageWithPingResult(IntPtr c_message) : base(c_message) { }
        public override PingResult GetPingResult() => Data;
        protected override PingResult GetDataFromMessage(IntPtr c_message)
        {
            var ping = CAPI.ovr_Message_GetPingResult(c_message);
            bool is_timeout = CAPI.ovr_PingResult_IsTimeout(ping);
            return new PingResult(
              CAPI.ovr_PingResult_GetID(ping),
              is_timeout ? (ulong?)null : CAPI.ovr_PingResult_GetPingTimeUsec(ping)
            );
        }
    }
    public class MessageWithLeaderboardDidUpdate : Message<bool>
    {
        public MessageWithLeaderboardDidUpdate(IntPtr c_message) : base(c_message) { }
        public override bool GetLeaderboardDidUpdate() => Data;
        protected override bool GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetLeaderboardUpdateStatus(msg);
            return CAPI.ovr_LeaderboardUpdateStatus_GetDidUpdate(obj);
        }
    }
    public class MessageWithMatchmakingNotification : Message<Room>
    {
        public MessageWithMatchmakingNotification(IntPtr c_message) : base(c_message) { }
        public override Room GetRoom() => Data;
        protected override Room GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetRoom(msg);
            return new Room(obj);
        }
    }
    public class MessageWithMatchmakingBrowseResult : Message<MatchmakingBrowseResult>
    {
        public MessageWithMatchmakingBrowseResult(IntPtr c_message) : base(c_message) { }
        public override MatchmakingEnqueueResult GetMatchmakingEnqueueResult() { return Data.EnqueueResult; }
        public override RoomList GetRoomList() => Data.Rooms;
        protected override MatchmakingBrowseResult GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetMatchmakingBrowseResult(msg);
            return new MatchmakingBrowseResult(obj);
        }
    }
    public class MessageWithHttpTransferUpdate : Message<HttpTransferUpdate>
    {
        public MessageWithHttpTransferUpdate(IntPtr c_message) : base(c_message) { }
        public override HttpTransferUpdate GetHttpTransferUpdate() => Data;
        protected override HttpTransferUpdate GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetHttpTransferUpdate(msg);
            return new HttpTransferUpdate(obj);
        }
    }
    public class MessageWithPlatformInitialize : Message<PlatformInitialize>
    {
        public MessageWithPlatformInitialize(IntPtr c_message) : base(c_message) { }
        public override PlatformInitialize GetPlatformInitialize() => Data;
        protected override PlatformInitialize GetDataFromMessage(IntPtr c_message)
        {
            var msg = CAPI.ovr_Message_GetNativeMessage(c_message);
            var obj = CAPI.ovr_Message_GetPlatformInitialize(msg);
            return new PlatformInitialize(obj);
        }
    }
}
