/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus Utilities SDK License Version 1.31 (the "License"); you may not use
the Utilities SDK except in compliance with the License, which is provided at the time of installation
or download, or which otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at
https://developer.oculus.com/licenses/utilities-1.31

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Settings = UnityEngine.XR.XRSettings;
using Node = UnityEngine.XR.XRNode;

/// <summary>
/// Configuration data for Oculus virtual reality.
/// </summary>
public class OVRManager : MonoBehaviour
{
    public enum TrackingOrigin
    {
        EyeLevel = OVRPlugin.TrackingOrigin.EyeLevel,
        FloorLevel = OVRPlugin.TrackingOrigin.FloorLevel,
        Stage = OVRPlugin.TrackingOrigin.Stage,
    }

    public enum EyeTextureFormat
    {
        Default = OVRPlugin.EyeTextureFormat.Default,
        R16G16B16A16_FP = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP,
        R11G11B10_FP = OVRPlugin.EyeTextureFormat.R11G11B10_FP,
    }

    public enum TiledMultiResLevel
    {
        Off = OVRPlugin.TiledMultiResLevel.Off,
        LMSLow = OVRPlugin.TiledMultiResLevel.LMSLow,
        LMSMedium = OVRPlugin.TiledMultiResLevel.LMSMedium,
        LMSHigh = OVRPlugin.TiledMultiResLevel.LMSHigh,
        LMSHighTop = OVRPlugin.TiledMultiResLevel.LMSHighTop,
    }

    public enum XRDevice
    {
        Unknown = 0,
        Oculus = 1,
        OpenVR = 2,
    }

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static OVRManager instance { get; private set; }

    /// <summary>
    /// Gets a reference to the active display.
    /// </summary>
    public static OVRDisplay display { get; private set; }

    /// <summary>
    /// Gets a reference to the active sensor.
    /// </summary>
    public static OVRTracker tracker { get; private set; }

    /// <summary>
    /// Gets a reference to the active boundary system.
    /// </summary>
    public static OVRBoundary boundary { get; private set; }

    static OVRProfile _profile;
    /// <summary>
    /// Gets the current profile, which contains information about the user's settings and body dimensions.
    /// </summary>
    public static OVRProfile profile
    {
        get
        {
            if (_profile == null)
                _profile = new OVRProfile();
            return _profile;
        }
    }

    IEnumerable<Camera> disabledCameras;
    float prevTimeScale;

    /// <summary>
    /// Occurs when an HMD attached.
    /// </summary>
    public static event Action HMDAcquired;

    /// <summary>
    /// Occurs when an HMD detached.
    /// </summary>
    public static event Action HMDLost;

    /// <summary>
    /// Occurs when an HMD is put on the user's head.
    /// </summary>
    public static event Action HMDMounted;

    /// <summary>
    /// Occurs when an HMD is taken off the user's head.
    /// </summary>
    public static event Action HMDUnmounted;

    /// <summary>
    /// Occurs when VR Focus is acquired.
    /// </summary>
    public static event Action VrFocusAcquired;

    /// <summary>
    /// Occurs when VR Focus is lost.
    /// </summary>
    public static event Action VrFocusLost;

    /// <summary>
    /// Occurs when Input Focus is acquired.
    /// </summary>
    public static event Action InputFocusAcquired;

    /// <summary>
    /// Occurs when Input Focus is lost.
    /// </summary>
    public static event Action InputFocusLost;

    /// <summary>
    /// Occurs when the active Audio Out device has changed and a restart is needed.
    /// </summary>
    public static event Action AudioOutChanged;

    /// <summary>
    /// Occurs when the active Audio In device has changed and a restart is needed.
    /// </summary>
    public static event Action AudioInChanged;

    /// <summary>
    /// Occurs when the sensor gained tracking.
    /// </summary>
    public static event Action TrackingAcquired;

    /// <summary>
    /// Occurs when the sensor lost tracking.
    /// </summary>
    public static event Action TrackingLost;

    /// <summary>
    /// Occurs when Health & Safety Warning is dismissed.
    /// </summary>
    //Disable the warning about it being unused. It's deprecated.
#pragma warning disable 0067
    [Obsolete]
    public static event Action HSWDismissed;
#pragma warning restore

    static bool _isHmdPresentCached = false;
    static bool _isHmdPresent = false;
    static bool _wasHmdPresent = false;
    /// <summary>
    /// If true, a head-mounted display is connected and present.
    /// </summary>
    public static bool isHmdPresent
    {
        get
        {
            if (!_isHmdPresentCached)
            {
                _isHmdPresentCached = true;
                _isHmdPresent = OVRNodeStateProperties.IsHmdPresent();
            }
            return _isHmdPresent;
        }
        private set
        {
            _isHmdPresentCached = true;
            _isHmdPresent = value;
        }
    }

    /// <summary>
    /// Gets the audio output device identifier.
    /// </summary>
    /// <description>
    /// On Windows, this is a string containing the GUID of the IMMDevice for the Windows audio endpoint to use.
    /// </description>
    public static string audioOutId => OVRPlugin.audioOutId;

    /// <summary>
    /// Gets the audio input device identifier.
    /// </summary>
    /// <description>
    /// On Windows, this is a string containing the GUID of the IMMDevice for the Windows audio endpoint to use.
    /// </description>
    public static string audioInId => OVRPlugin.audioInId;

