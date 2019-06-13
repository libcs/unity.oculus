//using System.Threading.Tasks;
//using UnityEngine;

//namespace Oculus.Platform
//{
//    internal static class UnityApplication
//    {
//        public static readonly RuntimePlatform platform;
//        public static bool isEditor;

//        static UnityApplication()
//        {
//            var task = Task.Run(() => UnityEngine.Application.platform);
//            try
//            {
//                platform = task.Result;
//                isEditor = UnityEngine.Application.isEditor;
//                return;
//            }
//            catch
//            {
//                platform = RuntimePlatform.NaCl;
//            }
//        }
//    }
//}