namespace Oculus.Platform
{
    using UnityEngine;

    public class AndroidPlatform
    {
        public bool Initialize(string appId)
        {
            if (UnityApplication.platform != RuntimePlatform.Android)
                return false;
            if (string.IsNullOrEmpty(appId))
                throw new UnityException("AppID must not be null or empty");
            return CAPI.ovr_UnityInitWrapper(appId);
        }

        public Request<Models.PlatformInitialize> AsyncInitialize(string appId)
        {
            if (UnityApplication.platform != RuntimePlatform.Android)
                return new Request<Models.PlatformInitialize>(0);
            if (string.IsNullOrEmpty(appId))
                throw new UnityException("AppID must not be null or empty");
            return new Request<Models.PlatformInitialize>(CAPI.ovr_UnityInitWrapperAsynchronous(appId));
        }
    }
}