    static bool _hasVrFocusCached = false;
    static bool _hasVrFocus = false;
    static bool _hadVrFocus = false;
    /// <summary>
    /// If true, the app has VR Focus.
    /// </summary>
    public static bool hasVrFocus
    {
        get
        {
            if (!_hasVrFocusCached)
            {
                _hasVrFocusCached = true;
                _hasVrFocus = OVRPlugin.hasVrFocus;
            }
            return _hasVrFocus;
        }
        private set
        {
            _hasVrFocusCached = true;
            _hasVrFocus = value;
        }
    }

    static bool _hadInputFocus = true;
    /// <summary>
    /// If true, the app has Input Focus.
    /// </summary>
    public static bool hasInputFocus => OVRPlugin.hasInputFocus;

    /// <summary>
    /// If true, chromatic de-aberration will be applied, improving the image at the cost of texture bandwidth.
    /// </summary>
    public bool chromatic
    {
        get => !isHmdPresent ? false : OVRPlugin.chromatic;
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.chromatic = value;
        }
    }

    /// <summary>
    /// If true, distortion rendering work is submitted a quarter-frame early to avoid pipeline stalls and increase CPU-GPU parallelism.
    /// </summary>
    [Header("Performance/Quality"), Tooltip("If true, distortion rendering work is submitted a quarter-frame early to avoid pipeline stalls and increase CPU-GPU parallelism.")] public bool queueAhead = true;

    /// <summary>
    /// If true, Unity will use the optimal antialiasing level for quality/performance on the current hardware.
    /// </summary>
    [Tooltip("If true, Unity will use the optimal antialiasing level for quality/performance on the current hardware.")] public bool useRecommendedMSAALevel = false;

    /// <summary>
    /// If true, both eyes will see the same image, rendered from the center eye pose, saving performance.
    /// </summary>
    [SerializeField, Tooltip("If true, both eyes will see the same image, rendered from the center eye pose, saving performance.")] bool _monoscopic = false;

    public bool monoscopic
    {
        get => !isHmdPresent ? _monoscopic : OVRPlugin.monoscopic;
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.monoscopic = value;
            _monoscopic = value;
        }
    }

    /// <summary>
    /// If true, dynamic resolution will be enabled
    /// </summary>
    [Tooltip("If true, dynamic resolution will be enabled On PC")] public bool enableAdaptiveResolution = false;

    /// <summary>
    /// Adaptive Resolution is based on Unity engine's renderViewportScale/eyeTextureResolutionScale feature
    /// But renderViewportScale was broken in an array of Unity engines, this function help to filter out those broken engines
    /// </summary>
    public static bool IsAdaptiveResSupportedByEngine() => Application.unityVersion != "2017.1.0f1";

    /// <summary>
    /// Min RenderScale the app can reach under adaptive resolution mode ( enableAdaptiveResolution = true );
    /// </summary>
    [Range(0.5f, 2.0f), Tooltip("Min RenderScale the app can reach under adaptive resolution mode")] public float minRenderScale = 0.7f;

    /// <summary>
    /// Max RenderScale the app can reach under adaptive resolution mode ( enableAdaptiveResolution = true );
    /// </summary>
    [Range(0.5f, 2.0f), Tooltip("Max RenderScale the app can reach under adaptive resolution mode")] public float maxRenderScale = 1.0f;

    /// <summary>
    /// Set the relative offset rotation of head poses
    /// </summary>
    [SerializeField, Tooltip("Set the relative offset rotation of head poses")] Vector3 _headPoseRelativeOffsetRotation;
    public Vector3 headPoseRelativeOffsetRotation
    {
        get => _headPoseRelativeOffsetRotation;
        set
        {
            if (OVRPlugin.GetHeadPoseModifier(out var rotation, out var translation))
            {
                var finalRotation = Quaternion.Euler(value);
                rotation = finalRotation.ToQuatf();
                OVRPlugin.SetHeadPoseModifier(ref rotation, ref translation);
            }
            _headPoseRelativeOffsetRotation = value;
        }
    }

    /// <summary>
    /// Set the relative offset translation of head poses
    /// </summary>
    [SerializeField, Tooltip("Set the relative offset translation of head poses")] Vector3 _headPoseRelativeOffsetTranslation;
    public Vector3 headPoseRelativeOffsetTranslation
    {
        get => _headPoseRelativeOffsetTranslation;
        set
        {
            if (OVRPlugin.GetHeadPoseModifier(out var rotation, out var translation))
                if (translation.FromFlippedZVector3f() != value)
                {
                    translation = value.ToFlippedZVector3f();
                    OVRPlugin.SetHeadPoseModifier(ref rotation, ref translation);
                }
            _headPoseRelativeOffsetTranslation = value;
        }
    }

    /// <summary>
    /// The TCP listening port of Oculus Profiler Service, which will be activated in Debug/Developerment builds
    /// When the app is running on editor or device, open "Tools/Oculus/Oculus Profiler Panel" to view the realtime system metrics
    /// </summary>
    public int profilerTcpPort = OVRSystemPerfMetrics.TcpListeningPort;

