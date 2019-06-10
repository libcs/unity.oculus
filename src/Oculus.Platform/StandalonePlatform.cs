namespace Oculus.Platform
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public sealed class StandalonePlatform
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UnityLogDelegate(IntPtr tag, IntPtr msg);

        public Request<Models.PlatformInitialize> InitializeInEditor()
        {
            string appID;
            if (UnityApplication.platform == RuntimePlatform.Android)
            {
                if (string.IsNullOrEmpty(PlatformSettings.MobileAppID))
                    throw new UnityException("Update your App ID by selecting 'Oculus Platform' -> 'Edit Settings'");
                appID = PlatformSettings.MobileAppID;
            }
            else
            {
                if (string.IsNullOrEmpty(PlatformSettings.AppID))
                    throw new UnityException("Update your App ID by selecting 'Oculus Platform' -> 'Edit Settings'");
                appID = PlatformSettings.AppID;
            }
            if (string.IsNullOrEmpty(StandalonePlatformSettings.OculusPlatformTestUserAccessToken))
                throw new UnityException("Update your standalone credentials by selecting 'Oculus Platform' -> 'Edit Settings'");
            var accessToken = StandalonePlatformSettings.OculusPlatformTestUserAccessToken;

            CAPI.ovr_UnityResetTestPlatform();
            CAPI.ovr_UnityInitGlobals(IntPtr.Zero);

            return new Request<Models.PlatformInitialize>(CAPI.ovr_PlatformInitializeWithAccessToken(ulong.Parse(appID), accessToken));
        }
    }
}
