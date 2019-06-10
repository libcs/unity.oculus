using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 414
namespace Oculus.Platform
{
    public class CAPI
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
#if UNITY_64 || UNITY_EDITOR_64
        public const string DLL_NAME = "LibOVRPlatform64_1";
#else
        public const string DLL_NAME = "LibOVRPlatform32_1";
#endif
#elif UNITY_EDITOR || UNITY_EDITOR_64
        public const string DLL_NAME = "ovrplatform";
#else
        public const string DLL_NAME = "ovrplatformloader";
#endif

        static UTF8Encoding nativeStringEncoding = new UTF8Encoding(false);

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrKeyValuePair
        {
            public string key_;
            KeyValuePairType valueType_;
            public string stringValue_;
            public int intValue_;
            public double doubleValue_;

            public ovrKeyValuePair(string key, string value)
            {
                key_ = key;
                valueType_ = KeyValuePairType.String;
                stringValue_ = value;
                intValue_ = 0;
                doubleValue_ = 0.0;
            }

            public ovrKeyValuePair(string key, int value)
            {
                key_ = key;
                valueType_ = KeyValuePairType.Int;
                intValue_ = value;
                stringValue_ = null;
                doubleValue_ = 0.0;
            }

            public ovrKeyValuePair(string key, double value)
            {
                key_ = key;
                valueType_ = KeyValuePairType.Double;
                doubleValue_ = value;
                stringValue_ = null;
                intValue_ = 0;
            }
        };

        public static IntPtr ArrayOfStructsToIntPtr(Array ar)
        {
            var totalSize = 0;
            for (var i = 0; i < ar.Length; i++)
                totalSize += Marshal.SizeOf(ar.GetValue(i));
            var childrenPtr = Marshal.AllocHGlobal(totalSize);
            var curr = childrenPtr;
            for (var i = 0; i < ar.Length; i++)
            {
                Marshal.StructureToPtr(ar.GetValue(i), curr, false);
                curr = (IntPtr)((long)curr + Marshal.SizeOf(ar.GetValue(i)));
            }
            return childrenPtr;
        }

        public static ovrKeyValuePair[] DictionaryToOVRKeyValuePairs(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return null;
            var nativeCustomData = new ovrKeyValuePair[dict.Count];
            var i = 0;
            foreach (var item in dict)
            {
                if (item.Value.GetType() == typeof(int)) nativeCustomData[i] = new ovrKeyValuePair(item.Key, (int)item.Value);
                else if (item.Value.GetType() == typeof(string)) nativeCustomData[i] = new ovrKeyValuePair(item.Key, (string)item.Value);
                else if (item.Value.GetType() == typeof(double)) nativeCustomData[i] = new ovrKeyValuePair(item.Key, (double)item.Value);
                else throw new Exception("Only int, double or string are allowed types in CustomQuery.data");
                i++;
            }
            return nativeCustomData;
        }

        public static byte[] IntPtrToByteArray(IntPtr data, ulong size)
        {
            var outArray = new byte[size];
            Marshal.Copy(data, outArray, 0, (int)size);
            return outArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrMatchmakingCriterion
        {
            public ovrMatchmakingCriterion(string key, MatchmakingCriterionImportance importance)
            {
                key_ = key;
                importance_ = importance;
                parameterArray = IntPtr.Zero;
                parameterArrayCount = 0;
            }

            public string key_;
            public MatchmakingCriterionImportance importance_;
            public IntPtr parameterArray;
            public uint parameterArrayCount;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrMatchmakingCustomQueryData
        {
            public IntPtr dataArray;
            public uint dataArrayCount;
            public IntPtr criterionArray;
            public uint criterionArrayCount;
        };

        public static Dictionary<string, string> DataStoreFromNative(IntPtr pointer)
        {
            var d = new Dictionary<string, string>();
            var size = (int)ovr_DataStore_GetNumKeys(pointer);
            for (var i = 0; i < size; i++)
            {
                var key = ovr_DataStore_GetKey(pointer, i);
                d[key] = ovr_DataStore_GetValue(pointer, key);
            }
            return d;
        }

        public static string StringFromNative(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return null;
            var l = GetNativeStringLengthNotIncludingNullTerminator(pointer);
            var data = new byte[l];
            Marshal.Copy(pointer, data, 0, l);
            return nativeStringEncoding.GetString(data);
        }

        public static int GetNativeStringLengthNotIncludingNullTerminator(IntPtr pointer)
        {
            var l = 0;
            while (true)
            {
                if (Marshal.ReadByte(pointer, l) == 0)
                    return l;
                l++;
            }
        }

        public static DateTime DateTimeFromNative(ulong seconds_since_the_one_true_epoch)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dt.AddSeconds(seconds_since_the_one_true_epoch).ToLocalTime();
        }

        public static byte[] BlobFromNative(uint size, IntPtr pointer)
        {
            var a = new byte[(int)size];
            for (var i = 0; i < (int)size; i++)
                a[i] = Marshal.ReadByte(pointer, i);
            return a;
        }

        public static byte[] FiledataFromNative(uint size, IntPtr pointer)
        {
            var data = new byte[(int)size];
            Marshal.Copy(pointer, data, 0, (int)size);
            return data;
        }

        public static IntPtr StringToNative(string s)
        {
            if (s == null)
                throw new Exception("StringFromNative: null argument");
            var l = nativeStringEncoding.GetByteCount(s);
            var data = new byte[l + 1];
            nativeStringEncoding.GetBytes(s, 0, s.Length, data, 0);
            var pointer = Marshal.AllocCoTaskMem(l + 1);
            Marshal.Copy(data, 0, pointer, l + 1);
            return pointer;
        }

        // Initialization
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UnityInitWrapper(string appId);

        // Initializes just the global variables to use the Unity api without calling the init logic
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UnityInitGlobals(IntPtr loggingCB);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_UnityInitWrapperAsynchronous(string appId);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UnityInitWrapperStandalone(string accessToken, IntPtr loggingCB);

        [StructLayout(LayoutKind.Sequential)]
        public struct OculusInitParams
        {
            public int sType;
            public string email;
            public string password;
            public ulong appId;
            public string uriPrefixOverride;
        }

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Platform_InitializeStandaloneOculus(ref OculusInitParams init);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PlatformInitializeWithAccessToken(ulong appId, string accessToken);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UnityInitWrapperWindows(string appId, IntPtr loggingCB);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_UnityInitWrapperWindowsAsynchronous(string appId, IntPtr loggingCB);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_SetDeveloperAccessToken(string accessToken);
        public static string ovr_GetLoggedInUserLocale() => StringFromNative(ovr_GetLoggedInUserLocale_Native());
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GetLoggedInUserLocale")] static extern IntPtr ovr_GetLoggedInUserLocale_Native();

        // Message queue access
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_PopMessage();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_FreeMessage(IntPtr message);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_NetworkingPeer_GetSendPolicy(IntPtr networkingPeer);

        // VOIP
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Voip_CreateEncoder();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_DestroyEncoder(IntPtr encoder);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Voip_CreateDecoder();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_DestroyDecoder(IntPtr decoder);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipDecoder_Decode(IntPtr obj, byte[] compressedData, ulong compressedSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Microphone_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Microphone_Destroy(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetSystemVoipPassthrough(bool passthrough);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetSystemVoipMicrophoneMuted(VoipMuteState muted);

        // Misc
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UnityResetTestPlatform();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_HTTP_GetWithMessageType(string url, int messageType);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_CrashApplication();
        public const int VoipFilterBufferSize = 480;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void FilterCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = VoipFilterBufferSize), In, Out] short[] pcmData, UIntPtr pcmDataLength, int frequency, int numChannels);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetMicrophoneFilterCallback(FilterCallback cb);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetMicrophoneFilterCallbackWithFixedSizeBuffer(FilterCallback cb, UIntPtr bufferSizeElements);