#if _WIN
    /// <summary>
    /// If true, the MixedRealityCapture properties will be displayed
    /// </summary>
    [HideInInspector] public bool expandMixedRealityCapturePropertySheet = false;

    /// <summary>
    /// If true, Mixed Reality mode will be enabled
    /// </summary>
    [HideInInspector, Tooltip("If true, Mixed Reality mode will be enabled. It would be always set to false when the game is launching without editor")] public bool enableMixedReality = false;

    public enum CompositionMethod
    {
        External,
        Direct,
        Sandwich
    }

    /// <summary>
    /// Composition method
    /// </summary>
    [HideInInspector] public CompositionMethod compositionMethod = CompositionMethod.External;

    /// <summary>
    /// Extra hidden layers
    /// </summary>
    [HideInInspector, Tooltip("Extra hidden layers")] public LayerMask extraHiddenLayers;

    /// <summary>
    /// If true, Mixed Reality mode will use direct composition from the first web camera
    /// </summary>
    public enum CameraDevice
    {
        WebCamera0,
        WebCamera1,
        ZEDCamera
    }

    /// <summary>
    /// The camera device for direct composition
    /// </summary>
    [HideInInspector, Tooltip("The camera device for direct composition")] public CameraDevice capturingCameraDevice = CameraDevice.WebCamera0;

    /// <summary>
    /// Flip the camera frame horizontally
    /// </summary>
    [HideInInspector, Tooltip("Flip the camera frame horizontally")] public bool flipCameraFrameHorizontally = false;

    /// <summary>
    /// Flip the camera frame vertically
    /// </summary>
    [HideInInspector, Tooltip("Flip the camera frame vertically")] public bool flipCameraFrameVertically = false;

    /// <summary>
    /// Delay the touch controller pose by a short duration (0 to 0.5 second) to match the physical camera latency
    /// </summary>
    [HideInInspector, Tooltip("Delay the touch controller pose by a short duration (0 to 0.5 second) to match the physical camera latency")] public float handPoseStateLatency = 0.0f;

    /// <summary>
    /// Delay the foreground / background image in the sandwich composition to match the physical camera latency. The maximum duration is sandwichCompositionBufferedFrames / {Game FPS}
    /// </summary>
    [HideInInspector, Tooltip("Delay the foreground / background image in the sandwich composition to match the physical camera latency. The maximum duration is sandwichCompositionBufferedFrames / {Game FPS}")] public float sandwichCompositionRenderLatency = 0.0f;

    /// <summary>
    /// The number of frames are buffered in the SandWich composition. The more buffered frames, the more memory it would consume.
    /// </summary>
    [HideInInspector, Tooltip("The number of frames are buffered in the SandWich composition. The more buffered frames, the more memory it would consume.")] public int sandwichCompositionBufferedFrames = 8;

    /// <summary>
    /// Chroma Key Color
    /// </summary>
    [HideInInspector, Tooltip("Chroma Key Color")] public Color chromaKeyColor = Color.green;

    /// <summary>
    /// Chroma Key Similarity
    /// </summary>
    [HideInInspector, Tooltip("Chroma Key Similarity")] public float chromaKeySimilarity = 0.60f;

    /// <summary>
    /// Chroma Key Smooth Range
    /// </summary>
    [HideInInspector, Tooltip("Chroma Key Smooth Range")] public float chromaKeySmoothRange = 0.03f;

    /// <summary>
    ///  Chroma Key Spill Range
    /// </summary>
    [HideInInspector, Tooltip("Chroma Key Spill Range")] public float chromaKeySpillRange = 0.06f;

    /// <summary>
    /// Use dynamic lighting (Depth sensor required)
    /// </summary>
    [HideInInspector, Tooltip("Use dynamic lighting (Depth sensor required)")] public bool useDynamicLighting = false;

    public enum DepthQuality
    {
        Low,
        Medium,
        High
    }
    /// <summary>
    /// The quality level of depth image. The lighting could be more smooth and accurate with high quality depth, but it would also be more costly in performance.
    /// </summary>
    [HideInInspector, Tooltip("The quality level of depth image. The lighting could be more smooth and accurate with high quality depth, but it would also be more costly in performance.")] public DepthQuality depthQuality = DepthQuality.Medium;

    /// <summary>
    /// Smooth factor in dynamic lighting. Larger is smoother
    /// </summary>
    [HideInInspector, Tooltip("Smooth factor in dynamic lighting. Larger is smoother")] public float dynamicLightingSmoothFactor = 8.0f;

    /// <summary>
    /// The maximum depth variation across the edges. Make it smaller to smooth the lighting on the edges.
    /// </summary>
    [HideInInspector, Tooltip("The maximum depth variation across the edges. Make it smaller to smooth the lighting on the edges.")] public float dynamicLightingDepthVariationClampingValue = 0.001f;

    public enum VirtualGreenScreenType
    {
        Off,
        OuterBoundary,
        PlayArea
    }

    /// <summary>
    /// Set the current type of the virtual green screen
    /// </summary>
    [HideInInspector, Tooltip("Type of virutal green screen ")] public VirtualGreenScreenType virtualGreenScreenType = VirtualGreenScreenType.Off;

    /// <summary>
    /// Top Y of virtual screen
    /// </summary>
    [HideInInspector, Tooltip("Top Y of virtual green screen")] public float virtualGreenScreenTopY = 10.0f;

    /// <summary>
    /// Bottom Y of virtual screen
    /// </summary>
    [HideInInspector, Tooltip("Bottom Y of virtual green screen")] public float virtualGreenScreenBottomY = -10.0f;

    /// <summary>
    /// When using a depth camera (e.g. ZED), whether to use the depth in virtual green screen culling.
    /// </summary>
    [HideInInspector, Tooltip("When using a depth camera (e.g. ZED), whether to use the depth in virtual green screen culling.")] public bool virtualGreenScreenApplyDepthCulling = false;

    /// <summary>
    /// The tolerance value (in meter) when using the virtual green screen with a depth camera. Make it bigger if the foreground objects got culled incorrectly.
    /// </summary>
    [HideInInspector, Tooltip("The tolerance value (in meter) when using the virtual green screen with a depth camera. Make it bigger if the foreground objects got culled incorrectly.")] public float virtualGreenScreenDepthTolerance = 0.2f;
