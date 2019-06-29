namespace Oculus.Platform
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class WindowsPlatform
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void UnityLogDelegate(IntPtr tag, IntPtr msg);

        void CPPLogCallback(IntPtr tag, IntPtr message) => Debug.Log(string.Format("{0}: {1}", Marshal.PtrToStringAnsi(tag), Marshal.PtrToStringAnsi(message)));

        IntPtr getCallbackPointer() => IntPtr.Zero; // Marshal.GetFunctionPointerForDelegate(new UnityLogDelegate(CPPLogCallback));

        public bool Initialize(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                throw new UnityException("AppID must not be null or empty");
            CAPI.ovr_UnityInitWrapperWindows(appId, getCallbackPointer());
            return true;
        }

        public Request<Models.PlatformInitialize> AsyncInitialize(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                throw new UnityException("AppID must not be null or empty");
            return new Request<Models.PlatformInitialize>(CAPI.ovr_UnityInitWrapperWindowsAsynchronous(appId, getCallbackPointer()));
        }
    }
}