        // Logging
        public static void LogNewEvent(string eventName, Dictionary<string, string> values)
        {
            var eventNameNative = StringToNative(eventName);
            var count = values == null ? 0 : values.Count;
            var valuesNative = new IntPtr[count * 2];
            if (count > 0)
            {
                var i = 0;
                foreach (var item in values)
                {
                    valuesNative[i * 2 + 0] = StringToNative(item.Key);
                    valuesNative[i * 2 + 1] = StringToNative(item.Value);
                    i++;
                }
            }
            ovr_Log_NewEvent(eventNameNative, valuesNative, (UIntPtr)count);
            Marshal.FreeCoTaskMem(eventNameNative);
            foreach (var nativeItem in valuesNative)
                Marshal.FreeCoTaskMem(nativeItem);
        }

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Log_NewEvent(IntPtr eventName, IntPtr[] values, UIntPtr length);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_ApplicationLifecycle_GetLaunchDetails();
        public static ulong ovr_HTTP_StartTransfer(string url, ovrKeyValuePair[] headers)
        {
            var url_native = StringToNative(url);
            var headers_length = (UIntPtr)headers.Length;
            var result = (ovr_HTTP_StartTransfer_Native(url_native, headers, headers_length));
            Marshal.FreeCoTaskMem(url_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_StartTransfer")] static extern ulong ovr_HTTP_StartTransfer_Native(IntPtr url, ovrKeyValuePair[] headers, UIntPtr numItems);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_HTTP_Write(ulong transferId, byte[] bytes, UIntPtr length);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_HTTP_WriteEOM(ulong transferId);
        public static string ovr_Message_GetStringForJavascript(IntPtr message) => StringFromNative(ovr_Message_GetStringForJavascript_Native(message));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Message_GetStringForJavascript")] static extern IntPtr ovr_Message_GetStringForJavascript_Native(IntPtr message);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Net_Accept(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Net_AcceptForCurrentRoom();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Net_Close(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Net_CloseForCurrentRoom();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Net_Connect(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Net_IsConnected(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Net_Ping(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Net_ReadPacket();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Net_SendPacket(ulong userID, UIntPtr length, byte[] bytes, SendPolicy policy);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Net_SendPacketToCurrentRoom(UIntPtr length, byte[] bytes, SendPolicy policy);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_Party_PluginGetSharedMemHandle();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipMuteState ovr_Party_PluginGetVoipMicrophoneMuted();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Party_PluginGetVoipPassthrough();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern SystemVoipStatus ovr_Party_PluginGetVoipStatus();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_Accept(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipDtxState ovr_Voip_GetIsConnectionUsingDtx(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipBitrate ovr_Voip_GetLocalBitrate(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetOutputBufferMaxSize();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetPCM(ulong senderID, short[] outputBuffer, UIntPtr outputBufferNumElements);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetPCMFloat(ulong senderID, float[] outputBuffer, UIntPtr outputBufferNumElements);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetPCMSize(ulong senderID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetPCMWithTimestamp(ulong senderID, short[] outputBuffer, UIntPtr outputBufferNumElements, uint[] timestamp);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Voip_GetPCMWithTimestampFloat(ulong senderID, float[] outputBuffer, UIntPtr outputBufferNumElements, uint[] timestamp);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipBitrate ovr_Voip_GetRemoteBitrate(ulong peerID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_Voip_GetSyncTimestamp(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern long ovr_Voip_GetSyncTimestampDifference(uint lhs, uint rhs);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipMuteState ovr_Voip_GetSystemVoipMicrophoneMuted();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern SystemVoipStatus ovr_Voip_GetSystemVoipStatus();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetMicrophoneMuted(VoipMuteState state);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetNewConnectionOptions(IntPtr voipOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_SetOutputSampleRate(VoipSampleRate rate);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_Start(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Voip_Stop(ulong userID);
        public static ulong ovr_Achievements_AddCount(string name, ulong count)
        {
            var name_native = StringToNative(name);
            var result = ovr_Achievements_AddCount_Native(name_native, count);
            Marshal.FreeCoTaskMem(name_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_AddCount")] static extern ulong ovr_Achievements_AddCount_Native(IntPtr name, ulong count);
        public static ulong ovr_Achievements_AddFields(string name, string fields)
        {
            var name_native = StringToNative(name);
            var fields_native = StringToNative(fields);
            var result = ovr_Achievements_AddFields_Native(name_native, fields_native);
            Marshal.FreeCoTaskMem(name_native);
            Marshal.FreeCoTaskMem(fields_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_AddFields")] static extern ulong ovr_Achievements_AddFields_Native(IntPtr name, IntPtr fields);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Achievements_GetAllDefinitions();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Achievements_GetAllProgress();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Achievements_GetDefinitionsByName(string[] names, int count);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Achievements_GetProgressByName(string[] names, int count);
        public static ulong ovr_Achievements_Unlock(string name)
        {
            var name_native = StringToNative(name);
            var result = ovr_Achievements_Unlock_Native(name_native);
            Marshal.FreeCoTaskMem(name_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Achievements_Unlock")] static extern ulong ovr_Achievements_Unlock_Native(IntPtr name);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Application_ExecuteCoordinatedLaunch(ulong appID, ulong roomID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Application_GetInstalledApplications();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Application_GetVersion();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Application_LaunchOtherApp(ulong appID, IntPtr deeplink_options);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_ApplicationLifecycle_GetRegisteredPIDs();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_ApplicationLifecycle_GetSessionKey();
        public static ulong ovr_ApplicationLifecycle_RegisterSessionKey(string sessionKey)
        {
            var sessionKey_native = StringToNative(sessionKey);
            var result = ovr_ApplicationLifecycle_RegisterSessionKey_Native(sessionKey_native);
            Marshal.FreeCoTaskMem(sessionKey_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationLifecycle_RegisterSessionKey")] static extern ulong ovr_ApplicationLifecycle_RegisterSessionKey_Native(IntPtr sessionKey);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_Delete(ulong assetFileID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_DeleteById(ulong assetFileID);
        public static ulong ovr_AssetFile_DeleteByName(string assetFileName)
        {
            var assetFileName_native = StringToNative(assetFileName);
            var result = ovr_AssetFile_DeleteByName_Native(assetFileName_native);
            Marshal.FreeCoTaskMem(assetFileName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFile_DeleteByName")] static extern ulong ovr_AssetFile_DeleteByName_Native(IntPtr assetFileName);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_Download(ulong assetFileID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_DownloadById(ulong assetFileID);
        public static ulong ovr_AssetFile_DownloadByName(string assetFileName)
        {
            var assetFileName_native = StringToNative(assetFileName);
            var result = ovr_AssetFile_DownloadByName_Native(assetFileName_native);
            Marshal.FreeCoTaskMem(assetFileName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFile_DownloadByName")] static extern ulong ovr_AssetFile_DownloadByName_Native(IntPtr assetFileName);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_DownloadCancel(ulong assetFileID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_DownloadCancelById(ulong assetFileID);
        public static ulong ovr_AssetFile_DownloadCancelByName(string assetFileName)
        {
            var assetFileName_native = StringToNative(assetFileName);
            var result = ovr_AssetFile_DownloadCancelByName_Native(assetFileName_native);
            Marshal.FreeCoTaskMem(assetFileName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFile_DownloadCancelByName")] static extern ulong ovr_AssetFile_DownloadCancelByName_Native(IntPtr assetFileName);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_GetList();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_Status(ulong assetFileID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFile_StatusById(ulong assetFileID);
        public static ulong ovr_AssetFile_StatusByName(string assetFileName)
        {
            var assetFileName_native = StringToNative(assetFileName);
            var result = ovr_AssetFile_StatusByName_Native(assetFileName_native);
            Marshal.FreeCoTaskMem(assetFileName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFile_StatusByName")] static extern ulong ovr_AssetFile_StatusByName_Native(IntPtr assetFileName);
        public static ulong ovr_Avatar_UpdateMetaData(string avatarMetaData, string imageFilePath)
        {
            var avatarMetaData_native = StringToNative(avatarMetaData);
            var imageFilePath_native = StringToNative(imageFilePath);
            var result = ovr_Avatar_UpdateMetaData_Native(avatarMetaData_native, imageFilePath_native);
            Marshal.FreeCoTaskMem(avatarMetaData_native);
            Marshal.FreeCoTaskMem(imageFilePath_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Avatar_UpdateMetaData")] static extern ulong ovr_Avatar_UpdateMetaData_Native(IntPtr avatarMetaData, IntPtr imageFilePath);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Cal_FinalizeApplication(ulong groupingObject, ulong[] userIDs, int numUserIDs, ulong finalized_application_ID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Cal_GetSuggestedApplications(ulong groupingObject, ulong[] userIDs, int numUserIDs);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Cal_ProposeApplication(ulong groupingObject, ulong[] userIDs, int numUserIDs, ulong proposed_application_ID);
        public static ulong ovr_CloudStorage_Delete(string bucket, string key)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var result = ovr_CloudStorage_Delete_Native(bucket_native, key_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Delete")] static extern ulong ovr_CloudStorage_Delete_Native(IntPtr bucket, IntPtr key);
        public static ulong ovr_CloudStorage_Load(string bucket, string key)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var result = ovr_CloudStorage_Load_Native(bucket_native, key_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Load")] static extern ulong ovr_CloudStorage_Load_Native(IntPtr bucket, IntPtr key);
        public static ulong ovr_CloudStorage_LoadBucketMetadata(string bucket)
        {
            var bucket_native = StringToNative(bucket);
            var result = ovr_CloudStorage_LoadBucketMetadata_Native(bucket_native);
            Marshal.FreeCoTaskMem(bucket_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadBucketMetadata")] static extern ulong ovr_CloudStorage_LoadBucketMetadata_Native(IntPtr bucket);
        public static ulong ovr_CloudStorage_LoadConflictMetadata(string bucket, string key)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var result = ovr_CloudStorage_LoadConflictMetadata_Native(bucket_native, key_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadConflictMetadata")] static extern ulong ovr_CloudStorage_LoadConflictMetadata_Native(IntPtr bucket, IntPtr key);
        public static ulong ovr_CloudStorage_LoadHandle(string handle)
        {
            var handle_native = StringToNative(handle);
            var result = ovr_CloudStorage_LoadHandle_Native(handle_native);
            Marshal.FreeCoTaskMem(handle_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadHandle")] static extern ulong ovr_CloudStorage_LoadHandle_Native(IntPtr handle);
        public static ulong ovr_CloudStorage_LoadMetadata(string bucket, string key)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var result = ovr_CloudStorage_LoadMetadata_Native(bucket_native, key_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_LoadMetadata")] static extern ulong ovr_CloudStorage_LoadMetadata_Native(IntPtr bucket, IntPtr key);
        public static ulong ovr_CloudStorage_ResolveKeepLocal(string bucket, string key, string remoteHandle)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var remoteHandle_native = StringToNative(remoteHandle);
            var result = ovr_CloudStorage_ResolveKeepLocal_Native(bucket_native, key_native, remoteHandle_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(remoteHandle_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_ResolveKeepLocal")] static extern ulong ovr_CloudStorage_ResolveKeepLocal_Native(IntPtr bucket, IntPtr key, IntPtr remoteHandle);
        public static ulong ovr_CloudStorage_ResolveKeepRemote(string bucket, string key, string remoteHandle)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var remoteHandle_native = StringToNative(remoteHandle);
            var result = ovr_CloudStorage_ResolveKeepRemote_Native(bucket_native, key_native, remoteHandle_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(remoteHandle_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_ResolveKeepRemote")] static extern ulong ovr_CloudStorage_ResolveKeepRemote_Native(IntPtr bucket, IntPtr key, IntPtr remoteHandle);
        public static ulong ovr_CloudStorage_Save(string bucket, string key, byte[] data, uint dataSize, long counter, string extraData)
        {
            var bucket_native = StringToNative(bucket);
            var key_native = StringToNative(key);
            var extraData_native = StringToNative(extraData);
            var result = ovr_CloudStorage_Save_Native(bucket_native, key_native, data, dataSize, counter, extraData_native);
            Marshal.FreeCoTaskMem(bucket_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(extraData_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage_Save")] static extern ulong ovr_CloudStorage_Save_Native(IntPtr bucket, IntPtr key, byte[] data, uint dataSize, long counter, IntPtr extraData);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_CloudStorage2_GetUserDirectoryPath();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Entitlement_GetIsViewerEntitled();
        public static ulong ovr_GraphAPI_Get(string url)
        {
            var url_native = StringToNative(url);
            var result = ovr_GraphAPI_Get_Native(url_native);
            Marshal.FreeCoTaskMem(url_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GraphAPI_Get")] static extern ulong ovr_GraphAPI_Get_Native(IntPtr url);
        public static ulong ovr_GraphAPI_Post(string url)
        {
            var url_native = StringToNative(url);
            var result = ovr_GraphAPI_Post_Native(url_native);
            Marshal.FreeCoTaskMem(url_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_GraphAPI_Post")] static extern ulong ovr_GraphAPI_Post_Native(IntPtr url);
        public static ulong ovr_HTTP_Get(string url)
        {
            var url_native = StringToNative(url);
            var result = ovr_HTTP_Get_Native(url_native);
            Marshal.FreeCoTaskMem(url_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_Get")] static extern ulong ovr_HTTP_Get_Native(IntPtr url);
        public static ulong ovr_HTTP_GetToFile(string url, string diskFile)
        {
            var url_native = StringToNative(url);
            var diskFile_native = StringToNative(diskFile);
            var result = ovr_HTTP_GetToFile_Native(url_native, diskFile_native);
            Marshal.FreeCoTaskMem(url_native);
            Marshal.FreeCoTaskMem(diskFile_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_GetToFile")] static extern ulong ovr_HTTP_GetToFile_Native(IntPtr url, IntPtr diskFile);
        public static ulong ovr_HTTP_MultiPartPost(string url, string filepath_param_name, string filepath, string access_token, ovrKeyValuePair[] post_params)
        {
            var url_native = StringToNative(url);
            var filepath_param_name_native = StringToNative(filepath_param_name);
            var filepath_native = StringToNative(filepath);
            var access_token_native = StringToNative(access_token);
            var post_params_length = (UIntPtr)post_params.Length;
            var result = ovr_HTTP_MultiPartPost_Native(url_native, filepath_param_name_native, filepath_native, access_token_native, post_params, post_params_length);
            Marshal.FreeCoTaskMem(url_native);
            Marshal.FreeCoTaskMem(filepath_param_name_native);
            Marshal.FreeCoTaskMem(filepath_native);
            Marshal.FreeCoTaskMem(access_token_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_MultiPartPost")] static extern ulong ovr_HTTP_MultiPartPost_Native(IntPtr url, IntPtr filepath_param_name, IntPtr filepath, IntPtr access_token, ovrKeyValuePair[] post_params, UIntPtr numItems);
        public static ulong ovr_HTTP_Post(string url)
        {
            var url_native = StringToNative(url);
            var result = ovr_HTTP_Post_Native(url_native);
            Marshal.FreeCoTaskMem(url_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_HTTP_Post")] static extern ulong ovr_HTTP_Post_Native(IntPtr url);
        public static ulong ovr_IAP_ConsumePurchase(string sku)
        {
            var sku_native = StringToNative(sku);
            var result = ovr_IAP_ConsumePurchase_Native(sku_native);
            Marshal.FreeCoTaskMem(sku_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_IAP_ConsumePurchase")] static extern ulong ovr_IAP_ConsumePurchase_Native(IntPtr sku);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_IAP_GetProductsBySKU(string[] skus, int count);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_IAP_GetViewerPurchases();
        public static ulong ovr_IAP_LaunchCheckoutFlow(string sku)
        {
            var sku_native = StringToNative(sku);
            var result = ovr_IAP_LaunchCheckoutFlow_Native(sku_native);
            Marshal.FreeCoTaskMem(sku_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_IAP_LaunchCheckoutFlow")] static extern ulong ovr_IAP_LaunchCheckoutFlow_Native(IntPtr sku);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_LanguagePack_GetCurrent();
        public static ulong ovr_LanguagePack_SetCurrent(string tag)
        {
            var tag_native = StringToNative(tag);
            var result = ovr_LanguagePack_SetCurrent_Native(tag_native);
            Marshal.FreeCoTaskMem(tag_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LanguagePack_SetCurrent")] static extern ulong ovr_LanguagePack_SetCurrent_Native(IntPtr tag);
        public static ulong ovr_Leaderboard_GetEntries(string leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt)
        {
            var leaderboardName_native = StringToNative(leaderboardName);
            var result = ovr_Leaderboard_GetEntries_Native(leaderboardName_native, limit, filter, startAt);
            Marshal.FreeCoTaskMem(leaderboardName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_GetEntries")] static extern ulong ovr_Leaderboard_GetEntries_Native(IntPtr leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt);
        public static ulong ovr_Leaderboard_GetEntriesAfterRank(string leaderboardName, int limit, ulong afterRank)
        {
            var leaderboardName_native = StringToNative(leaderboardName);
            var result = ovr_Leaderboard_GetEntriesAfterRank_Native(leaderboardName_native, limit, afterRank);
            Marshal.FreeCoTaskMem(leaderboardName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_GetEntriesAfterRank")] static extern ulong ovr_Leaderboard_GetEntriesAfterRank_Native(IntPtr leaderboardName, int limit, ulong afterRank);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Leaderboard_GetNextEntries(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Leaderboard_GetPreviousEntries(IntPtr handle);
        public static ulong ovr_Leaderboard_WriteEntry(string leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate)
        {
            var leaderboardName_native = StringToNative(leaderboardName);
            var result = ovr_Leaderboard_WriteEntry_Native(leaderboardName_native, score, extraData, extraDataLength, forceUpdate);
            Marshal.FreeCoTaskMem(leaderboardName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Leaderboard_WriteEntry")] static extern ulong ovr_Leaderboard_WriteEntry_Native(IntPtr leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_GetStatus();
        public static ulong ovr_Livestreaming_IsAllowedForApplication(string packageName)
        {
            var packageName_native = StringToNative(packageName);
            var result = ovr_Livestreaming_IsAllowedForApplication_Native(packageName_native);
            Marshal.FreeCoTaskMem(packageName_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Livestreaming_IsAllowedForApplication")] static extern ulong ovr_Livestreaming_IsAllowedForApplication_Native(IntPtr packageName);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_PauseStream();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_ResumeStream();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_StartPartyStream();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_StartStream(LivestreamingAudience audience, LivestreamingMicrophoneStatus micStatus);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_StopPartyStream();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_StopStream();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_UpdateCommentsOverlayVisibility(bool isVisible);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Livestreaming_UpdateMicStatus(LivestreamingMicrophoneStatus micStatus);
        public static ulong ovr_Matchmaking_Browse(string pool, IntPtr customQueryData)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_Browse_Native(pool_native, customQueryData);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Browse")] static extern ulong ovr_Matchmaking_Browse_Native(IntPtr pool, IntPtr customQueryData);
        public static ulong ovr_Matchmaking_Browse2(string pool, IntPtr matchmakingOptions)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_Browse2_Native(pool_native, matchmakingOptions);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Browse2")] static extern ulong ovr_Matchmaking_Browse2_Native(IntPtr pool, IntPtr matchmakingOptions);
        public static ulong ovr_Matchmaking_Cancel(string pool, string requestHash)
        {
            var pool_native = StringToNative(pool);
            var requestHash_native = StringToNative(requestHash);
            var result = ovr_Matchmaking_Cancel_Native(pool_native, requestHash_native);
            Marshal.FreeCoTaskMem(pool_native);
            Marshal.FreeCoTaskMem(requestHash_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Cancel")] static extern ulong ovr_Matchmaking_Cancel_Native(IntPtr pool, IntPtr requestHash);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_Cancel2();
        public static ulong ovr_Matchmaking_CreateAndEnqueueRoom(string pool, uint maxUsers, bool subscribeToUpdates, IntPtr customQueryData)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_CreateAndEnqueueRoom_Native(pool_native, maxUsers, subscribeToUpdates, customQueryData);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateAndEnqueueRoom")] static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom_Native(IntPtr pool, uint maxUsers, bool subscribeToUpdates, IntPtr customQueryData);
        public static ulong ovr_Matchmaking_CreateAndEnqueueRoom2(string pool, IntPtr matchmakingOptions)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_CreateAndEnqueueRoom2_Native(pool_native, matchmakingOptions);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateAndEnqueueRoom2")] static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom2_Native(IntPtr pool, IntPtr matchmakingOptions);
        public static ulong ovr_Matchmaking_CreateRoom(string pool, uint maxUsers, bool subscribeToUpdates)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_CreateRoom_Native(pool_native, maxUsers, subscribeToUpdates);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateRoom")] static extern ulong ovr_Matchmaking_CreateRoom_Native(IntPtr pool, uint maxUsers, bool subscribeToUpdates);
        public static ulong ovr_Matchmaking_CreateRoom2(string pool, IntPtr matchmakingOptions)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_CreateRoom2_Native(pool_native, matchmakingOptions);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_CreateRoom2")] static extern ulong ovr_Matchmaking_CreateRoom2_Native(IntPtr pool, IntPtr matchmakingOptions);
        public static ulong ovr_Matchmaking_Enqueue(string pool, IntPtr customQueryData)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_Enqueue_Native(pool_native, customQueryData);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Enqueue")] static extern ulong ovr_Matchmaking_Enqueue_Native(IntPtr pool, IntPtr customQueryData);
        public static ulong ovr_Matchmaking_Enqueue2(string pool, IntPtr matchmakingOptions)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_Enqueue2_Native(pool_native, matchmakingOptions);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_Enqueue2")] static extern ulong ovr_Matchmaking_Enqueue2_Native(IntPtr pool, IntPtr matchmakingOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_EnqueueRoom(ulong roomID, IntPtr customQueryData);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_EnqueueRoom2(ulong roomID, IntPtr matchmakingOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_GetAdminSnapshot();
        public static ulong ovr_Matchmaking_GetStats(string pool, uint maxLevel, MatchmakingStatApproach approach)
        {
            var pool_native = StringToNative(pool);
            var result = ovr_Matchmaking_GetStats_Native(pool_native, maxLevel, approach);
            Marshal.FreeCoTaskMem(pool_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_GetStats")] static extern ulong ovr_Matchmaking_GetStats_Native(IntPtr pool, uint maxLevel, MatchmakingStatApproach approach);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_JoinRoom(ulong roomID, bool subscribeToUpdates);
        public static ulong ovr_Matchmaking_ReportResultInsecure(ulong roomID, ovrKeyValuePair[] data)
        {
            var data_length = (UIntPtr)data.Length;
            var result = ovr_Matchmaking_ReportResultInsecure_Native(roomID, data, data_length);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Matchmaking_ReportResultInsecure")] static extern ulong ovr_Matchmaking_ReportResultInsecure_Native(ulong roomID, ovrKeyValuePair[] data, UIntPtr numItems);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Matchmaking_StartMatch(ulong roomID);
        public static ulong ovr_Media_ShareToFacebook(string postTextSuggestion, string filePath, MediaContentType contentType)
        {
            var postTextSuggestion_native = StringToNative(postTextSuggestion);
            var filePath_native = StringToNative(filePath);
            var result = ovr_Media_ShareToFacebook_Native(postTextSuggestion_native, filePath_native, contentType);
            Marshal.FreeCoTaskMem(postTextSuggestion_native);
            Marshal.FreeCoTaskMem(filePath_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Media_ShareToFacebook")] static extern ulong ovr_Media_ShareToFacebook_Native(IntPtr postTextSuggestion, IntPtr filePath, MediaContentType contentType);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Notification_GetRoomInvites();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Notification_MarkAsRead(ulong notificationID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_GatherInApplication(ulong partyID, ulong appID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_Get(ulong partyID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_GetCurrent();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_GetCurrentForUser(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_Invite(ulong partyID, ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_Join(ulong partyID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_Leave(ulong partyID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_CreateAndJoinPrivate(RoomJoinPolicy joinPolicy, uint maxUsers, bool subscribeToUpdates);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_CreateAndJoinPrivate2(RoomJoinPolicy joinPolicy, uint maxUsers, IntPtr roomOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_Get(ulong roomID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetCurrent();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetCurrentForUser(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetInvitableUsers();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetInvitableUsers2(IntPtr roomOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetModeratedRooms();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetSocialRooms(ulong appID);
        public static ulong ovr_Room_InviteUser(ulong roomID, string inviteToken)
        {
            var inviteToken_native = StringToNative(inviteToken);
            var result = ovr_Room_InviteUser_Native(roomID, inviteToken_native);
            Marshal.FreeCoTaskMem(inviteToken_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_InviteUser")] static extern ulong ovr_Room_InviteUser_Native(ulong roomID, IntPtr inviteToken);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_Join(ulong roomID, bool subscribeToUpdates);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_Join2(ulong roomID, IntPtr roomOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_KickUser(ulong roomID, ulong userID, int kickDurationSeconds);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_LaunchInvitableUserFlow(ulong roomID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_Leave(ulong roomID);
        public static ulong ovr_Room_SetDescription(ulong roomID, string description)
        {
            var description_native = StringToNative(description);
            var result = ovr_Room_SetDescription_Native(roomID, description_native);
            Marshal.FreeCoTaskMem(description_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_SetDescription")] static extern ulong ovr_Room_SetDescription_Native(ulong roomID, IntPtr description);
        public static ulong ovr_Room_UpdateDataStore(ulong roomID, ovrKeyValuePair[] data)
        {
            var data_length = (UIntPtr)data.Length;
            var result = ovr_Room_UpdateDataStore_Native(roomID, data, data_length);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_UpdateDataStore")] static extern ulong ovr_Room_UpdateDataStore_Native(ulong roomID, ovrKeyValuePair[] data, UIntPtr numItems);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_UpdateMembershipLockStatus(ulong roomID, RoomMembershipLockStatus membershipLockStatus);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_UpdateOwner(ulong roomID, ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_UpdatePrivateRoomJoinPolicy(ulong roomID, RoomJoinPolicy newJoinPolicy);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_SystemPermissions_GetStatus(PermissionType permType);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_SystemPermissions_LaunchDeeplink(PermissionType permType);
        public static ulong ovr_User_CancelRecordingForReportFlow(string recordingUUID)
        {
            var recordingUUID_native = StringToNative(recordingUUID);
            var result = ovr_User_CancelRecordingForReportFlow_Native(recordingUUID_native);
            Marshal.FreeCoTaskMem(recordingUUID_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_CancelRecordingForReportFlow")] static extern ulong ovr_User_CancelRecordingForReportFlow_Native(IntPtr recordingUUID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_Get(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetAccessToken();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetLinkedAccounts(IntPtr userOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetLoggedInUser();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetLoggedInUserFriends();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetLoggedInUserFriendsAndRooms();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetLoggedInUserRecentlyMetUsersAndRooms(IntPtr userOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetOrgScopedID(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetSdkAccounts();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetUserProof();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchBlockFlow(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchFriendRequestFlow(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchProfile(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchReportFlow(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchReportFlow2(ulong optionalUserID, IntPtr abuseReportOptions);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_LaunchUnblockFlow(ulong userID);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_NewEntitledTestUser();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_NewTestUser();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_NewTestUserFriends();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_StartRecordingForReportFlow();
        public static ulong ovr_User_StopRecordingAndLaunchReportFlow(ulong optionalUserID, string optionalRecordingUUID)
        {
            var optionalRecordingUUID_native = StringToNative(optionalRecordingUUID);
            var result = ovr_User_StopRecordingAndLaunchReportFlow_Native(optionalUserID, optionalRecordingUUID_native);
            Marshal.FreeCoTaskMem(optionalRecordingUUID_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_StopRecordingAndLaunchReportFlow")] static extern ulong ovr_User_StopRecordingAndLaunchReportFlow_Native(ulong optionalUserID, IntPtr optionalRecordingUUID);
        public static ulong ovr_User_StopRecordingAndLaunchReportFlow2(ulong optionalUserID, string optionalRecordingUUID, IntPtr abuseReportOptions)
        {
            var optionalRecordingUUID_native = StringToNative(optionalRecordingUUID);
            var result = ovr_User_StopRecordingAndLaunchReportFlow2_Native(optionalUserID, optionalRecordingUUID_native, abuseReportOptions);
            Marshal.FreeCoTaskMem(optionalRecordingUUID_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_StopRecordingAndLaunchReportFlow2")] static extern ulong ovr_User_StopRecordingAndLaunchReportFlow2_Native(ulong optionalUserID, IntPtr optionalRecordingUUID, IntPtr abuseReportOptions);
        public static ulong ovr_User_TestUserCreateDeviceManifest(string deviceID, ulong[] appIDs, int numAppIDs)
        {
            var deviceID_native = StringToNative(deviceID);
            var result = ovr_User_TestUserCreateDeviceManifest_Native(deviceID_native, appIDs, numAppIDs);
            Marshal.FreeCoTaskMem(deviceID_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_TestUserCreateDeviceManifest")] static extern ulong ovr_User_TestUserCreateDeviceManifest_Native(IntPtr deviceID, ulong[] appIDs, int numAppIDs);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Voip_SetSystemVoipSuppressed(bool suppressed);
        public static string ovr_AbuseReportRecording_GetRecordingUuid(IntPtr obj) => StringFromNative(ovr_AbuseReportRecording_GetRecordingUuid_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AbuseReportRecording_GetRecordingUuid")] static extern IntPtr ovr_AbuseReportRecording_GetRecordingUuid_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_AchievementDefinition_GetBitfieldLength(IntPtr obj);
        public static string ovr_AchievementDefinition_GetName(IntPtr obj) => StringFromNative(ovr_AchievementDefinition_GetName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementDefinition_GetName")] static extern IntPtr ovr_AchievementDefinition_GetName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AchievementDefinition_GetTarget(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern AchievementType ovr_AchievementDefinition_GetType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_AchievementDefinitionArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_AchievementDefinitionArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_AchievementDefinitionArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementDefinitionArray_GetNextUrl")] static extern IntPtr ovr_AchievementDefinitionArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_AchievementDefinitionArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AchievementDefinitionArray_HasNextPage(IntPtr obj);
        public static string ovr_AchievementProgress_GetBitfield(IntPtr obj) => StringFromNative(ovr_AchievementProgress_GetBitfield_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetBitfield")] static extern IntPtr ovr_AchievementProgress_GetBitfield_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AchievementProgress_GetCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AchievementProgress_GetIsUnlocked(IntPtr obj);
        public static string ovr_AchievementProgress_GetName(IntPtr obj) => StringFromNative(ovr_AchievementProgress_GetName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetName")] static extern IntPtr ovr_AchievementProgress_GetName_Native(IntPtr obj);
        public static DateTime ovr_AchievementProgress_GetUnlockTime(IntPtr obj) => DateTimeFromNative(ovr_AchievementProgress_GetUnlockTime_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgress_GetUnlockTime")] static extern ulong ovr_AchievementProgress_GetUnlockTime_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_AchievementProgressArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_AchievementProgressArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_AchievementProgressArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementProgressArray_GetNextUrl")] static extern IntPtr ovr_AchievementProgressArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_AchievementProgressArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AchievementProgressArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AchievementUpdate_GetJustUnlocked(IntPtr obj);
        public static string ovr_AchievementUpdate_GetName(IntPtr obj) => StringFromNative(ovr_AchievementUpdate_GetName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AchievementUpdate_GetName")] static extern IntPtr ovr_AchievementUpdate_GetName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Application_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_ApplicationVersion_GetCurrentCode(IntPtr obj);
        public static string ovr_ApplicationVersion_GetCurrentName(IntPtr obj) => StringFromNative(ovr_ApplicationVersion_GetCurrentName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationVersion_GetCurrentName")] static extern IntPtr ovr_ApplicationVersion_GetCurrentName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_ApplicationVersion_GetLatestCode(IntPtr obj);
        public static string ovr_ApplicationVersion_GetLatestName(IntPtr obj) => StringFromNative(ovr_ApplicationVersion_GetLatestName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationVersion_GetLatestName")] static extern IntPtr ovr_ApplicationVersion_GetLatestName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetDetails_GetAssetId(IntPtr obj);
        public static string ovr_AssetDetails_GetAssetType(IntPtr obj) => StringFromNative(ovr_AssetDetails_GetAssetType_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetDetails_GetAssetType")] static extern IntPtr ovr_AssetDetails_GetAssetType_Native(IntPtr obj);
        public static string ovr_AssetDetails_GetDownloadStatus(IntPtr obj) => StringFromNative(ovr_AssetDetails_GetDownloadStatus_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetDetails_GetDownloadStatus")] static extern IntPtr ovr_AssetDetails_GetDownloadStatus_Native(IntPtr obj);
        public static string ovr_AssetDetails_GetFilepath(IntPtr obj) => StringFromNative(ovr_AssetDetails_GetFilepath_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetDetails_GetFilepath")] static extern IntPtr ovr_AssetDetails_GetFilepath_Native(IntPtr obj);
        public static string ovr_AssetDetails_GetIapStatus(IntPtr obj) => StringFromNative(ovr_AssetDetails_GetIapStatus_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetDetails_GetIapStatus")] static extern IntPtr ovr_AssetDetails_GetIapStatus_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_AssetDetails_GetLanguage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_AssetDetailsArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_AssetDetailsArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDeleteResult_GetAssetFileId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDeleteResult_GetAssetId(IntPtr obj);
        public static string ovr_AssetFileDeleteResult_GetFilepath(IntPtr obj) => StringFromNative(ovr_AssetFileDeleteResult_GetFilepath_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFileDeleteResult_GetFilepath")] static extern IntPtr ovr_AssetFileDeleteResult_GetFilepath_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AssetFileDeleteResult_GetSuccess(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDownloadCancelResult_GetAssetFileId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDownloadCancelResult_GetAssetId(IntPtr obj);
        public static string ovr_AssetFileDownloadCancelResult_GetFilepath(IntPtr obj) => StringFromNative(ovr_AssetFileDownloadCancelResult_GetFilepath_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFileDownloadCancelResult_GetFilepath")] static extern IntPtr ovr_AssetFileDownloadCancelResult_GetFilepath_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AssetFileDownloadCancelResult_GetSuccess(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDownloadResult_GetAssetId(IntPtr obj);
        public static string ovr_AssetFileDownloadResult_GetFilepath(IntPtr obj) => StringFromNative(ovr_AssetFileDownloadResult_GetFilepath_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_AssetFileDownloadResult_GetFilepath")] static extern IntPtr ovr_AssetFileDownloadResult_GetFilepath_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDownloadUpdate_GetAssetFileId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_AssetFileDownloadUpdate_GetAssetId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_AssetFileDownloadUpdate_GetBytesTotal(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_AssetFileDownloadUpdate_GetBytesTransferred(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_AssetFileDownloadUpdate_GetCompleted(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_CalApplicationFinalized_GetCountdownMS(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_CalApplicationFinalized_GetID(IntPtr obj);
        public static string ovr_CalApplicationFinalized_GetLaunchDetails(IntPtr obj) => StringFromNative(ovr_CalApplicationFinalized_GetLaunchDetails_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CalApplicationFinalized_GetLaunchDetails")] static extern IntPtr ovr_CalApplicationFinalized_GetLaunchDetails_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_CalApplicationProposed_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_CalApplicationSuggestion_GetID(IntPtr obj);
        public static string ovr_CalApplicationSuggestion_GetSocialContext(IntPtr obj) => StringFromNative(ovr_CalApplicationSuggestion_GetSocialContext_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CalApplicationSuggestion_GetSocialContext")] static extern IntPtr ovr_CalApplicationSuggestion_GetSocialContext_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_CalApplicationSuggestionArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_CalApplicationSuggestionArray_GetSize(IntPtr obj);
        public static string ovr_CloudStorage2UserDirectoryPathResponse_GetPath(IntPtr obj) => StringFromNative(ovr_CloudStorage2UserDirectoryPathResponse_GetPath_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorage2UserDirectoryPathResponse_GetPath")] static extern IntPtr ovr_CloudStorage2UserDirectoryPathResponse_GetPath_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_CloudStorageConflictMetadata_GetLocal(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_CloudStorageConflictMetadata_GetRemote(IntPtr obj);
        public static string ovr_CloudStorageData_GetBucket(IntPtr obj) => StringFromNative(ovr_CloudStorageData_GetBucket_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetBucket")] static extern IntPtr ovr_CloudStorageData_GetBucket_Native(IntPtr obj);
        public static byte[] ovr_CloudStorageData_GetData(IntPtr obj) => FiledataFromNative(ovr_CloudStorageData_GetDataSize(obj), ovr_CloudStorageData_GetData_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetData")] static extern IntPtr ovr_CloudStorageData_GetData_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_CloudStorageData_GetDataSize(IntPtr obj);
        public static string ovr_CloudStorageData_GetKey(IntPtr obj) => StringFromNative(ovr_CloudStorageData_GetKey_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageData_GetKey")] static extern IntPtr ovr_CloudStorageData_GetKey_Native(IntPtr obj);
        public static string ovr_CloudStorageMetadata_GetBucket(IntPtr obj) => StringFromNative(ovr_CloudStorageMetadata_GetBucket_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetBucket")] static extern IntPtr ovr_CloudStorageMetadata_GetBucket_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern long ovr_CloudStorageMetadata_GetCounter(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_CloudStorageMetadata_GetDataSize(IntPtr obj);
        public static string ovr_CloudStorageMetadata_GetExtraData(IntPtr obj) => StringFromNative(ovr_CloudStorageMetadata_GetExtraData_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetExtraData")] static extern IntPtr ovr_CloudStorageMetadata_GetExtraData_Native(IntPtr obj);
        public static string ovr_CloudStorageMetadata_GetKey(IntPtr obj) => StringFromNative(ovr_CloudStorageMetadata_GetKey_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetKey")] static extern IntPtr ovr_CloudStorageMetadata_GetKey_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_CloudStorageMetadata_GetSaveTime(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern CloudStorageDataStatus ovr_CloudStorageMetadata_GetStatus(IntPtr obj);
        public static string ovr_CloudStorageMetadata_GetVersionHandle(IntPtr obj) => StringFromNative(ovr_CloudStorageMetadata_GetVersionHandle_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadata_GetVersionHandle")] static extern IntPtr ovr_CloudStorageMetadata_GetVersionHandle_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_CloudStorageMetadataArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_CloudStorageMetadataArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_CloudStorageMetadataArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageMetadataArray_GetNextUrl")] static extern IntPtr ovr_CloudStorageMetadataArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_CloudStorageMetadataArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_CloudStorageMetadataArray_HasNextPage(IntPtr obj);
        public static string ovr_CloudStorageUpdateResponse_GetBucket(IntPtr obj) => StringFromNative(ovr_CloudStorageUpdateResponse_GetBucket_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetBucket")] private static extern IntPtr ovr_CloudStorageUpdateResponse_GetBucket_Native(IntPtr obj);
        public static string ovr_CloudStorageUpdateResponse_GetKey(IntPtr obj) => StringFromNative(ovr_CloudStorageUpdateResponse_GetKey_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetKey")] static extern IntPtr ovr_CloudStorageUpdateResponse_GetKey_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern CloudStorageUpdateStatus ovr_CloudStorageUpdateResponse_GetStatus(IntPtr obj);
        public static string ovr_CloudStorageUpdateResponse_GetVersionHandle(IntPtr obj) => StringFromNative(ovr_CloudStorageUpdateResponse_GetVersionHandle_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_CloudStorageUpdateResponse_GetVersionHandle")] static extern IntPtr ovr_CloudStorageUpdateResponse_GetVersionHandle_Native(IntPtr obj);
        public static uint ovr_DataStore_Contains(IntPtr obj, string key)
        {
            var key_native = StringToNative(key);
            var result = ovr_DataStore_Contains_Native(obj, key_native);
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_Contains")] static extern uint ovr_DataStore_Contains_Native(IntPtr obj, IntPtr key);
        public static string ovr_DataStore_GetKey(IntPtr obj, int index) => StringFromNative(ovr_DataStore_GetKey_Native(obj, index));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_GetKey")] static extern IntPtr ovr_DataStore_GetKey_Native(IntPtr obj, int index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_DataStore_GetNumKeys(IntPtr obj);
        public static string ovr_DataStore_GetValue(IntPtr obj, string key)
        {
            var key_native = StringToNative(key);
            var result = StringFromNative(ovr_DataStore_GetValue_Native(obj, key_native));
            Marshal.FreeCoTaskMem(key_native);
            return result;
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_DataStore_GetValue")] static extern IntPtr ovr_DataStore_GetValue_Native(IntPtr obj, IntPtr key);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_Error_GetCode(IntPtr obj);
        public static string ovr_Error_GetDisplayableMessage(IntPtr obj) => StringFromNative(ovr_Error_GetDisplayableMessage_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Error_GetDisplayableMessage")] static extern IntPtr ovr_Error_GetDisplayableMessage_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_Error_GetHttpCode(IntPtr obj);
        public static string ovr_Error_GetMessage(IntPtr obj) => StringFromNative(ovr_Error_GetMessage_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Error_GetMessage")] static extern IntPtr ovr_Error_GetMessage_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_HttpTransferUpdate_GetBytes(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_HttpTransferUpdate_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_HttpTransferUpdate_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_HttpTransferUpdate_IsCompleted(IntPtr obj);
        public static string ovr_InstalledApplication_GetApplicationId(IntPtr obj) => StringFromNative(ovr_InstalledApplication_GetApplicationId_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetApplicationId")] static extern IntPtr ovr_InstalledApplication_GetApplicationId_Native(IntPtr obj);
        public static string ovr_InstalledApplication_GetPackageName(IntPtr obj) => StringFromNative(ovr_InstalledApplication_GetPackageName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetPackageName")] static extern IntPtr ovr_InstalledApplication_GetPackageName_Native(IntPtr obj);
        public static string ovr_InstalledApplication_GetStatus(IntPtr obj) => StringFromNative(ovr_InstalledApplication_GetStatus_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetStatus")] static extern IntPtr ovr_InstalledApplication_GetStatus_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_InstalledApplication_GetVersionCode(IntPtr obj);
        public static string ovr_InstalledApplication_GetVersionName(IntPtr obj) => StringFromNative(ovr_InstalledApplication_GetVersionName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_InstalledApplication_GetVersionName")] static extern IntPtr ovr_InstalledApplication_GetVersionName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_InstalledApplicationArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_InstalledApplicationArray_GetSize(IntPtr obj);
        public static string ovr_LanguagePackInfo_GetEnglishName(IntPtr obj) => StringFromNative(ovr_LanguagePackInfo_GetEnglishName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LanguagePackInfo_GetEnglishName")] static extern IntPtr ovr_LanguagePackInfo_GetEnglishName_Native(IntPtr obj);
        public static string ovr_LanguagePackInfo_GetNativeName(IntPtr obj) => StringFromNative(ovr_LanguagePackInfo_GetNativeName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LanguagePackInfo_GetNativeName")] static extern IntPtr ovr_LanguagePackInfo_GetNativeName_Native(IntPtr obj);
        public static string ovr_LanguagePackInfo_GetTag(IntPtr obj) => StringFromNative(ovr_LanguagePackInfo_GetTag_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LanguagePackInfo_GetTag")] static extern IntPtr ovr_LanguagePackInfo_GetTag_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchBlockFlowResult_GetDidBlock(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchBlockFlowResult_GetDidCancel(IntPtr obj);
        public static string ovr_LaunchDetails_GetDeeplinkMessage(IntPtr obj) => StringFromNative(ovr_LaunchDetails_GetDeeplinkMessage_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LaunchDetails_GetDeeplinkMessage")] static extern IntPtr ovr_LaunchDetails_GetDeeplinkMessage_Native(IntPtr obj);
        public static string ovr_LaunchDetails_GetLaunchSource(IntPtr obj) => StringFromNative(ovr_LaunchDetails_GetLaunchSource_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LaunchDetails_GetLaunchSource")] static extern IntPtr ovr_LaunchDetails_GetLaunchSource_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern LaunchType ovr_LaunchDetails_GetLaunchType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_LaunchDetails_GetRoomID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_LaunchDetails_GetUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchFriendRequestFlowResult_GetDidCancel(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchFriendRequestFlowResult_GetDidSendRequest(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchReportFlowResult_GetDidCancel(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_LaunchReportFlowResult_GetUserReportId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchUnblockFlowResult_GetDidCancel(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LaunchUnblockFlowResult_GetDidUnblock(IntPtr obj);
        public static byte[] ovr_LeaderboardEntry_GetExtraData(IntPtr obj) => BlobFromNative(ovr_LeaderboardEntry_GetExtraDataLength(obj), ovr_LeaderboardEntry_GetExtraData_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntry_GetExtraData")] static extern IntPtr ovr_LeaderboardEntry_GetExtraData_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_LeaderboardEntry_GetExtraDataLength(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_LeaderboardEntry_GetRank(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern long ovr_LeaderboardEntry_GetScore(IntPtr obj);
        public static DateTime ovr_LeaderboardEntry_GetTimestamp(IntPtr obj) => DateTimeFromNative(ovr_LeaderboardEntry_GetTimestamp_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntry_GetTimestamp")] static extern ulong ovr_LeaderboardEntry_GetTimestamp_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_LeaderboardEntry_GetUser(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_LeaderboardEntryArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_LeaderboardEntryArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_LeaderboardEntryArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntryArray_GetNextUrl")] static extern IntPtr ovr_LeaderboardEntryArray_GetNextUrl_Native(IntPtr obj);
        public static string ovr_LeaderboardEntryArray_GetPreviousUrl(IntPtr obj) => StringFromNative(ovr_LeaderboardEntryArray_GetPreviousUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LeaderboardEntryArray_GetPreviousUrl")] static extern IntPtr ovr_LeaderboardEntryArray_GetPreviousUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_LeaderboardEntryArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_LeaderboardEntryArray_GetTotalCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LeaderboardEntryArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LeaderboardEntryArray_HasPreviousPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LeaderboardUpdateStatus_GetDidUpdate(IntPtr obj);
        public static string ovr_LinkedAccount_GetAccessToken(IntPtr obj) => StringFromNative(ovr_LinkedAccount_GetAccessToken_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LinkedAccount_GetAccessToken")] static extern IntPtr ovr_LinkedAccount_GetAccessToken_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ServiceProvider ovr_LinkedAccount_GetServiceProvider(IntPtr obj);
        public static string ovr_LinkedAccount_GetUserId(IntPtr obj) => StringFromNative(ovr_LinkedAccount_GetUserId_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LinkedAccount_GetUserId")] static extern IntPtr ovr_LinkedAccount_GetUserId_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_LinkedAccountArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_LinkedAccountArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LivestreamingApplicationStatus_GetStreamingEnabled(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern LivestreamingStartStatus ovr_LivestreamingStartResult_GetStreamingResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LivestreamingStatus_GetCommentsVisible(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LivestreamingStatus_GetIsPaused(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LivestreamingStatus_GetLivestreamingEnabled(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_LivestreamingStatus_GetLivestreamingType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_LivestreamingStatus_GetMicEnabled(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_LivestreamingVideoStats_GetCommentCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern int ovr_LivestreamingVideoStats_GetReactionCount(IntPtr obj);
        public static string ovr_LivestreamingVideoStats_GetTotalViews(IntPtr obj) => StringFromNative(ovr_LivestreamingVideoStats_GetTotalViews_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_LivestreamingVideoStats_GetTotalViews")] static extern IntPtr ovr_LivestreamingVideoStats_GetTotalViews_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingAdminSnapshot_GetCandidates(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern double ovr_MatchmakingAdminSnapshot_GetMyCurrentThreshold(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_MatchmakingAdminSnapshotCandidate_GetCanMatch(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetMyTotalScore(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetTheirCurrentThreshold(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern double ovr_MatchmakingAdminSnapshotCandidate_GetTheirTotalScore(IntPtr obj);
        public static string ovr_MatchmakingAdminSnapshotCandidate_GetTraceId(IntPtr obj) => StringFromNative(ovr_MatchmakingAdminSnapshotCandidate_GetTraceId_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingAdminSnapshotCandidate_GetTraceId")] static extern IntPtr ovr_MatchmakingAdminSnapshotCandidate_GetTraceId_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingAdminSnapshotCandidateArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_MatchmakingAdminSnapshotCandidateArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingBrowseResult_GetEnqueueResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingBrowseResult_GetRooms(IntPtr obj);
        public static string ovr_MatchmakingCandidate_GetEntryHash(IntPtr obj) => StringFromNative(ovr_MatchmakingCandidate_GetEntryHash_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingCandidate_GetEntryHash")] static extern IntPtr ovr_MatchmakingCandidate_GetEntryHash_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_MatchmakingCandidate_GetUserId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingCandidateArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_MatchmakingCandidateArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_MatchmakingCandidateArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingCandidateArray_GetNextUrl")] static extern IntPtr ovr_MatchmakingCandidateArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_MatchmakingCandidateArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_MatchmakingCandidateArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueueResult_GetAdminSnapshot(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingEnqueueResult_GetAverageWait(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingEnqueueResult_GetMatchesInLastHourCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingEnqueueResult_GetMaxExpectedWait(IntPtr obj);
        public static string ovr_MatchmakingEnqueueResult_GetPool(IntPtr obj) => StringFromNative(ovr_MatchmakingEnqueueResult_GetPool_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingEnqueueResult_GetPool")] static extern IntPtr ovr_MatchmakingEnqueueResult_GetPool_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingEnqueueResult_GetRecentMatchPercentage(IntPtr obj);
        public static string ovr_MatchmakingEnqueueResult_GetRequestHash(IntPtr obj) => StringFromNative(ovr_MatchmakingEnqueueResult_GetRequestHash_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingEnqueueResult_GetRequestHash")] static extern IntPtr ovr_MatchmakingEnqueueResult_GetRequestHash_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueueResultAndRoom_GetMatchmakingEnqueueResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueueResultAndRoom_GetRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_MatchmakingEnqueuedUser_GetAdditionalUserID(IntPtr obj, uint index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingEnqueuedUser_GetAdditionalUserIDsSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueuedUser_GetCustomData(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueuedUser_GetUser(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingEnqueuedUserArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_MatchmakingEnqueuedUserArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_MatchmakingNotification_GetAddedByUserId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingNotification_GetRoom(IntPtr obj);
        public static string ovr_MatchmakingNotification_GetTraceId(IntPtr obj) => StringFromNative(ovr_MatchmakingNotification_GetTraceId_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingNotification_GetTraceId")] static extern IntPtr ovr_MatchmakingNotification_GetTraceId_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingRoom_GetPingTime(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingRoom_GetRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_MatchmakingRoom_HasPingTime(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingRoomArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_MatchmakingRoomArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingStats_GetDrawCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingStats_GetLossCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingStats_GetSkillLevel(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_MatchmakingStats_GetWinCount(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAbuseReportRecording(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAchievementDefinitionArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAchievementProgressArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAchievementUpdate(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetApplicationVersion(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetDetails(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetDetailsArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetFileDeleteResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetFileDownloadCancelResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetFileDownloadResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetAssetFileDownloadUpdate(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCalApplicationFinalized(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCalApplicationProposed(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCalApplicationSuggestionArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCloudStorageConflictMetadata(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCloudStorageData(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCloudStorageMetadata(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCloudStorageMetadataArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetCloudStorageUpdateResponse(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetError(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetHttpTransferUpdate(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetInstalledApplicationArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLaunchBlockFlowResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLaunchFriendRequestFlowResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLaunchReportFlowResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLaunchUnblockFlowResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLeaderboardEntryArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLeaderboardUpdateStatus(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLinkedAccountArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLivestreamingApplicationStatus(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLivestreamingStartResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLivestreamingStatus(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetLivestreamingVideoStats(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingAdminSnapshot(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingBrowseResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingEnqueueResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingEnqueueResultAndRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingRoomArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetMatchmakingStats(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetNativeMessage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetNetworkingPeer(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetOrgScopedID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetParty(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPartyID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPartyUpdateNotification(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPidArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPingResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPlatformInitialize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetProductArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPurchase(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetPurchaseArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Message_GetRequestID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetRoomArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetRoomInviteNotification(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetRoomInviteNotificationArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetSdkAccountArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetShareMediaResult(IntPtr obj);
        public static string ovr_Message_GetString(IntPtr obj) => StringFromNative(ovr_Message_GetString_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Message_GetString")] static extern IntPtr ovr_Message_GetString_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetSystemPermission(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetSystemVoipState(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern Message.MessageType ovr_Message_GetType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetUser(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetUserAndRoomArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetUserArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetUserProof(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Message_GetUserReportID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Message_IsError(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Microphone_GetNumSamplesAvailable(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Microphone_GetOutputBufferMaxSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Microphone_GetPCM(IntPtr obj, short[] outputBuffer, UIntPtr outputBufferNumElements);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Microphone_GetPCMFloat(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferNumElements);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Microphone_ReadData(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Microphone_SetAcceptableRecordingDelayHint(IntPtr obj, UIntPtr delayMs);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Microphone_Start(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Microphone_Stop(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_NetworkingPeer_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern PeerConnectionState ovr_NetworkingPeer_GetState(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_OrgScopedID_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_Packet_Free(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Packet_GetBytes(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern SendPolicy ovr_Packet_GetSendPolicy(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Packet_GetSenderID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_Packet_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Party_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Party_GetInvitedUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Party_GetLeader(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Party_GetRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Party_GetUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PartyID_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern PartyUpdateAction ovr_PartyUpdateNotification_GetAction(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PartyUpdateNotification_GetPartyId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PartyUpdateNotification_GetSenderId(IntPtr obj);
        public static string ovr_PartyUpdateNotification_GetUpdateTimestamp(IntPtr obj) => StringFromNative(ovr_PartyUpdateNotification_GetUpdateTimestamp_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_PartyUpdateNotification_GetUpdateTimestamp")] static extern IntPtr ovr_PartyUpdateNotification_GetUpdateTimestamp_Native(IntPtr obj);
        public static string ovr_PartyUpdateNotification_GetUserAlias(IntPtr obj) => StringFromNative(ovr_PartyUpdateNotification_GetUserAlias_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_PartyUpdateNotification_GetUserAlias")] static extern IntPtr ovr_PartyUpdateNotification_GetUserAlias_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PartyUpdateNotification_GetUserId(IntPtr obj);
        public static string ovr_PartyUpdateNotification_GetUserName(IntPtr obj) => StringFromNative(ovr_PartyUpdateNotification_GetUserName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_PartyUpdateNotification_GetUserName")] static extern IntPtr ovr_PartyUpdateNotification_GetUserName_Native(IntPtr obj);
        public static string ovr_Pid_GetId(IntPtr obj) => StringFromNative(ovr_Pid_GetId_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Pid_GetId")] static extern IntPtr ovr_Pid_GetId_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_PidArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_PidArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PingResult_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_PingResult_GetPingTimeUsec(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_PingResult_IsTimeout(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern PlatformInitializeResult ovr_PlatformInitialize_GetResult(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_Price_GetAmountInHundredths(IntPtr obj);
        public static string ovr_Price_GetCurrency(IntPtr obj) => StringFromNative(ovr_Price_GetCurrency_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Price_GetCurrency")] static extern IntPtr ovr_Price_GetCurrency_Native(IntPtr obj);
        public static string ovr_Price_GetFormatted(IntPtr obj) => StringFromNative(ovr_Price_GetFormatted_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Price_GetFormatted")] static extern IntPtr ovr_Price_GetFormatted_Native(IntPtr obj);
        public static string ovr_Product_GetDescription(IntPtr obj) => StringFromNative(ovr_Product_GetDescription_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetDescription")] static extern IntPtr ovr_Product_GetDescription_Native(IntPtr obj);
        public static string ovr_Product_GetFormattedPrice(IntPtr obj) => StringFromNative(ovr_Product_GetFormattedPrice_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetFormattedPrice")] static extern IntPtr ovr_Product_GetFormattedPrice_Native(IntPtr obj);
        public static string ovr_Product_GetName(IntPtr obj) => StringFromNative(ovr_Product_GetName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetName")] static extern IntPtr ovr_Product_GetName_Native(IntPtr obj);
        public static string ovr_Product_GetSKU(IntPtr obj) => StringFromNative(ovr_Product_GetSKU_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Product_GetSKU")] static extern IntPtr ovr_Product_GetSKU_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_ProductArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_ProductArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_ProductArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ProductArray_GetNextUrl")] static extern IntPtr ovr_ProductArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_ProductArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_ProductArray_HasNextPage(IntPtr obj);
        public static DateTime ovr_Purchase_GetExpirationTime(IntPtr obj) => DateTimeFromNative(ovr_Purchase_GetExpirationTime_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetExpirationTime")] static extern ulong ovr_Purchase_GetExpirationTime_Native(IntPtr obj);
        public static DateTime ovr_Purchase_GetGrantTime(IntPtr obj) => DateTimeFromNative(ovr_Purchase_GetGrantTime_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetGrantTime")] static extern ulong ovr_Purchase_GetGrantTime_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Purchase_GetPurchaseID(IntPtr obj);
        public static string ovr_Purchase_GetSKU(IntPtr obj) => StringFromNative(ovr_Purchase_GetSKU_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Purchase_GetSKU")] static extern IntPtr ovr_Purchase_GetSKU_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_PurchaseArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_PurchaseArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_PurchaseArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_PurchaseArray_GetNextUrl")] static extern IntPtr ovr_PurchaseArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_PurchaseArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_PurchaseArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetApplicationID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Room_GetDataStore(IntPtr obj);
        public static string ovr_Room_GetDescription(IntPtr obj) => StringFromNative(ovr_Room_GetDescription_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_GetDescription")] static extern IntPtr ovr_Room_GetDescription_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_Room_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Room_GetInvitedUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_Room_GetIsMembershipLocked(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern RoomJoinPolicy ovr_Room_GetJoinPolicy(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern RoomJoinability ovr_Room_GetJoinability(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Room_GetMatchedUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_Room_GetMaxUsers(IntPtr obj);
        public static string ovr_Room_GetName(IntPtr obj) => StringFromNative(ovr_Room_GetName_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_Room_GetName")] static extern IntPtr ovr_Room_GetName_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Room_GetOwner(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern RoomType ovr_Room_GetType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_Room_GetUsers(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern uint ovr_Room_GetVersion(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_RoomArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_RoomArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_RoomArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomArray_GetNextUrl")] static extern IntPtr ovr_RoomArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_RoomArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_RoomArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_RoomInviteNotification_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_RoomInviteNotification_GetRoomID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_RoomInviteNotification_GetSenderID(IntPtr obj);
        public static DateTime ovr_RoomInviteNotification_GetSentTime(IntPtr obj) => DateTimeFromNative(ovr_RoomInviteNotification_GetSentTime_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomInviteNotification_GetSentTime")] static extern ulong ovr_RoomInviteNotification_GetSentTime_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_RoomInviteNotificationArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_RoomInviteNotificationArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_RoomInviteNotificationArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomInviteNotificationArray_GetNextUrl")]
        static extern IntPtr ovr_RoomInviteNotificationArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_RoomInviteNotificationArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_RoomInviteNotificationArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern SdkAccountType ovr_SdkAccount_GetAccountType(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_SdkAccount_GetUserId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_SdkAccountArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_SdkAccountArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ShareMediaStatus ovr_ShareMediaResult_GetStatus(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_SystemPermission_GetHasPermission(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern PermissionGrantStatus ovr_SystemPermission_GetPermissionGrantStatus(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern VoipMuteState ovr_SystemVoipState_GetMicrophoneMuted(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern SystemVoipStatus ovr_SystemVoipState_GetStatus(IntPtr obj);
        public static string ovr_TestUser_GetAccessToken(IntPtr obj) => StringFromNative(ovr_TestUser_GetAccessToken_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetAccessToken")] static extern IntPtr ovr_TestUser_GetAccessToken_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_TestUser_GetAppAccessArray(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_TestUser_GetFbAppAccessArray(IntPtr obj);
        public static string ovr_TestUser_GetFriendAccessToken(IntPtr obj) => StringFromNative(ovr_TestUser_GetFriendAccessToken_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetFriendAccessToken")] static extern IntPtr ovr_TestUser_GetFriendAccessToken_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_TestUser_GetFriendAppAccessArray(IntPtr obj);
        public static string ovr_TestUser_GetUserAlias(IntPtr obj) => StringFromNative(ovr_TestUser_GetUserAlias_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUser_GetUserAlias")] static extern IntPtr ovr_TestUser_GetUserAlias_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_TestUser_GetUserFbid(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_TestUser_GetUserId(IntPtr obj);
        public static string ovr_TestUserAppAccess_GetAccessToken(IntPtr obj) => StringFromNative(ovr_TestUserAppAccess_GetAccessToken_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_TestUserAppAccess_GetAccessToken")] static extern IntPtr ovr_TestUserAppAccess_GetAccessToken_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_TestUserAppAccess_GetAppId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_TestUserAppAccess_GetUserId(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_TestUserAppAccessArray_GetElement(IntPtr obj, UIntPtr index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_TestUserAppAccessArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_User_GetID(IntPtr obj);
        public static string ovr_User_GetImageUrl(IntPtr obj) => StringFromNative(ovr_User_GetImageUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetImageUrl")] static extern IntPtr ovr_User_GetImageUrl_Native(IntPtr obj);
        public static string ovr_User_GetInviteToken(IntPtr obj) => StringFromNative(ovr_User_GetInviteToken_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetInviteToken")] static extern IntPtr ovr_User_GetInviteToken_Native(IntPtr obj);
        public static string ovr_User_GetOculusID(IntPtr obj) => StringFromNative(ovr_User_GetOculusID_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetOculusID")] static extern IntPtr ovr_User_GetOculusID_Native(IntPtr obj);
        public static string ovr_User_GetPresence(IntPtr obj) => StringFromNative(ovr_User_GetPresence_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetPresence")] static extern IntPtr ovr_User_GetPresence_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UserPresenceStatus ovr_User_GetPresenceStatus(IntPtr obj);
        public static string ovr_User_GetSmallImageUrl(IntPtr obj) => StringFromNative(ovr_User_GetSmallImageUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_User_GetSmallImageUrl")] static extern IntPtr ovr_User_GetSmallImageUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_UserAndRoom_GetRoom(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_UserAndRoom_GetUser(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_UserAndRoomArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_UserAndRoomArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_UserAndRoomArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserAndRoomArray_GetNextUrl")] static extern IntPtr ovr_UserAndRoomArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_UserAndRoomArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UserAndRoomArray_HasNextPage(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_UserArray_GetElement(IntPtr obj, UIntPtr index);
        public static string ovr_UserArray_GetNextUrl(IntPtr obj) => StringFromNative(ovr_UserArray_GetNextUrl_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserArray_GetNextUrl")] static extern IntPtr ovr_UserArray_GetNextUrl_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_UserArray_GetSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UserArray_HasNextPage(IntPtr obj);
        public static string ovr_UserProof_GetNonce(IntPtr obj) => StringFromNative(ovr_UserProof_GetNonce_Native(obj));
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_UserProof_GetNonce")] static extern IntPtr ovr_UserProof_GetNonce_Native(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern bool ovr_UserReportID_GetDidCancel(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern ulong ovr_UserReportID_GetID(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipDecoder_Decode(IntPtr obj, byte[] compressedData, UIntPtr compressedSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_VoipDecoder_GetDecodedPCM(IntPtr obj, float[] outputBuffer, UIntPtr outputBufferSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipEncoder_AddPCM(IntPtr obj, float[] inputData, uint inputSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_VoipEncoder_GetCompressedData(IntPtr obj, byte[] outputBuffer, UIntPtr intputSize);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern UIntPtr ovr_VoipEncoder_GetCompressedDataSize(IntPtr obj);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_AbuseReportOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_AbuseReportOptions_Destroy(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_AbuseReportOptions_SetPreventPeopleChooser(IntPtr handle, bool value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_AbuseReportOptions_SetReportType(IntPtr handle, AbuseReportType value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_ApplicationOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_ApplicationOptions_Destroy(IntPtr handle);
        public static void ovr_ApplicationOptions_SetDeeplinkMessage(IntPtr handle, string value)
        {
            var value_native = StringToNative(value);
            ovr_ApplicationOptions_SetDeeplinkMessage_Native(handle, value_native);
            Marshal.FreeCoTaskMem(value_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_ApplicationOptions_SetDeeplinkMessage")] static extern void ovr_ApplicationOptions_SetDeeplinkMessage_Native(IntPtr handle, IntPtr value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_MatchmakingOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_Destroy(IntPtr handle);
        public static void ovr_MatchmakingOptions_SetCreateRoomDataStoreString(IntPtr handle, string key, string value)
        {
            var key_native = StringToNative(key);
            var value_native = StringToNative(value);
            ovr_MatchmakingOptions_SetCreateRoomDataStoreString_Native(handle, key_native, value_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(value_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetCreateRoomDataStoreString")] static extern void ovr_MatchmakingOptions_SetCreateRoomDataStoreString_Native(IntPtr handle, IntPtr key, IntPtr value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_ClearCreateRoomDataStore(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_SetCreateRoomJoinPolicy(IntPtr handle, RoomJoinPolicy value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_SetCreateRoomMaxUsers(IntPtr handle, uint value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_AddEnqueueAdditionalUser(IntPtr handle, ulong value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_ClearEnqueueAdditionalUsers(IntPtr handle);
        public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsInt(IntPtr handle, string key, int value)
        {
            var key_native = StringToNative(key);
            ovr_MatchmakingOptions_SetEnqueueDataSettingsInt_Native(handle, key_native, value);
            Marshal.FreeCoTaskMem(key_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsInt")] static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsInt_Native(IntPtr handle, IntPtr key, int value);
        public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble(IntPtr handle, string key, double value)
        {
            var key_native = StringToNative(key);
            ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble_Native(handle, key_native, value);
            Marshal.FreeCoTaskMem(key_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble")] static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsDouble_Native(IntPtr handle, IntPtr key, double value);
        public static void ovr_MatchmakingOptions_SetEnqueueDataSettingsString(IntPtr handle, string key, string value)
        {
            IntPtr key_native = StringToNative(key);
            IntPtr value_native = StringToNative(value);
            ovr_MatchmakingOptions_SetEnqueueDataSettingsString_Native(handle, key_native, value_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(value_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueDataSettingsString")] static extern void ovr_MatchmakingOptions_SetEnqueueDataSettingsString_Native(IntPtr handle, IntPtr key, IntPtr value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_ClearEnqueueDataSettings(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_MatchmakingOptions_SetEnqueueIsDebug(IntPtr handle, bool value);
        public static void ovr_MatchmakingOptions_SetEnqueueQueryKey(IntPtr handle, string value)
        {
            var value_native = StringToNative(value);
            ovr_MatchmakingOptions_SetEnqueueQueryKey_Native(handle, value_native);
            Marshal.FreeCoTaskMem(value_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_MatchmakingOptions_SetEnqueueQueryKey")] static extern void ovr_MatchmakingOptions_SetEnqueueQueryKey_Native(IntPtr handle, IntPtr value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_RoomOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_Destroy(IntPtr handle);
        public static void ovr_RoomOptions_SetDataStoreString(IntPtr handle, string key, string value)
        {
            var key_native = StringToNative(key);
            var value_native = StringToNative(value);
            ovr_RoomOptions_SetDataStoreString_Native(handle, key_native, value_native);
            Marshal.FreeCoTaskMem(key_native);
            Marshal.FreeCoTaskMem(value_native);
        }
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovr_RoomOptions_SetDataStoreString")] static extern void ovr_RoomOptions_SetDataStoreString_Native(IntPtr handle, IntPtr key, IntPtr value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_ClearDataStore(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetExcludeRecentlyMet(IntPtr handle, bool value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetMaxUserResults(IntPtr handle, uint value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetOrdering(IntPtr handle, UserOrdering value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetRecentlyMetTimeWindow(IntPtr handle, TimeWindow value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetRoomId(IntPtr handle, ulong value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_RoomOptions_SetTurnOffUpdates(IntPtr handle, bool value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_UserOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UserOptions_Destroy(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UserOptions_SetMaxUsers(IntPtr handle, uint value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UserOptions_AddServiceProvider(IntPtr handle, ServiceProvider value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UserOptions_ClearServiceProviders(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_UserOptions_SetTimeWindow(IntPtr handle, TimeWindow value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovr_VoipOptions_Create();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipOptions_Destroy(IntPtr handle);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipOptions_SetBitrateForNewConnections(IntPtr handle, VoipBitrate value);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)] public static extern void ovr_VoipOptions_SetCreateNewConnectionUseDtx(IntPtr handle, VoipDtxState value);
    }
}