#endif

    /// <summary>
    /// The number of expected display frames per rendered frame.
    /// </summary>
    public int vsyncCount
    {
        get => !isHmdPresent ? 1 : OVRPlugin.vsyncCount;
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.vsyncCount = value;
        }
    }

    public static string OCULUS_UNITY_NAME_STR = "Oculus";
    public static string OPENVR_UNITY_NAME_STR = "OpenVR";

    public static XRDevice loadedXRDevice;

    /// <summary>
    /// Gets the current battery level.
    /// </summary>
    /// <returns><c>battery level in the range [0.0,1.0]</c>
    /// <param name="batteryLevel">Battery level.</param>
    public static float batteryLevel => !isHmdPresent ? 1f : OVRPlugin.batteryLevel;

    /// <summary>
    /// Gets the current battery temperature.
    /// </summary>
    /// <returns><c>battery temperature in Celsius</c>
    /// <param name="batteryTemperature">Battery temperature.</param>
    public static float batteryTemperature => !isHmdPresent ? 0f : OVRPlugin.batteryTemperature;

    /// <summary>
    /// Gets the current battery status.
    /// </summary>
    /// <returns><c>battery status</c>
    /// <param name="batteryStatus">Battery status.</param>
    public static int batteryStatus => !isHmdPresent ? -1 : (int)OVRPlugin.batteryStatus;

    /// <summary>
    /// Gets the current volume level.
    /// </summary>
    /// <returns><c>volume level in the range [0,1].</c>
    public static float volumeLevel => !isHmdPresent ? 0f : OVRPlugin.systemVolume;

    /// <summary>
    /// Gets or sets the current CPU performance level (0-2). Lower performance levels save more power.
    /// </summary>
    public static int cpuLevel
    {
        get => !isHmdPresent ? 2 : OVRPlugin.cpuLevel;
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.cpuLevel = value;
        }
    }

    /// <summary>
    /// Gets or sets the current GPU performance level (0-2). Lower performance levels save more power.
    /// </summary>
    public static int gpuLevel
    {
        get => !isHmdPresent ? 2 : OVRPlugin.gpuLevel;
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.gpuLevel = value;
        }
    }

    /// <summary>
    /// If true, the CPU and GPU are currently throttled to save power and/or reduce the temperature.
    /// </summary>
    public static bool isPowerSavingActive => !isHmdPresent ? false : OVRPlugin.powerSaving;

    /// <summary>
    /// Gets or sets the eye texture format.
    /// </summary>
    public static EyeTextureFormat eyeTextureFormat
    {
        get => (EyeTextureFormat)OVRPlugin.GetDesiredEyeTextureFormat();
        set => OVRPlugin.SetDesiredEyeTextureFormat((OVRPlugin.EyeTextureFormat)value);
    }

    /// <summary>
    /// Gets if tiled-based multi-resolution technique is supported
    /// This feature is only supported on QCOMM-based Android devices
    /// </summary>
    public static bool tiledMultiResSupported => OVRPlugin.tiledMultiResSupported;

    /// <summary>
    /// Gets or sets the tiled-based multi-resolution level
    /// This feature is only supported on QCOMM-based Android devices
    /// </summary>
    public static TiledMultiResLevel tiledMultiResLevel
    {
        get
        {
            if (!OVRPlugin.tiledMultiResSupported)
                Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
            return (TiledMultiResLevel)OVRPlugin.tiledMultiResLevel;
        }
        set
        {
            if (!OVRPlugin.tiledMultiResSupported)
                Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
            OVRPlugin.tiledMultiResLevel = (OVRPlugin.TiledMultiResLevel)value;
        }
    }

    /// <summary>
    /// Gets if the GPU Utility is supported
    /// This feature is only supported on QCOMM-based Android devices
    /// </summary>
    public static bool gpuUtilSupported => OVRPlugin.gpuUtilSupported;

    /// <summary>
    /// Gets the GPU Utilised Level (0.0 - 1.0)
    /// This feature is only supported on QCOMM-based Android devices
    /// </summary>
    public static float gpuUtilLevel
    {
        get
        {
            if (!OVRPlugin.gpuUtilSupported)
                Debug.LogWarning("GPU Util is not supported");
            return OVRPlugin.gpuUtilLevel;
        }
    }

    /// <summary>
    /// Sets the Color Scale and Offset which is commonly used for effects like fade-to-black.
    /// In our compositor, once a given frame is rendered, warped, and ready to be displayed, we then multiply
    /// each pixel by colorScale and add it to colorOffset, whereby newPixel = oldPixel * colorScale + colorOffset.
    /// Note that for mobile devices (Quest, Go, etc.), colorOffset is not supported, so colorScale is all that can
    /// be used. A colorScale of (1, 1, 1, 1) and colorOffset of (0, 0, 0, 0) will lead to an identity multiplication
    /// and have no effect.
    /// </summary>
    public static void SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, bool applyToAllLayers) => OVRPlugin.SetColorScaleAndOffset(colorScale, colorOffset, applyToAllLayers);

    /// <summary>
    /// Specifies OpenVR pose local to tracking space
    /// </summary>
    public static void SetOpenVRLocalPose(Vector3 leftPos, Vector3 rightPos, Quaternion leftRot, Quaternion rightRot)
    {
        if (loadedXRDevice == XRDevice.OpenVR)
            OVRInput.SetOpenVRLocalPose(leftPos, rightPos, leftRot, rightRot);
    }

    // Series of offsets that line up the virtual controllers to the phsyical world.
    static Vector3 OpenVRTouchRotationOffsetEulerLeft = new Vector3(40.0f, 0.0f, 0.0f);
    static Vector3 OpenVRTouchRotationOffsetEulerRight = new Vector3(40.0f, 0.0f, 0.0f);
    static Vector3 OpenVRTouchPositionOffsetLeft = new Vector3(0.0075f, -0.005f, -0.0525f);
    static Vector3 OpenVRTouchPositionOffsetRight = new Vector3(-0.0075f, -0.005f, -0.0525f);

    /// <summary>
    /// Specifies the pose offset required to make an OpenVR controller's reported pose match the virtual pose.
    /// Currently we only specify this offset for Oculus Touch on OpenVR.
    /// </summary>
    public static OVRPose GetOpenVRControllerOffset(Node hand)
    {
        var poseOffset = OVRPose.identity;
        if ((hand == Node.LeftHand || hand == Node.RightHand) && loadedXRDevice == XRDevice.OpenVR)
        {
            var index = (hand == Node.LeftHand) ? 0 : 1;
            if (OVRInput.openVRControllerDetails[index].controllerType == OVRInput.OpenVRController.OculusTouch)
            {
                var offsetOrientation = hand == Node.LeftHand ? OpenVRTouchRotationOffsetEulerLeft : OpenVRTouchRotationOffsetEulerRight;
                poseOffset.orientation = Quaternion.Euler(offsetOrientation.x, offsetOrientation.y, offsetOrientation.z);
                poseOffset.position = hand == Node.LeftHand ? OpenVRTouchPositionOffsetLeft : OpenVRTouchPositionOffsetRight;
            }
        }
        return poseOffset;
    }

    [Header("Tracking"), SerializeField, Tooltip("Defines the current tracking origin type.")] TrackingOrigin _trackingOriginType = OVRManager.TrackingOrigin.EyeLevel;
    /// <summary>
    /// Defines the current tracking origin type.
    /// </summary>
    public TrackingOrigin trackingOriginType
    {
        get => !isHmdPresent ? _trackingOriginType : (TrackingOrigin)OVRPlugin.GetTrackingOriginType();
        set
        {
            if (!isHmdPresent)
                return;
            if (OVRPlugin.SetTrackingOriginType((OVRPlugin.TrackingOrigin)value))
                // Keep the field exposed in the Unity Editor synchronized with any changes.
                _trackingOriginType = value;
        }
    }

    /// <summary>
    /// If true, head tracking will affect the position of each OVRCameraRig's cameras.
    /// </summary>
    [Tooltip("If true, head tracking will affect the position of each OVRCameraRig's cameras.")] public bool usePositionTracking = true;

    /// <summary>
    /// If true, head tracking will affect the rotation of each OVRCameraRig's cameras.
    /// </summary>
    [HideInInspector] public bool useRotationTracking = true;

    /// <summary>
    /// If true, the distance between the user's eyes will affect the position of each OVRCameraRig's cameras.
    /// </summary>
    [Tooltip("If true, the distance between the user's eyes will affect the position of each OVRCameraRig's cameras.")] public bool useIPDInPositionTracking = true;

    /// <summary>
    /// If true, each scene load will cause the head pose to reset.
    /// </summary>
    [Tooltip("If true, each scene load will cause the head pose to reset.")] public bool resetTrackerOnLoad = false;

    /// <summary>
    /// If true, the Reset View in the universal menu will cause the pose to be reset. This should generally be
    /// enabled for applications with a stationary position in the virtual world and will allow the View Reset
    /// command to place the person back to a predefined location (such as a cockpit seat).
    /// Set this to false if you have a locomotion system because resetting the view would effectively teleport
    /// the player to potentially invalid locations.
    /// </summary>
    [Tooltip("If true, the Reset View in the universal menu will cause the pose to be reset. This should generally be enabled for applications with a stationary position in the virtual world and will allow the View Reset command to place the person back to a predefined location (such as a cockpit seat). Set this to false if you have a locomotion system because resetting the view would effectively teleport the player to potentially invalid locations.")] public bool AllowRecenter = true;

    [SerializeField, Tooltip("Specifies HMD recentering behavior when controller recenter is performed. True recenters the HMD as well, false does not.")] bool _reorientHMDOnControllerRecenter = true;
    /// <summary>
    /// Defines the recentering mode specified in the tooltip above.
    /// </summary>
    public bool reorientHMDOnControllerRecenter
    {
        get => !isHmdPresent ? false : OVRPlugin.GetReorientHMDOnControllerRecenter();
        set
        {
            if (!isHmdPresent)
                return;
            OVRPlugin.SetReorientHMDOnControllerRecenter(value);
        }
    }

    /// <summary>
    /// True if the current platform supports virtual reality.
    /// </summary>
    public bool isSupportedPlatform { get; private set; }

    static bool _isUserPresentCached = false;
    static bool _isUserPresent = false;
    static bool _wasUserPresent = false;
    /// <summary>
    /// True if the user is currently wearing the display.
    /// </summary>
    public bool isUserPresent
    {
        get
        {
            if (!_isUserPresentCached)
            {
                _isUserPresentCached = true;
                _isUserPresent = OVRPlugin.userPresent;
            }
            return _isUserPresent;
        }
        private set
        {
            _isUserPresentCached = true;
            _isUserPresent = value;
        }
    }

    static bool prevAudioOutIdIsCached = false;
    static bool prevAudioInIdIsCached = false;
    static string prevAudioOutId = string.Empty;
    static string prevAudioInId = string.Empty;
    static bool wasPositionTracked = false;

    public static Version utilitiesVersion => OVRPlugin.wrapperVersion;
    public static Version pluginVersion => OVRPlugin.version;
    public static Version sdkVersion => OVRPlugin.nativeSDKVersion;

