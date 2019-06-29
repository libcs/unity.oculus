namespace Oculus.Platform
{
    using UnityEngine;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public sealed class PlatformSettings : ScriptableObject
    {
        public static string AppID
        {
            get => Instance.ovrAppID;
            set => Instance.ovrAppID = value;
        }

        public static string MobileAppID
        {
            get => Instance.ovrMobileAppID;
            set => Instance.ovrMobileAppID = value;
        }

        public static bool UseStandalonePlatform
        {
            get => Instance.ovrUseStandalonePlatform;
            set => Instance.ovrUseStandalonePlatform = value;
        }

        public static bool EnableARM64Support
        {
            get => Instance.ovrEnableARM64Support;
            set => Instance.ovrEnableARM64Support = value;
        }

        [SerializeField] string ovrAppID = "";

        [SerializeField] string ovrMobileAppID = "";

#if UNITY_EDITOR_WIN
        [SerializeField] bool ovrUseStandalonePlatform = false;
#else
        [SerializeField] bool ovrUseStandalonePlatform = true;
#endif

        [SerializeField] bool ovrEnableARM64Support = false;

        static PlatformSettings instance;
        public static PlatformSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<PlatformSettings>("OculusPlatformSettings");
                    // This can happen if the developer never input their App Id into the Unity Editor
                    // and therefore never created the OculusPlatformSettings.asset file
                    // Use a dummy object with defaults for the getters so we don't have a null pointer exception
                    if (instance == null)
                    {
                        instance = CreateInstance<PlatformSettings>();
                        //if (UnityEngine.Application.isEditor)
                        //{
                        //    // Only in the editor should we save it to disk
                        //    var properPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources");
                        //    if (!System.IO.Directory.Exists(properPath))
                        //        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        //    var fullPath = System.IO.Path.Combine(System.IO.Path.Combine("Assets", "Resources"), "OculusPlatformSettings.asset");
                        //    UnityEditor.AssetDatabase.CreateAsset(instance, fullPath);
                        //}
                    }
                }
                return instance;
            }
            set => instance = value;
        }
    }
}