#if _WIN
    static bool prevEnableMixedReality = false;
    static bool MixedRealityEnabledFromCmd()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i].ToLower() == "-mixedreality")
                return true;
        return false;
    }

    static bool UseDirectCompositionFromCmd()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i].ToLower() == "-directcomposition")
                return true;
        return false;
    }

    static bool UseExternalCompositionFromCmd()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i].ToLower() == "-externalcomposition")
                return true;
        return false;
    }

    static bool CreateMixedRealityCaptureConfigurationFileFromCmd()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i].ToLower() == "-create_mrc_config")
                return true;
        return false;
    }

    static bool LoadMixedRealityCaptureConfigurationFileFromCmd()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i].ToLower() == "-load_mrc_config")
                return true;
        return false;
    }
#endif

    internal static bool IsUnityAlphaOrBetaVersion()
    {
        var ver = Application.unityVersion;
        var pos = ver.Length - 1;
        while (pos >= 0 && ver[pos] >= '0' && ver[pos] <= '9')
            --pos;
        return pos >= 0 && (ver[pos] == 'a' || ver[pos] == 'b');
    }

    internal static string UnityAlphaOrBetaVersionWarningMessage = "WARNING: It's not recommended to use Unity alpha/beta release in Oculus development. Use a stable release if you encounter any issue.";

    #region Unity Messages

    void Awake()
    {
        // Only allow one instance at runtime.
        if (instance != null)
        {
            enabled = false;
            DestroyImmediate(this);
            return;
        }
        instance = this;
        // uncomment the following line to disable the callstack printed to log
        //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Debug.Log("Unity v" + Application.unityVersion + ", " +
                  "Oculus Utilities v" + OVRPlugin.wrapperVersion + ", " +
                  "OVRPlugin v" + OVRPlugin.version + ", " +
                  "SDK v" + OVRPlugin.nativeSDKVersion + ".");
        if (!Application.isEditor && IsUnityAlphaOrBetaVersion())
            Debug.LogWarning(UnityAlphaOrBetaVersionWarningMessage);

#if _WIN
        var supportedTypes =
            UnityEngine.Rendering.GraphicsDeviceType.Direct3D11.ToString() + ", " +
            UnityEngine.Rendering.GraphicsDeviceType.Direct3D12.ToString();
        if (!supportedTypes.Contains(SystemInfo.graphicsDeviceType.ToString()))
            Debug.LogWarning("VR rendering requires one of the following device types: (" + supportedTypes + "). Your graphics device: " + SystemInfo.graphicsDeviceType.ToString());
#endif

        // Detect whether this platform is a supported platform
        var currPlatform = Application.platform;
        if (currPlatform == RuntimePlatform.Android ||
            // currPlatform == RuntimePlatform.LinuxPlayer ||
            currPlatform == RuntimePlatform.OSXEditor ||
            currPlatform == RuntimePlatform.OSXPlayer ||
            currPlatform == RuntimePlatform.WindowsEditor ||
            currPlatform == RuntimePlatform.WindowsPlayer)
            isSupportedPlatform = true;
        else
            isSupportedPlatform = false;
        if (!isSupportedPlatform)
        {
            Debug.LogWarning("This platform is unsupported");
            return;
        }

        if (Application.platform == RuntimePlatform.Android && !Application.isEditor)
            // Turn off chromatic aberration by default to save texture bandwidth.
            chromatic = false;

        if (Application.platform == RuntimePlatform.WindowsPlayer && !Application.isEditor)
            enableMixedReality = false;     // we should never start the standalone game in MxR mode, unless the command-line parameter is provided

#if _WIN
        var loadMrcConfig = LoadMixedRealityCaptureConfigurationFileFromCmd();
        var createMrcConfig = CreateMixedRealityCaptureConfigurationFileFromCmd();
        if (loadMrcConfig || createMrcConfig)
        {
            var mrcSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
            mrcSettings.ReadFrom(this);
            if (loadMrcConfig)
            {
                mrcSettings.CombineWithConfigurationFile();
                mrcSettings.ApplyTo(this);
            }
            if (createMrcConfig)
                mrcSettings.WriteToConfigurationFile();
            ScriptableObject.Destroy(mrcSettings);
        }
        if (MixedRealityEnabledFromCmd())
            enableMixedReality = true;
        if (enableMixedReality)
        {
            Debug.Log("OVR: Mixed Reality mode enabled");
            if (UseDirectCompositionFromCmd()) compositionMethod = CompositionMethod.Direct;
            if (UseExternalCompositionFromCmd()) compositionMethod = CompositionMethod.External;
            Debug.Log("OVR: CompositionMethod : " + compositionMethod);
        }
#endif

#if _WIN
        if (enableAdaptiveResolution && !OVRManager.IsAdaptiveResSupportedByEngine())
        {
            enableAdaptiveResolution = false;
            Debug.LogError("Your current Unity Engine " + Application.unityVersion + " might have issues to support adaptive resolution, please disable it under OVRManager");
        }
#endif

        Initialize();

        if (resetTrackerOnLoad)
            display.RecenterPose();
        if (Debug.isDebugBuild)
        {
            // Activate system metrics collection in Debug/Developerment build
            if (GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>() == null)
                gameObject.AddComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
            OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer perfTcpServer = GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
            perfTcpServer.listeningPort = profilerTcpPort;
            if (!perfTcpServer.enabled)
                perfTcpServer.enabled = true;
        }

#if _WIN
        // Force OcculusionMesh on all the time, you can change the value to false if you really need it be off for some reasons,
        // be aware there are performance drops if you don't use occlusionMesh.
        OVRPlugin.occlusionMesh = true;
#endif
    }

#if UNITY_EDITOR
	static bool _scriptsReloaded;
	[UnityEditor.Callbacks.DidReloadScripts]
	static void ScriptsReloaded() => _scriptsReloaded = true;
#endif

    void SetCurrentXRDevice()
    {
        if (OVRPlugin.initialized)
            loadedXRDevice = XRDevice.Oculus;
        else if (Settings.enabled)
        {
            var loadedXRDeviceName = Settings.loadedDeviceName;
            if (loadedXRDeviceName == OPENVR_UNITY_NAME_STR) loadedXRDevice = XRDevice.OpenVR;
            else loadedXRDevice = XRDevice.Unknown;
        }
        else loadedXRDevice = XRDevice.Unknown;
    }

    void Initialize()
    {
        if (display == null) display = new OVRDisplay();
        if (tracker == null) tracker = new OVRTracker();
        if (boundary == null) boundary = new OVRBoundary();
        reorientHMDOnControllerRecenter = _reorientHMDOnControllerRecenter;
        SetCurrentXRDevice();
    }

#if _WIN
    bool suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;
#endif

    void Update()
    {
#if UNITY_EDITOR
		if (_scriptsReloaded)
		{
			_scriptsReloaded = false;
			instance = this;
			Initialize();
		}
#endif

        SetCurrentXRDevice();
        if (OVRPlugin.shouldQuit)
        {
            Debug.Log("[OVRManager] OVRPlugin.shouldQuit detected");
            Application.Quit();
        }
        if (AllowRecenter && OVRPlugin.shouldRecenter)
            display.RecenterPose();
        if (trackingOriginType != _trackingOriginType)
            trackingOriginType = _trackingOriginType;
        tracker.isEnabled = usePositionTracking;
        OVRPlugin.rotation = useRotationTracking;
        OVRPlugin.useIPDInPositionTracking = useIPDInPositionTracking;

        // Dispatch HMD events.
        isHmdPresent = OVRNodeStateProperties.IsHmdPresent();
        if (useRecommendedMSAALevel && QualitySettings.antiAliasing != display.recommendedMSAALevel)
        {
            Debug.Log("The current MSAA level is " + QualitySettings.antiAliasing +
            ", but the recommended MSAA level is " + display.recommendedMSAALevel +
            ". Switching to the recommended level.");
            QualitySettings.antiAliasing = display.recommendedMSAALevel;
        }
        if (monoscopic != _monoscopic)
            monoscopic = _monoscopic;
        if (headPoseRelativeOffsetRotation != _headPoseRelativeOffsetRotation)
            headPoseRelativeOffsetRotation = _headPoseRelativeOffsetRotation;
        if (headPoseRelativeOffsetTranslation != _headPoseRelativeOffsetTranslation)
            headPoseRelativeOffsetTranslation = _headPoseRelativeOffsetTranslation;
        if (_wasHmdPresent && !isHmdPresent)
            try
            {
                Debug.Log("[OVRManager] HMDLost event");
                HMDLost?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }

        if (!_wasHmdPresent && isHmdPresent)
            try
            {
                Debug.Log("[OVRManager] HMDAcquired event");
                HMDAcquired?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }

        _wasHmdPresent = isHmdPresent;

        // Dispatch HMD mounted events.
        isUserPresent = OVRPlugin.userPresent;
        if (_wasUserPresent && !isUserPresent)
            try
            {
                Debug.Log("[OVRManager] HMDUnmounted event");
                HMDUnmounted?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }

        if (!_wasUserPresent && isUserPresent)
            try
            {
                Debug.Log("[OVRManager] HMDMounted event");
                HMDMounted?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        _wasUserPresent = isUserPresent;

        // Dispatch VR Focus events.
        hasVrFocus = OVRPlugin.hasVrFocus;
        if (_hadVrFocus && !hasVrFocus)
            try
            {
                Debug.Log("[OVRManager] VrFocusLost event");
                VrFocusLost?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        if (!_hadVrFocus && hasVrFocus)
            try
            {
                Debug.Log("[OVRManager] VrFocusAcquired event");
                VrFocusAcquired?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        _hadVrFocus = hasVrFocus;

        // Dispatch VR Input events.
        var hasInputFocus = OVRPlugin.hasInputFocus;
        if (_hadInputFocus && !hasInputFocus)
            try
            {
                Debug.Log("[OVRManager] InputFocusLost event");
                InputFocusLost?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        if (!_hadInputFocus && hasInputFocus)
            try
            {
                Debug.Log("[OVRManager] InputFocusAcquired event");
                InputFocusAcquired?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        _hadInputFocus = hasInputFocus;

        // Changing effective rendering resolution dynamically according performance
#if _WIN
        if (enableAdaptiveResolution)
        {
            if (Settings.eyeTextureResolutionScale < maxRenderScale) Settings.eyeTextureResolutionScale = maxRenderScale; // Allocate renderScale to max to avoid re-allocation
            else maxRenderScale = Mathf.Max(maxRenderScale, Settings.eyeTextureResolutionScale); // Adjusting maxRenderScale in case app started with a larger renderScale value
            minRenderScale = Mathf.Min(minRenderScale, maxRenderScale);
            var minViewportScale = minRenderScale / Settings.eyeTextureResolutionScale;
            var recommendedViewportScale = OVRPlugin.GetEyeRecommendedResolutionScale() / Settings.eyeTextureResolutionScale;
            recommendedViewportScale = Mathf.Clamp(recommendedViewportScale, minViewportScale, 1.0f);
            Settings.renderViewportScale = recommendedViewportScale;
        }
#endif

        // Dispatch Audio Device events.
        var audioOutId = OVRPlugin.audioOutId;
        if (!prevAudioOutIdIsCached)
        {
            prevAudioOutId = audioOutId;
            prevAudioOutIdIsCached = true;
        }
        else if (audioOutId != prevAudioOutId)
        {
            try
            {
                Debug.Log("[OVRManager] AudioOutChanged event");
                AudioOutChanged?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
            prevAudioOutId = audioOutId;
        }

        var audioInId = OVRPlugin.audioInId;
        if (!prevAudioInIdIsCached)
        {
            prevAudioInId = audioInId;
            prevAudioInIdIsCached = true;
        }
        else if (audioInId != prevAudioInId)
        {
            try
            {
                Debug.Log("[OVRManager] AudioInChanged event");
                AudioInChanged?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
            prevAudioInId = audioInId;
        }

        // Dispatch tracking events.
        if (wasPositionTracked && !tracker.isPositionTracked)
            try
            {
                Debug.Log("[OVRManager] TrackingLost event");
                TrackingLost?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        if (!wasPositionTracked && tracker.isPositionTracked)
            try
            {
                Debug.Log("[OVRManager] TrackingAcquired event");
                TrackingAcquired?.Invoke();
            }
            catch (Exception e) { Debug.LogError("Caught Exception: " + e); }
        wasPositionTracked = tracker.isPositionTracked;

        display.Update();
        OVRInput.Update();

#if _WIN
        if (enableMixedReality && !prevEnableMixedReality)
            OVRPlugin.SendEvent("mixed_reality_capture", "activated");
        if (enableMixedReality || prevEnableMixedReality)
        {
            var mainCamera = FindMainCamera();
            if (Camera.main != null)
            {
                suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;
                if (enableMixedReality)
                    OVRMixedReality.Update(gameObject, mainCamera, compositionMethod, useDynamicLighting, capturingCameraDevice, depthQuality);
                if (prevEnableMixedReality && !enableMixedReality)
                    OVRMixedReality.Cleanup();
                prevEnableMixedReality = enableMixedReality;
            }
            else if (!suppressDisableMixedRealityBecauseOfNoMainCameraWarning)
            {
                Debug.LogWarning("Main Camera is not set, Mixed Reality disabled");
                suppressDisableMixedRealityBecauseOfNoMainCameraWarning = true;
            }
        }
#endif
    }

    bool multipleMainCameraWarningPresented = false;
    Camera FindMainCamera()
    {
        var objects = GameObject.FindGameObjectsWithTag("MainCamera");
        var cameras = new List<Camera>(4);
        foreach (var obj in objects)
        {
            var camera = obj.GetComponent<Camera>();
            if (camera != null && camera.enabled)
            {
                var cameraRig = camera.GetComponentInParent<OVRCameraRig>();
                if (cameraRig != null && cameraRig.trackingSpace != null)
                    cameras.Add(camera);
            }
        }
        if (cameras.Count == 0) return Camera.main;     // pick one of the cameras which tagged as "MainCamera"
        else if (cameras.Count == 1) return cameras[0];
        else
        {
            if (!multipleMainCameraWarningPresented)
            {
                Debug.LogWarning("Multiple MainCamera found. Assume the real MainCamera is the camera with the least depth");
                multipleMainCameraWarningPresented = true;
            }
            // return the camera with least depth
            cameras.Sort((Camera c0, Camera c1) => { return c0.depth < c1.depth ? -1 : (c0.depth > c1.depth ? 1 : 0); });
            return cameras[0];
        }
    }

    void OnDisable()
    {
        var perfTcpServer = GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
        if (perfTcpServer != null)
            perfTcpServer.enabled = false;
#if _WIN
        OVRMixedReality.Cleanup();
#endif
    }

    void LateUpdate() => OVRHaptics.Process();

    void FixedUpdate() => OVRInput.FixedUpdate();

    void OnDestroy() => Debug.Log("[OVRManager] OnDestroy");

    void OnApplicationPause(bool pause) => Debug.Log(pause ? "[OVRManager] OnApplicationPause(true)" : "[OVRManager] OnApplicationPause(false)");

    void OnApplicationFocus(bool focus) => Debug.Log(focus ? "[OVRManager] OnApplicationFocus(true)" : "[OVRManager] OnApplicationFocus(false)");

    void OnApplicationQuit() => Debug.Log("[OVRManager] OnApplicationQuit");

    #endregion // Unity Messages

    /// <summary>
    /// Leaves the application/game and returns to the launcher/dashboard
    /// </summary>
    public void ReturnToLauncher() => PlatformUIConfirmQuit(); // show the platform UI quit prompt

    public static void PlatformUIConfirmQuit()
    {
        if (!isHmdPresent)
            return;
        OVRPlugin.ShowUI(OVRPlugin.PlatformUI.ConfirmQuit);
    }
}
