// SKYTODO

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
using System.Runtime.InteropServices;
using UnityEngine;

// Internal C# wrapper for OVRPlugin.

public static class OVRPlugin
{
    public static bool isAndroid => Application.platform == RuntimePlatform.Android && !Application.isEditor;
    public const bool isSupportedPlatform = false;
    public static readonly Version wrapperVersion = OVRP_1_37_0.version;

    static Version _version;
    public static Version version
    {
        get
        {
            if (_version == null)
            {
                try
                {
                    var pluginVersion = OVRP_1_1_0.ovrp_GetVersion();
                    // Truncate unsupported trailing version info for Version. Original string is returned if not present.
                    _version = pluginVersion != null ? new Version(pluginVersion.Split('-')[0]) : _versionZero;
                }
                catch { _version = _versionZero; }
                // Unity 5.1.1f3-p3 have OVRPlugin version "0.5.0", which isn't accurate.
                if (_version == OVRP_0_5_0.version)
                    _version = OVRP_0_1_0.version;
                if (_version > _versionZero && _version < OVRP_1_3_0.version)
                    throw new PlatformNotSupportedException("Oculus Utilities version " + wrapperVersion + " is too new for OVRPlugin version " + _version.ToString() + ". Update to the latest version of Unity.");
            }
            return _version;
        }
    }

    static Version _nativeSDKVersion;
    public static Version nativeSDKVersion
    {
        get
        {
            if (_nativeSDKVersion == null)
            {
                try
                {
                    var sdkVersion = version >= OVRP_1_1_0.version ? OVRP_1_1_0.ovrp_GetNativeSDKVersion() : _versionZero.ToString();
                    // Truncate unsupported trailing version info for Version. Original string is returned if not present.
                    _nativeSDKVersion = sdkVersion != null ? new Version(sdkVersion.Split('-')[0]) : _versionZero;
                }
                catch { _nativeSDKVersion = _versionZero; }
            }
            return _nativeSDKVersion;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    class GUID
    {
        public int a;
        public short b;
        public short c;
        public byte d0;
        public byte d1;
        public byte d2;
        public byte d3;
        public byte d4;
        public byte d5;
        public byte d6;
        public byte d7;
    }

    public enum Bool
    {
        False = 0,
        True
    }

    public enum Result
    {
        /// Success
        Success = 0,
        /// Failure
        Failure = -1000,
        Failure_InvalidParameter = -1001,
        Failure_NotInitialized = -1002,
        Failure_InvalidOperation = -1003,
        Failure_Unsupported = -1004,
        Failure_NotYetImplemented = -1005,
        Failure_OperationFailed = -1006,
        Failure_InsufficientSize = -1007,
    }

    public enum CameraStatus
    {
        CameraStatus_None,
        CameraStatus_Connected,
        CameraStatus_Calibrating,
        CameraStatus_CalibrationFailed,
        CameraStatus_Calibrated,
        CameraStatus_EnumSize = 0x7fffffff
    }

    public enum Eye
    {
        None = -1,
        Left = 0,
        Right = 1,
        Count = 2
    }

    public enum Tracker
    {
        None = -1,
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Count,
    }

    public enum Node
    {
        None = -1,
        EyeLeft = 0,
        EyeRight = 1,
        EyeCenter = 2,
        HandLeft = 3,
        HandRight = 4,
        TrackerZero = 5,
        TrackerOne = 6,
        TrackerTwo = 7,
        TrackerThree = 8,
        Head = 9,
        DeviceObjectZero = 10,
        Count,
    }

    public enum Controller
    {
        None = 0,
        LTouch = 0x00000001,
        RTouch = 0x00000002,
        Touch = LTouch | RTouch,
        Remote = 0x00000004,
        Gamepad = 0x00000010,
        Touchpad = 0x08000000,
        LTrackedRemote = 0x01000000,
        RTrackedRemote = 0x02000000,
        Active = unchecked((int)0x80000000),
        All = ~None,
    }

    public enum Handedness
    {
        Unsupported = 0,
        LeftHanded = 1,
        RightHanded = 2,
    }

    public enum TrackingOrigin
    {
        EyeLevel = 0,
        FloorLevel = 1,
        Stage = 2,
        Count,
    }

    public enum RecenterFlags
    {
        Default = 0,
        Controllers = 0x40000000,
        IgnoreAll = unchecked((int)0x80000000),
        Count,
    }

    public enum BatteryStatus
    {
        Charging = 0,
        Discharging,
        Full,
        NotCharging,
        Unknown,
    }

    public enum EyeTextureFormat
    {
        Default = 0,
        R8G8B8A8_sRGB = 0,
        R8G8B8A8 = 1,
        R16G16B16A16_FP = 2,
        R11G11B10_FP = 3,
        B8G8R8A8_sRGB = 4,
        B8G8R8A8 = 5,
        R5G6B5 = 11,
        EnumSize = 0x7fffffff
    }

    public enum PlatformUI
    {
        None = -1,
        ConfirmQuit = 1,
        GlobalMenuTutorial, // Deprecated
    }

    public enum SystemRegion
    {
        Unspecified = 0,
        Japan,
        China,
    }

    public enum SystemHeadset
    {
        None = 0,
        GearVR_R320, // Note4 Innovator
        GearVR_R321, // S6 Innovator
        GearVR_R322, // Commercial 1
        GearVR_R323, // Commercial 2 (USB Type C)
        GearVR_R324, // Commercial 3 (USB Type C)
        GearVR_R325, // Commercial 4 (USB Type C)
        Oculus_Go,
        Oculus_Quest,
        Rift_DK1 = 0x1000,
        Rift_DK2,
        Rift_CV1,
        Rift_CB,
        Rift_S,
    }

    public enum OverlayShape
    {
        Quad = 0,
        Cylinder = 1,
        Cubemap = 2,
        OffcenterCubemap = 4,
        Equirect = 5,
    }

    public enum Step
    {
        Render = -1,
        Physics = 0,
    }

    public enum CameraDevice
    {
        None = 0,
        WebCamera0 = 100,
        WebCamera1 = 101,
        ZEDCamera = 300,
    }

    public enum CameraDeviceDepthSensingMode
    {
        Standard = 0,
        Fill = 1,
    }

    public enum CameraDeviceDepthQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    public enum TiledMultiResLevel
    {
        Off = 0,
        LMSLow = 1,
        LMSMedium = 2,
        LMSHigh = 3,
        // High foveation setting with more detail toward the bottom of the view and more foveation near the top (Same as High on Oculus Go)
        LMSHighTop = 4,
        EnumSize = 0x7FFFFFFF
    }

    public enum PerfMetrics
    {
        App_CpuTime_Float = 0,
        App_GpuTime_Float = 1,
        Compositor_CpuTime_Float = 3,
        Compositor_GpuTime_Float = 4,
        Compositor_DroppedFrameCount_Int = 5,
        System_GpuUtilPercentage_Float = 7,
        System_CpuUtilAveragePercentage_Float = 8,
        System_CpuUtilWorstPercentage_Float = 9,
        // 1.32.0
        Device_CpuClockFrequencyInMHz_Float = 10,
        Device_GpuClockFrequencyInMHz_Float = 11,
        Device_CpuClockLevel_Int = 12,
        Device_GpuClockLevel_Int = 13,
        Count,
        EnumSize = 0x7FFFFFFF
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraDeviceIntrinsicsParameters
    {
        float fx; /* Focal length in pixels along x axis. */
        float fy; /* Focal length in pixels along y axis. */
        float cx; /* Optical center along x axis, defined in pixels (usually close to width/2). */
        float cy; /* Optical center along y axis, defined in pixels (usually close to height/2). */
        double disto0; /* Distortion factor : [ k1, k2, p1, p2, k3 ]. Radial (k1,k2,k3) and Tangential (p1,p2) distortion.*/
        double disto1;
        double disto2;
        double disto3;
        double disto4;
        float v_fov; /* Vertical field of view after stereo rectification, in degrees. */
        float h_fov; /* Horizontal field of view after stereo rectification, in degrees.*/
        float d_fov; /* Diagonal field of view after stereo rectification, in degrees.*/
        int w; /* Resolution width */
        int h; /* Resolution height */
    }

    const int OverlayShapeFlagShift = 4;
    enum OverlayFlag
    {
        None = unchecked(0x00000000),
        OnTop = unchecked(0x00000001),
        HeadLocked = unchecked(0x00000002),
        NoDepth = unchecked(0x00000004),
        // Using the 5-8 bits for shapes, total 16 potential shapes can be supported 0x000000[0]0 ->  0x000000[F]0
        ShapeFlag_Quad = unchecked(OverlayShape.Quad << OverlayShapeFlagShift),
        ShapeFlag_Cylinder = unchecked(OverlayShape.Cylinder << OverlayShapeFlagShift),
        ShapeFlag_Cubemap = unchecked(OverlayShape.Cubemap << OverlayShapeFlagShift),
        ShapeFlag_OffcenterCubemap = unchecked(OverlayShape.OffcenterCubemap << OverlayShapeFlagShift),
        ShapeFlagRangeMask = unchecked(0xF << OverlayShapeFlagShift),
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2f
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;
        public static readonly Vector3f zero = new Vector3f { x = 0.0f, y = 0.0f, z = 0.0f };
        public override string ToString() => string.Format("{0}, {1}, {2}", x, y, z);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Quatf
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public static readonly Quatf identity = new Quatf { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", x, y, z, w);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Posef
    {
        public Quatf Orientation;
        public Vector3f Position;
        public static readonly Posef identity = new Posef { Orientation = Quatf.identity, Position = Vector3f.zero };
        public override string ToString() => string.Format("Position ({0}), Orientation({1})", Position, Orientation);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureRectMatrixf
    {
        public Rect leftRect;
        public Rect rightRect;
        public Vector4 leftScaleBias;
        public Vector4 rightScaleBias;
        public static readonly TextureRectMatrixf zero = new TextureRectMatrixf { leftRect = new Rect(0, 0, 1, 1), rightRect = new Rect(0, 0, 1, 1), leftScaleBias = new Vector4(1, 1, 0, 0), rightScaleBias = new Vector4(1, 1, 0, 0) };
        public override string ToString() => string.Format("Rect Left ({0}), Rect Right({1}), Scale Bias Left ({2}), Scale Bias Right({3})", leftRect, rightRect, leftScaleBias, rightScaleBias);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PoseStatef
    {
        public Posef Pose;
        public Vector3f Velocity;
        public Vector3f Acceleration;
        public Vector3f AngularVelocity;
        public Vector3f AngularAcceleration;
        public double Time;
        public static readonly PoseStatef identity = new PoseStatef
        {
            Pose = Posef.identity,
            Velocity = Vector3f.zero,
            Acceleration = Vector3f.zero,
            AngularVelocity = Vector3f.zero,
            AngularAcceleration = Vector3f.zero
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerState4
    {
        public uint ConnectedControllers;
        public uint Buttons;
        public uint Touches;
        public uint NearTouches;
        public float LIndexTrigger;
        public float RIndexTrigger;
        public float LHandTrigger;
        public float RHandTrigger;
        public Vector2f LThumbstick;
        public Vector2f RThumbstick;
        public Vector2f LTouchpad;
        public Vector2f RTouchpad;
        public byte LBatteryPercentRemaining;
        public byte RBatteryPercentRemaining;
        public byte LRecenterCount;
        public byte RRecenterCount;
        public byte Reserved_27;
        public byte Reserved_26;
        public byte Reserved_25;
        public byte Reserved_24;
        public byte Reserved_23;
        public byte Reserved_22;
        public byte Reserved_21;
        public byte Reserved_20;
        public byte Reserved_19;
        public byte Reserved_18;
        public byte Reserved_17;
        public byte Reserved_16;
        public byte Reserved_15;
        public byte Reserved_14;
        public byte Reserved_13;
        public byte Reserved_12;
        public byte Reserved_11;
        public byte Reserved_10;
        public byte Reserved_09;
        public byte Reserved_08;
        public byte Reserved_07;
        public byte Reserved_06;
        public byte Reserved_05;
        public byte Reserved_04;
        public byte Reserved_03;
        public byte Reserved_02;
        public byte Reserved_01;
        public byte Reserved_00;
        public ControllerState4(ControllerState2 cs)
        {
            ConnectedControllers = cs.ConnectedControllers;
            Buttons = cs.Buttons;
            Touches = cs.Touches;
            NearTouches = cs.NearTouches;
            LIndexTrigger = cs.LIndexTrigger;
            RIndexTrigger = cs.RIndexTrigger;
            LHandTrigger = cs.LHandTrigger;
            RHandTrigger = cs.RHandTrigger;
            LThumbstick = cs.LThumbstick;
            RThumbstick = cs.RThumbstick;
            LTouchpad = cs.LTouchpad;
            RTouchpad = cs.RTouchpad;
            LBatteryPercentRemaining = 0;
            RBatteryPercentRemaining = 0;
            LRecenterCount = 0;
            RRecenterCount = 0;
            Reserved_27 = 0;
            Reserved_26 = 0;
            Reserved_25 = 0;
            Reserved_24 = 0;
            Reserved_23 = 0;
            Reserved_22 = 0;
            Reserved_21 = 0;
            Reserved_20 = 0;
            Reserved_19 = 0;
            Reserved_18 = 0;
            Reserved_17 = 0;
            Reserved_16 = 0;
            Reserved_15 = 0;
            Reserved_14 = 0;
            Reserved_13 = 0;
            Reserved_12 = 0;
            Reserved_11 = 0;
            Reserved_10 = 0;
            Reserved_09 = 0;
            Reserved_08 = 0;
            Reserved_07 = 0;
            Reserved_06 = 0;
            Reserved_05 = 0;
            Reserved_04 = 0;
            Reserved_03 = 0;
            Reserved_02 = 0;
            Reserved_01 = 0;
            Reserved_00 = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerState2
    {
        public uint ConnectedControllers;
        public uint Buttons;
        public uint Touches;
        public uint NearTouches;
        public float LIndexTrigger;
        public float RIndexTrigger;
        public float LHandTrigger;
        public float RHandTrigger;
        public Vector2f LThumbstick;
        public Vector2f RThumbstick;
        public Vector2f LTouchpad;
        public Vector2f RTouchpad;
        public ControllerState2(ControllerState cs)
        {
            ConnectedControllers = cs.ConnectedControllers;
            Buttons = cs.Buttons;
            Touches = cs.Touches;
            NearTouches = cs.NearTouches;
            LIndexTrigger = cs.LIndexTrigger;
            RIndexTrigger = cs.RIndexTrigger;
            LHandTrigger = cs.LHandTrigger;
            RHandTrigger = cs.RHandTrigger;
            LThumbstick = cs.LThumbstick;
            RThumbstick = cs.RThumbstick;
            LTouchpad = new Vector2f { x = 0.0f, y = 0.0f };
            RTouchpad = new Vector2f { x = 0.0f, y = 0.0f };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerState
    {
        public uint ConnectedControllers;
        public uint Buttons;
        public uint Touches;
        public uint NearTouches;
        public float LIndexTrigger;
        public float RIndexTrigger;
        public float LHandTrigger;
        public float RHandTrigger;
        public Vector2f LThumbstick;
        public Vector2f RThumbstick;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HapticsBuffer
    {
        public IntPtr Samples;
        public int SamplesCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HapticsState
    {
        public int SamplesAvailable;
        public int SamplesQueued;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HapticsDesc
    {
        public int SampleRateHz;
        public int SampleSizeInBytes;
        public int MinimumSafeSamplesQueued;
        public int MinimumBufferSamplesCount;
        public int OptimalBufferSamplesCount;
        public int MaximumBufferSamplesCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AppPerfFrameStats
    {
        public int HmdVsyncIndex;
        public int AppFrameIndex;
        public int AppDroppedFrameCount;
        public float AppMotionToPhotonLatency;
        public float AppQueueAheadTime;
        public float AppCpuElapsedTime;
        public float AppGpuElapsedTime;
        public int CompositorFrameIndex;
        public int CompositorDroppedFrameCount;
        public float CompositorLatency;
        public float CompositorCpuElapsedTime;
        public float CompositorGpuElapsedTime;
        public float CompositorCpuStartToGpuEndElapsedTime;
        public float CompositorGpuEndToVsyncElapsedTime;
    }

    public const int AppPerfFrameStatsMaxCount = 5;

    [StructLayout(LayoutKind.Sequential)]
    public struct AppPerfStats
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = AppPerfFrameStatsMaxCount)] public AppPerfFrameStats[] FrameStats;
        public int FrameStatsCount;
        public Bool AnyFrameStatsDropped;
        public float AdaptiveGpuPerformanceScale;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sizei
    {
        public int w;
        public int h;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sizef
    {
        public float w;
        public float h;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2i
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Recti
    {
        Vector2i Pos;
        Sizei Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectf
    {
        Vector2f Pos;
        Sizef Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Frustumf
    {
        public float zNear;
        public float zFar;
        public float fovX;
        public float fovY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Frustumf2
    {
        public float zNear;
        public float zFar;
        public Fovf Fov;
    }

    public enum BoundaryType
    {
        OuterBoundary = 0x0001,
        PlayArea = 0x0100,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundaryTestResult
    {
        public Bool IsTriggering;
        public float ClosestDistance;
        public Vector3f ClosestPoint;
        public Vector3f ClosestPointNormal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundaryGeometry
    {
        public BoundaryType BoundaryType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public Vector3f[] Points;
        public int PointsCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Colorf
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Fovf
    {
        public float UpTan;
        public float DownTan;
        public float LeftTan;
        public float RightTan;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraIntrinsics
    {
        public bool IsValid;
        public double LastChangedTimeSeconds;
        public Fovf FOVPort;
        public float VirtualNearPlaneDistanceMeters;
        public float VirtualFarPlaneDistanceMeters;
        public Sizei ImageSensorPixelResolution;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraExtrinsics
    {
        public bool IsValid;
        public double LastChangedTimeSeconds;
        public CameraStatus CameraStatusData;
        public Node AttachedToNode;
        public Posef RelativePose;
    }

    public enum LayerLayout
    {
        Stereo = 0,
        Mono = 1,
        DoubleWide = 2,
        Array = 3,
        EnumSize = 0xF
    }

    public enum LayerFlags
    {
        Static = 1 << 0,
        LoadingScreen = 1 << 1,
        SymmetricFov = 1 << 2,
        TextureOriginAtBottomLeft = 1 << 3,
        ChromaticAberrationCorrection = 1 << 4,
        NoAllocation = 1 << 5,
        ProtectedContent = 1 << 6,
        AndroidSurfaceSwapChain = 1 << 7,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LayerDesc
    {
        public OverlayShape Shape;
        public LayerLayout Layout;
        public Sizei TextureSize;
        public int MipLevels;
        public int SampleCount;
        public EyeTextureFormat Format;
        public int LayerFlags;
        //Eye FOV-only members.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Fovf[] Fov;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Rectf[] VisibleRect;
        public Sizei MaxViewportSize;
        EyeTextureFormat DepthFormat;
        public override string ToString()
        {
            var delim = ", ";
            return Shape.ToString()
                + delim + Layout.ToString()
                + delim + TextureSize.w.ToString() + "x" + TextureSize.h.ToString()
                + delim + MipLevels.ToString()
                + delim + SampleCount.ToString()
                + delim + Format.ToString()
                + delim + LayerFlags.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LayerSubmit
    {
        int LayerId;
        int TextureStage;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] Recti[] ViewportRect;
        Posef Pose;
        int LayerSubmitFlags;
    }

    public static bool initialized => OVRP_1_1_0.ovrp_GetInitialized() == Bool.True;

    public static bool chromatic
    {
        get => version >= OVRP_1_7_0.version
                ? initialized && OVRP_1_7_0.ovrp_GetAppChromaticCorrection() == Bool.True
                : !isAndroid;
        set
        {
            if (initialized && version >= OVRP_1_7_0.version)
                OVRP_1_7_0.ovrp_SetAppChromaticCorrection(ToBool(value));
        }
    }

    public static bool monoscopic
    {
        get => initialized && OVRP_1_1_0.ovrp_GetAppMonoscopic() == Bool.True;
        set
        {
            if (initialized)
                OVRP_1_1_0.ovrp_SetAppMonoscopic(ToBool(value));
        }
    }

    public static bool rotation
    {
        get => initialized && OVRP_1_1_0.ovrp_GetTrackingOrientationEnabled() == Bool.True;
        set
        {
            if (initialized)
                OVRP_1_1_0.ovrp_SetTrackingOrientationEnabled(ToBool(value));
        }
    }

    public static bool position
    {
        get => initialized && OVRP_1_1_0.ovrp_GetTrackingPositionEnabled() == Bool.True;
        set
        {
            if (initialized)
                OVRP_1_1_0.ovrp_SetTrackingPositionEnabled(ToBool(value));
        }
    }

    public static bool useIPDInPositionTracking
    {
        get => initialized && version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetTrackingIPDEnabled() == Bool.True : true;
        set
        {
            if (initialized && version >= OVRP_1_6_0.version)
                OVRP_1_6_0.ovrp_SetTrackingIPDEnabled(ToBool(value));
        }
    }

    public static bool positionSupported => initialized && OVRP_1_1_0.ovrp_GetTrackingPositionSupported() == Bool.True;
    public static bool positionTracked => initialized && OVRP_1_1_0.ovrp_GetNodePositionTracked(Node.EyeCenter) == Bool.True;
    public static bool powerSaving => initialized && OVRP_1_1_0.ovrp_GetSystemPowerSavingMode() == Bool.True;
    public static bool hmdPresent => initialized && OVRP_1_1_0.ovrp_GetNodePresent(Node.EyeCenter) == Bool.True;
    public static bool userPresent => initialized && OVRP_1_1_0.ovrp_GetUserPresent() == Bool.True;
    public static bool headphonesPresent => initialized && OVRP_1_3_0.ovrp_GetSystemHeadphonesPresent() == Bool.True;
    public static int recommendedMSAALevel => initialized && version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetSystemRecommendedMSAALevel() : 2;
    public static SystemRegion systemRegion => initialized && version >= OVRP_1_5_0.version ? OVRP_1_5_0.ovrp_GetSystemRegion() : SystemRegion.Unspecified;

    static GUID _nativeAudioOutGuid = new GUID();
    static Guid _cachedAudioOutGuid;
    static string _cachedAudioOutString;

    public static string audioOutId
    {
        get
        {
            try
            {
                if (_nativeAudioOutGuid == null)
                    _nativeAudioOutGuid = new GUID();
                var ptr = OVRP_1_1_0.ovrp_GetAudioOutId();
                if (ptr != IntPtr.Zero)
                {
                    Marshal.PtrToStructure(ptr, _nativeAudioOutGuid);
                    var managedGuid = new Guid(
                        _nativeAudioOutGuid.a,
                        _nativeAudioOutGuid.b,
                        _nativeAudioOutGuid.c,
                        _nativeAudioOutGuid.d0,
                        _nativeAudioOutGuid.d1,
                        _nativeAudioOutGuid.d2,
                        _nativeAudioOutGuid.d3,
                        _nativeAudioOutGuid.d4,
                        _nativeAudioOutGuid.d5,
                        _nativeAudioOutGuid.d6,
                        _nativeAudioOutGuid.d7);
                    if (managedGuid != _cachedAudioOutGuid)
                    {
                        _cachedAudioOutGuid = managedGuid;
                        _cachedAudioOutString = _cachedAudioOutGuid.ToString();
                    }
                    return _cachedAudioOutString;
                }
            }
            catch { }
            return string.Empty;
        }
    }

    static GUID _nativeAudioInGuid = new OVRPlugin.GUID();
    static Guid _cachedAudioInGuid;
    static string _cachedAudioInString;

    public static string audioInId
    {
        get
        {
            try
            {
                if (_nativeAudioInGuid == null)
                    _nativeAudioInGuid = new GUID();
                var ptr = OVRP_1_1_0.ovrp_GetAudioInId();
                if (ptr != IntPtr.Zero)
                {
                    Marshal.PtrToStructure(ptr, _nativeAudioInGuid);
                    var managedGuid = new Guid(
                        _nativeAudioInGuid.a,
                        _nativeAudioInGuid.b,
                        _nativeAudioInGuid.c,
                        _nativeAudioInGuid.d0,
                        _nativeAudioInGuid.d1,
                        _nativeAudioInGuid.d2,
                        _nativeAudioInGuid.d3,
                        _nativeAudioInGuid.d4,
                        _nativeAudioInGuid.d5,
                        _nativeAudioInGuid.d6,
                        _nativeAudioInGuid.d7);
                    if (managedGuid != _cachedAudioInGuid)
                    {
                        _cachedAudioInGuid = managedGuid;
                        _cachedAudioInString = _cachedAudioInGuid.ToString();
                    }
                    return _cachedAudioInString;
                }
            }
            catch { }
            return string.Empty;
        }
    }

    public static bool hasVrFocus => OVRP_1_1_0.ovrp_GetAppHasVrFocus() == Bool.True;
    public static bool hasInputFocus => version >= OVRP_1_18_0.version ? OVRP_1_18_0.ovrp_GetAppHasInputFocus(out var inputFocus) == Result.Success ? inputFocus == Bool.True : false : true;
    public static bool shouldQuit => OVRP_1_1_0.ovrp_GetAppShouldQuit() == Bool.True;
    public static bool shouldRecenter => OVRP_1_1_0.ovrp_GetAppShouldRecenter() == Bool.True;
    public static string productName => OVRP_1_1_0.ovrp_GetSystemProductName();
    public static string latency => !initialized ? string.Empty : OVRP_1_1_0.ovrp_GetAppLatencyTimings();
    public static float eyeDepth
    {
        get => !initialized ? 0.0f : OVRP_1_1_0.ovrp_GetUserEyeDepth();
        set => OVRP_1_1_0.ovrp_SetUserEyeDepth(value);
    }
    public static float eyeHeight
    {
        get => OVRP_1_1_0.ovrp_GetUserEyeHeight();
        set => OVRP_1_1_0.ovrp_SetUserEyeHeight(value);
    }
    public static float batteryLevel => OVRP_1_1_0.ovrp_GetSystemBatteryLevel();
    public static float batteryTemperature => OVRP_1_1_0.ovrp_GetSystemBatteryTemperature();
    public static int cpuLevel
    {
        get => OVRP_1_1_0.ovrp_GetSystemCpuLevel();
        set => OVRP_1_1_0.ovrp_SetSystemCpuLevel(value);
    }
    public static int gpuLevel
    {
        get => OVRP_1_1_0.ovrp_GetSystemGpuLevel();
        set => OVRP_1_1_0.ovrp_SetSystemGpuLevel(value);
    }

    public static int vsyncCount
    {
        get => OVRP_1_1_0.ovrp_GetSystemVSyncCount();
        set => OVRP_1_2_0.ovrp_SetSystemVSyncCount(value);
    }
    public static float systemVolume => OVRP_1_1_0.ovrp_GetSystemVolume();
    public static float ipd
    {
        get => OVRP_1_1_0.ovrp_GetUserIPD();
        set => OVRP_1_1_0.ovrp_SetUserIPD(value);
    }
    public static bool occlusionMesh
    {
        get => initialized && (OVRP_1_3_0.ovrp_GetEyeOcclusionMeshEnabled() == Bool.True);
        set
        {
            if (!initialized)
                return;
            OVRP_1_3_0.ovrp_SetEyeOcclusionMeshEnabled(ToBool(value));
        }
    }
    public static BatteryStatus batteryStatus => OVRP_1_1_0.ovrp_GetSystemBatteryStatus();
    public static Frustumf GetEyeFrustum(Eye eyeId) => OVRP_1_1_0.ovrp_GetNodeFrustum((Node)eyeId);
    public static Sizei GetEyeTextureSize(Eye eyeId) => OVRP_0_1_0.ovrp_GetEyeTextureSize(eyeId);
    public static Posef GetTrackerPose(Tracker trackerId) => GetNodePose((Node)((int)trackerId + (int)Node.TrackerZero), Step.Render);
    public static Frustumf GetTrackerFrustum(Tracker trackerId) => OVRP_1_1_0.ovrp_GetNodeFrustum((Node)((int)trackerId + (int)Node.TrackerZero));
    public static bool ShowUI(PlatformUI ui) => OVRP_1_1_0.ovrp_ShowSystemUI(ui) == Bool.True;
    public static bool EnqueueSubmitLayer(bool onTop, bool headLocked, bool noDepthBufferTesting, IntPtr leftTexture, IntPtr rightTexture, int layerId, int frameIndex, Posef pose, Vector3f scale, int layerIndex = 0, OverlayShape shape = OverlayShape.Quad,
                                        bool overrideTextureRectMatrix = false, TextureRectMatrixf textureRectMatrix = default(TextureRectMatrixf), bool overridePerLayerColorScaleAndOffset = false, Vector4 colorScale = default(Vector4), Vector4 colorOffset = default(Vector4))
    {
        if (!initialized)
            return false;
        if (version >= OVRP_1_6_0.version)
        {
            var flags = (uint)OverlayFlag.None;
            if (onTop) flags |= (uint)OverlayFlag.OnTop;
            if (headLocked) flags |= (uint)OverlayFlag.HeadLocked;
            if (noDepthBufferTesting) flags |= (uint)OverlayFlag.NoDepth;
            if (shape == OverlayShape.Cylinder || shape == OverlayShape.Cubemap)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (version >= OVRP_1_7_0.version) flags |= (uint)shape << OverlayShapeFlagShift;
                    else return false;
                }
                else
                {
                    if (shape == OverlayShape.Cubemap && version >= OVRP_1_10_0.version) flags |= (uint)shape << OverlayShapeFlagShift;
                    else if (shape == OverlayShape.Cylinder && version >= OVRP_1_16_0.version) flags |= (uint)shape << OverlayShapeFlagShift;
                    else return false;
                }
            }
            if (shape == OverlayShape.OffcenterCubemap)
            {
                if (Application.platform == RuntimePlatform.Android)
                    if (version >= OVRP_1_11_0.version) flags |= (uint)shape << OverlayShapeFlagShift;
                    else return false;
                else return false;
            }
            if (shape == OverlayShape.Equirect)
            {
                if (Application.platform == RuntimePlatform.Android)
                    if (version >= OVRP_1_21_0.version) flags |= (uint)shape << OverlayShapeFlagShift;
                    else return false;
                else return false;
            }
            if (version >= OVRP_1_34_0.version && layerId != -1)
                return OVRP_1_34_0.ovrp_EnqueueSubmitLayer2(flags, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex,
                overrideTextureRectMatrix ? Bool.True : Bool.False, ref textureRectMatrix, overridePerLayerColorScaleAndOffset ? Bool.True : Bool.False, ref colorScale, ref colorOffset) == Result.Success;
            else if (version >= OVRP_1_15_0.version && layerId != -1) return OVRP_1_15_0.ovrp_EnqueueSubmitLayer(flags, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex) == Result.Success;
            return OVRP_1_6_0.ovrp_SetOverlayQuad3(flags, leftTexture, rightTexture, IntPtr.Zero, pose, scale, layerIndex) == Bool.True;
        }
        if (layerIndex != 0)
            return false;
        return OVRP_0_1_1.ovrp_SetOverlayQuad2(ToBool(onTop), ToBool(headLocked), leftTexture, IntPtr.Zero, pose, scale) == Bool.True;
    }

    public static LayerDesc CalculateLayerDesc(OverlayShape shape, LayerLayout layout, Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags)
    {
        var layerDesc = new LayerDesc();
        if (!initialized)
            return layerDesc;
        if (version >= OVRP_1_15_0.version)
            OVRP_1_15_0.ovrp_CalculateLayerDesc(shape, layout, ref textureSize, mipLevels, sampleCount, format, layerFlags, ref layerDesc);
        return layerDesc;
    }

    public static bool EnqueueSetupLayer(LayerDesc desc, int compositionDepth, IntPtr layerID)
    {
        if (!initialized)
            return false;
        if (version >= OVRP_1_28_0.version)
            return OVRP_1_28_0.ovrp_EnqueueSetupLayer2(ref desc, compositionDepth, layerID) == Result.Success;
        else if (version >= OVRP_1_15_0.version)
        {
            if (compositionDepth != 0)
                Debug.LogWarning("Use Oculus Plugin 1.28.0 or above to support non-zero compositionDepth");
            return OVRP_1_15_0.ovrp_EnqueueSetupLayer(ref desc, layerID) == Result.Success;
        }
        return false;
    }

    public static bool EnqueueDestroyLayer(IntPtr layerID) => !initialized
        ? false
        : version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_EnqueueDestroyLayer(layerID) == Result.Success : false;

    public static IntPtr GetLayerTexture(int layerId, int stage, Eye eyeId)
    {
        var textureHandle = IntPtr.Zero;
        if (!initialized)
            return textureHandle;
        if (version >= OVRP_1_15_0.version)
            OVRP_1_15_0.ovrp_GetLayerTexturePtr(layerId, stage, eyeId, ref textureHandle);
        return textureHandle;
    }

    public static int GetLayerTextureStageCount(int layerId)
    {
        if (!initialized)
            return 1;
        var stageCount = 1;
        if (version >= OVRP_1_15_0.version)
            OVRP_1_15_0.ovrp_GetLayerTextureStageCount(layerId, ref stageCount);
        return stageCount;
    }

    public static IntPtr GetLayerAndroidSurfaceObject(int layerId)
    {
        var surfaceObject = IntPtr.Zero;
        if (!initialized)
            return surfaceObject;
        if (version >= OVRP_1_29_0.version)
            OVRP_1_29_0.ovrp_GetLayerAndroidSurfaceObject(layerId, ref surfaceObject);
        return surfaceObject;
    }

    public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_Update2((int)Step.Physics, frameIndex, predictionSeconds) == Bool.True : false;
    public static Posef GetNodePose(Node nodeId, Step stepId) => version >= OVRP_1_12_0.version
        ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Pose
        : version >= OVRP_1_8_0.version && stepId == Step.Physics
        ? OVRP_1_8_0.ovrp_GetNodePose2(0, nodeId)
        : OVRP_0_1_2.ovrp_GetNodePose(nodeId);
    public static Vector3f GetNodeVelocity(Node nodeId, Step stepId) => version >= OVRP_1_12_0.version
        ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Velocity
        : version >= OVRP_1_8_0.version && stepId == Step.Physics
        ? OVRP_1_8_0.ovrp_GetNodeVelocity2(0, nodeId).Position
        : OVRP_0_1_3.ovrp_GetNodeVelocity(nodeId).Position;
    public static Vector3f GetNodeAngularVelocity(Node nodeId, Step stepId) => version >= OVRP_1_12_0.version ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularVelocity : new Vector3f();
    public static Vector3f GetNodeAcceleration(Node nodeId, Step stepId) => version >= OVRP_1_12_0.version
        ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Acceleration
        : version >= OVRP_1_8_0.version && stepId == Step.Physics
        ? OVRP_1_8_0.ovrp_GetNodeAcceleration2(0, nodeId).Position
        : OVRP_0_1_3.ovrp_GetNodeAcceleration(nodeId).Position;
    public static Vector3f GetNodeAngularAcceleration(Node nodeId, Step stepId) => version >= OVRP_1_12_0.version ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularAcceleration : new Vector3f();
    public static bool GetNodePresent(Node nodeId) => OVRP_1_1_0.ovrp_GetNodePresent(nodeId) == Bool.True;
    public static bool GetNodeOrientationTracked(Node nodeId) => OVRP_1_1_0.ovrp_GetNodeOrientationTracked(nodeId) == Bool.True;
    public static bool GetNodePositionTracked(Node nodeId) => OVRP_1_1_0.ovrp_GetNodePositionTracked(nodeId) == Bool.True;
    public static PoseStatef GetNodePoseStateRaw(Node nodeId, Step stepId) => version >= OVRP_1_29_0.version
        ? OVRP_1_29_0.ovrp_GetNodePoseStateRaw(stepId, -1, nodeId, out var nodePoseState) == Result.Success ? nodePoseState : PoseStatef.identity
        : version >= OVRP_1_12_0.version ? OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId) : PoseStatef.identity;
    public static Posef GetCurrentTrackingTransformPose() => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_GetCurrentTrackingTransformPose(out var trackingTransformPose) == Result.Success ? trackingTransformPose : Posef.identity
        : Posef.identity;
    public static Posef GetTrackingTransformRawPose() => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_GetTrackingTransformRawPose(out var trackingTransforRawPose) == Result.Success ? trackingTransforRawPose : Posef.identity
        : Posef.identity;
    public static ControllerState GetControllerState(uint controllerMask) => OVRP_1_1_0.ovrp_GetControllerState(controllerMask);
    public static ControllerState2 GetControllerState2(uint controllerMask) => version >= OVRP_1_12_0.version
            ? OVRP_1_12_0.ovrp_GetControllerState2(controllerMask)
            : new ControllerState2(OVRP_1_1_0.ovrp_GetControllerState(controllerMask));
    public static ControllerState4 GetControllerState4(uint controllerMask)
    {
        if (version >= OVRP_1_16_0.version)
        {
            var controllerState = new ControllerState4();
            OVRP_1_16_0.ovrp_GetControllerState4(controllerMask, ref controllerState);
            return controllerState;
        }
        return new ControllerState4(GetControllerState2(controllerMask));
    }
    public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude) => OVRP_0_1_2.ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == Bool.True;
    public static HapticsDesc GetControllerHapticsDesc(uint controllerMask) => version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetControllerHapticsDesc(controllerMask) : new HapticsDesc();
    public static HapticsState GetControllerHapticsState(uint controllerMask) => version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetControllerHapticsState(controllerMask) : new HapticsState();
    public static bool SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer) => version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == Bool.True : false;
    public static float GetEyeRecommendedResolutionScale() => version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetEyeRecommendedResolutionScale() : 1.0f;
    public static float GetAppCpuStartToGpuEndTime() => version >= OVRP_1_6_0.version ? OVRP_1_6_0.ovrp_GetAppCpuStartToGpuEndTime() : 0.0f;
    public static bool GetBoundaryConfigured() => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_GetBoundaryConfigured() == Bool.True : false;
    public static BoundaryTestResult TestBoundaryNode(Node nodeId, BoundaryType boundaryType) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_TestBoundaryNode(nodeId, boundaryType) : new BoundaryTestResult();
    public static BoundaryTestResult TestBoundaryPoint(Vector3f point, BoundaryType boundaryType) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_TestBoundaryPoint(point, boundaryType) : new BoundaryTestResult();
    public static BoundaryGeometry GetBoundaryGeometry(BoundaryType boundaryType) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_GetBoundaryGeometry(boundaryType) : new BoundaryGeometry();
    public static bool GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount)
    {
        if (version >= OVRP_1_9_0.version) return OVRP_1_9_0.ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == Bool.True;
        else { pointsCount = 0; return false; }
    }
    public static AppPerfStats GetAppPerfStats() => version >= OVRP_1_9_0.version ? OVRP_1_9_0.ovrp_GetAppPerfStats() : new AppPerfStats();
    public static bool ResetAppPerfStats() => version >= OVRP_1_9_0.version ? OVRP_1_9_0.ovrp_ResetAppPerfStats() == Bool.True : false;
    public static float GetAppFramerate() => version >= OVRP_1_12_0.version ? OVRP_1_12_0.ovrp_GetAppFramerate() : 0.0f;
    public static bool SetHandNodePoseStateLatency(double latencyInSeconds) => version >= OVRP_1_18_0.version
        ? OVRP_1_18_0.ovrp_SetHandNodePoseStateLatency(latencyInSeconds) == Result.Success ? true : false
        : false;
    public static double GetHandNodePoseStateLatency() => version >= OVRP_1_18_0.version
        ? OVRP_1_18_0.ovrp_GetHandNodePoseStateLatency(out var value) == Result.Success ? value : 0.0
        : 0.0;
    public static EyeTextureFormat GetDesiredEyeTextureFormat()
    {
        if (version >= OVRP_1_11_0.version)
        {
            var eyeTextureFormatValue = (uint)OVRP_1_11_0.ovrp_GetDesiredEyeTextureFormat();
            // convert both R8G8B8A8 and R8G8B8A8_SRGB to R8G8B8A8 here for avoid confusing developers
            if (eyeTextureFormatValue == 1)
                eyeTextureFormatValue = 0;
            return (EyeTextureFormat)eyeTextureFormatValue;
        }
        else return EyeTextureFormat.Default;
    }
    public static bool SetDesiredEyeTextureFormat(EyeTextureFormat value) => version >= OVRP_1_11_0.version ? OVRP_1_11_0.ovrp_SetDesiredEyeTextureFormat(value) == Bool.True : false;
    public static bool InitializeMixedReality() => version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_InitializeMixedReality() == Result.Success : false;
    public static bool ShutdownMixedReality() => version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_ShutdownMixedReality() == Result.Success : false;
    public static bool IsMixedRealityInitialized() => version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_GetMixedRealityInitialized() == Bool.True : false;
    public static int GetExternalCameraCount() => version >= OVRP_1_15_0.version
        ? OVRP_1_15_0.ovrp_GetExternalCameraCount(out var cameraCount) != Result.Success ? 0 : cameraCount
        : 0;
    public static bool UpdateExternalCamera() => version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_UpdateExternalCamera() == Result.Success : false;
    public static bool GetMixedRealityCameraInfo(int cameraId, out CameraExtrinsics cameraExtrinsics, out CameraIntrinsics cameraIntrinsics)
    {
        cameraExtrinsics = default(CameraExtrinsics);
        cameraIntrinsics = default(CameraIntrinsics);
        if (version >= OVRP_1_15_0.version)
        {
            var retValue = true;
            var result = OVRP_1_15_0.ovrp_GetExternalCameraExtrinsics(cameraId, out cameraExtrinsics);
            if (result != Result.Success)
                retValue = false; //Debug.LogWarning("ovrp_GetExternalCameraExtrinsics return " + result);
            result = OVRP_1_15_0.ovrp_GetExternalCameraIntrinsics(cameraId, out cameraIntrinsics);
            if (result != Result.Success)
                retValue = false; //Debug.LogWarning("ovrp_GetExternalCameraIntrinsics return " + result);
            return retValue;
        }
        else return false;
    }
    public static Vector3f GetBoundaryDimensions(BoundaryType boundaryType) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_GetBoundaryDimensions(boundaryType) : new Vector3f();
    public static bool GetBoundaryVisible() => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_GetBoundaryVisible() == Bool.True : false;
    public static bool SetBoundaryVisible(bool value) => version >= OVRP_1_8_0.version ? OVRP_1_8_0.ovrp_SetBoundaryVisible(ToBool(value)) == Bool.True : false;
    public static SystemHeadset GetSystemHeadsetType() => version >= OVRP_1_9_0.version ? OVRP_1_9_0.ovrp_GetSystemHeadsetType() : SystemHeadset.None;
    public static Controller GetActiveController() => version >= OVRP_1_9_0.version ? OVRP_1_9_0.ovrp_GetActiveController() : Controller.None;
    public static Controller GetConnectedControllers() => version >= OVRP_1_9_0.version ? OVRP_1_9_0.ovrp_GetConnectedControllers() : Controller.None;
    static Bool ToBool(bool b) => b ? Bool.True : Bool.False;
    public static TrackingOrigin TrackingOriginType => OVRP_1_0_0.ovrp_GetTrackingOriginType();
    public static bool SetTrackingOriginType(TrackingOrigin originType) => OVRP_1_0_0.ovrp_SetTrackingOriginType(originType) == Bool.True;
    public static Posef GetTrackingCalibratedOrigin() => OVRP_1_0_0.ovrp_GetTrackingCalibratedOrigin();
    public static bool SetTrackingCalibratedOrigin() => OVRP_1_2_0.ovrpi_SetTrackingCalibratedOrigin() == Bool.True;
    public static bool RecenterTrackingOrigin(RecenterFlags flags) => OVRP_1_0_0.ovrp_RecenterTrackingOrigin((uint)flags) == Bool.True;

#if _WIN
    public static bool UpdateCameraDevices() => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_UpdateCameraDevices() == Result.Success : false;
    public static bool IsCameraDeviceAvailable(CameraDevice cameraDevice) => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_IsCameraDeviceAvailable(cameraDevice) == Bool.True : false;
    public static bool SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, int width, int height) => version >= OVRP_1_16_0.version
        ? OVRP_1_16_0.ovrp_SetCameraDevicePreferredColorFrameSize(cameraDevice, new Sizei { w = width, h = height }) == Result.Success
        : false;
    public static bool OpenCameraDevice(CameraDevice cameraDevice) => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_OpenCameraDevice(cameraDevice) == Result.Success : false;
    public static bool CloseCameraDevice(CameraDevice cameraDevice) => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_CloseCameraDevice(cameraDevice) == Result.Success : false;
    public static bool HasCameraDeviceOpened(CameraDevice cameraDevice) => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_HasCameraDeviceOpened(cameraDevice) == Bool.True : false;
    public static bool IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice) => version >= OVRP_1_16_0.version ? OVRP_1_16_0.ovrp_IsCameraDeviceColorFrameAvailable(cameraDevice) == Bool.True : false;

    static Texture2D cachedCameraFrameTexture = null;
    public static Texture2D GetCameraDeviceColorFrameTexture(CameraDevice cameraDevice)
    {
        if (version >= OVRP_1_16_0.version)
        {
            var result = OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameSize(cameraDevice, out var size);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceColorFrameSize return " + result);
                return null;
            }
            result = OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameBgraPixels(cameraDevice, out var pixels, out var rowPitch);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceColorFrameBgraPixels return " + result);
                return null;
            }
            if (rowPitch != size.w * 4)
            {
                //Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", size.w * 4, rowPitch));
                return null;
            }
            if (!cachedCameraFrameTexture || cachedCameraFrameTexture.width != size.w || cachedCameraFrameTexture.height != size.h)
                cachedCameraFrameTexture = new Texture2D(size.w, size.h, TextureFormat.BGRA32, false);
            cachedCameraFrameTexture.LoadRawTextureData(pixels, rowPitch * size.h);
            cachedCameraFrameTexture.Apply();
            return cachedCameraFrameTexture;
        }
        else return null;
    }

    public static bool DoesCameraDeviceSupportDepth(CameraDevice cameraDevice) => version >= OVRP_1_17_0.version
        ? OVRP_1_17_0.ovrp_DoesCameraDeviceSupportDepth(cameraDevice, out var supportDepth) == Result.Success && supportDepth == Bool.True
        : false;
    public static bool SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode) => version >= OVRP_1_17_0.version
        ? OVRP_1_17_0.ovrp_SetCameraDeviceDepthSensingMode(camera, depthSensoringMode) == Result.Success
        : false;
    public static bool SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality) => version >= OVRP_1_17_0.version
        ? OVRP_1_17_0.ovrp_SetCameraDevicePreferredDepthQuality(camera, depthQuality) == Result.Success
        : false;
    public static bool IsCameraDeviceDepthFrameAvailable(CameraDevice cameraDevice) => version >= OVRP_1_17_0.version
        ? OVRP_1_17_0.ovrp_IsCameraDeviceDepthFrameAvailable(cameraDevice, out var available) == Result.Success && available == Bool.True
        : false;

    static Texture2D cachedCameraDepthTexture = null;
    public static Texture2D GetCameraDeviceDepthFrameTexture(CameraDevice cameraDevice)
    {
        if (version >= OVRP_1_17_0.version)
        {
            var result = OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out var size);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceDepthFrameSize return " + result);
                return null;
            }
            result = OVRP_1_17_0.ovrp_GetCameraDeviceDepthFramePixels(cameraDevice, out var depthData, out var rowPitch);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceDepthFramePixels return " + result);
                return null;
            }
            if (rowPitch != size.w * 4)
            {
                //Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", size.w * 4, rowPitch));
                return null;
            }
            if (!cachedCameraDepthTexture || cachedCameraDepthTexture.width != size.w || cachedCameraDepthTexture.height != size.h)
            {
                cachedCameraDepthTexture = new Texture2D(size.w, size.h, TextureFormat.RFloat, false);
                cachedCameraDepthTexture.filterMode = FilterMode.Point;
            }
            cachedCameraDepthTexture.LoadRawTextureData(depthData, rowPitch * size.h);
            cachedCameraDepthTexture.Apply();
            return cachedCameraDepthTexture;
        }
        else return null;
    }

    static Texture2D cachedCameraDepthConfidenceTexture = null;
    public static Texture2D GetCameraDeviceDepthConfidenceTexture(CameraDevice cameraDevice)
    {
        if (version >= OVRP_1_17_0.version)
        {
            var result = OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out var size);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceDepthFrameSize return " + result);
                return null;
            }
            result = OVRP_1_17_0.ovrp_GetCameraDeviceDepthConfidencePixels(cameraDevice, out var confidenceData, out var rowPitch);
            if (result != Result.Success)
            {
                //Debug.LogWarning("ovrp_GetCameraDeviceDepthConfidencePixels return " + result);
                return null;
            }
            if (rowPitch != size.w * 4)
            {
                //Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", size.w * 4, rowPitch));
                return null;
            }
            if (!cachedCameraDepthConfidenceTexture || cachedCameraDepthConfidenceTexture.width != size.w || cachedCameraDepthConfidenceTexture.height != size.h)
                cachedCameraDepthConfidenceTexture = new Texture2D(size.w, size.h, TextureFormat.RFloat, false);
            cachedCameraDepthConfidenceTexture.LoadRawTextureData(confidenceData, rowPitch * size.h);
            cachedCameraDepthConfidenceTexture.Apply();
            return cachedCameraDepthConfidenceTexture;
        }
        else return null;
    }
#endif

    public static bool tiledMultiResSupported => version >= OVRP_1_21_0.version
        ? OVRP_1_21_0.ovrp_GetTiledMultiResSupported(out var supported) == Result.Success ? supported == Bool.True : false
        : false;
    public static TiledMultiResLevel tiledMultiResLevel
    {
        get
        {
            if (version >= OVRP_1_21_0.version && tiledMultiResSupported)
            {
                OVRP_1_21_0.ovrp_GetTiledMultiResLevel(out var level);
                return level;
            }
            else return TiledMultiResLevel.Off;
        }
        set
        {
            if (version >= OVRP_1_21_0.version && tiledMultiResSupported)
                OVRP_1_21_0.ovrp_SetTiledMultiResLevel(value);
        }
    }
    public static bool gpuUtilSupported => version >= OVRP_1_21_0.version
        ? OVRP_1_21_0.ovrp_GetGPUUtilSupported(out var supported) == Result.Success ? supported == Bool.True : false
        : false;
    public static float gpuUtilLevel => version >= OVRP_1_21_0.version && gpuUtilSupported
        ? OVRP_1_21_0.ovrp_GetGPUUtilLevel(out var level) == Result.Success ? level : 0.0f
        : 0.0f;

    static OVRNativeBuffer _nativeSystemDisplayFrequenciesAvailable = null;
    static float[] _cachedSystemDisplayFrequenciesAvailable = null;

    public static float[] systemDisplayFrequenciesAvailable
    {
        get
        {
            if (_cachedSystemDisplayFrequenciesAvailable == null)
            {
                _cachedSystemDisplayFrequenciesAvailable = new float[0];
                if (version >= OVRP_1_21_0.version)
                {
                    var numFrequencies = 0;
                    if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(IntPtr.Zero, ref numFrequencies) == Result.Success)
                        if (numFrequencies > 0)
                        {
                            var maxNumElements = numFrequencies;
                            _nativeSystemDisplayFrequenciesAvailable = new OVRNativeBuffer(sizeof(float) * maxNumElements);
                            if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), ref numFrequencies) == Result.Success)
                            {
                                var numElementsToCopy = numFrequencies <= maxNumElements ? numFrequencies : maxNumElements;
                                if (numElementsToCopy > 0)
                                {
                                    _cachedSystemDisplayFrequenciesAvailable = new float[numElementsToCopy];
                                    Marshal.Copy(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), _cachedSystemDisplayFrequenciesAvailable, 0, numElementsToCopy);
                                }
                            }
                        }
                }
            }
            return _cachedSystemDisplayFrequenciesAvailable;
        }
    }
    public static float systemDisplayFrequency
    {
        get => version >= OVRP_1_21_0.version
            ? OVRP_1_21_0.ovrp_GetSystemDisplayFrequency2(out var displayFrequency) == Result.Success ? displayFrequency : 0.0f
            : version >= OVRP_1_1_0.version ? OVRP_1_1_0.ovrp_GetSystemDisplayFrequency() : 0.0f;
        set
        {
            if (version >= OVRP_1_21_0.version)
                OVRP_1_21_0.ovrp_SetSystemDisplayFrequency(value);
        }
    }
    public static bool GetNodeFrustum2(Node nodeId, out Frustumf2 frustum)
    {
        frustum = default(Frustumf2);
        return version >= OVRP_1_15_0.version
            ? OVRP_1_15_0.ovrp_GetNodeFrustum2(nodeId, out frustum) != Result.Success ? false : true
            : false;
    }
    public static bool AsymmetricFovEnabled => version >= OVRP_1_21_0.version
        ? OVRP_1_21_0.ovrp_GetAppAsymmetricFov(out var asymmetricFovEnabled) != Result.Success
            ? false
            : asymmetricFovEnabled == Bool.True
        : false;
    public static bool EyeTextureArrayEnabled => version >= OVRP_1_15_0.version ? OVRP_1_15_0.ovrp_GetEyeTextureArrayEnabled() == Bool.True : false;
    public static Handedness GetDominantHand() => version >= OVRP_1_28_0.version && OVRP_1_28_0.ovrp_GetDominantHand(out var dominantHand) == Result.Success
        ? dominantHand
        : Handedness.Unsupported;
    public static bool GetReorientHMDOnControllerRecenter() => version < OVRP_1_28_0.version || OVRP_1_28_0.ovrp_GetReorientHMDOnControllerRecenter(out var recenterMode) != Result.Success
        ? false
        : recenterMode == Bool.True;
    public static bool SetReorientHMDOnControllerRecenter(bool recenterSetting) => version < OVRP_1_28_0.version || OVRP_1_28_0.ovrp_SetReorientHMDOnControllerRecenter(recenterSetting ? Bool.True : Bool.False) == Result.Success ? false : true;
    public static bool SendEvent(string name, string param = "", string source = "") => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_SendEvent2(name, param, source.Length == 0 ? "integration" : source) == Result.Success
        : version >= OVRP_1_28_0.version ? OVRP_1_28_0.ovrp_SendEvent(name, param) == Result.Success : false;
    public static bool SetHeadPoseModifier(ref Quatf relativeRotation, ref Vector3f relativeTranslation) => version >= OVRP_1_29_0.version
        ? OVRP_1_29_0.ovrp_SetHeadPoseModifier(ref relativeRotation, ref relativeTranslation) == Result.Success
        : false;
    public static bool GetHeadPoseModifier(out Quatf relativeRotation, out Vector3f relativeTranslation)
    {
        if (version >= OVRP_1_29_0.version)
            return OVRP_1_29_0.ovrp_GetHeadPoseModifier(out relativeRotation, out relativeTranslation) == Result.Success;
        relativeRotation = Quatf.identity;
        relativeTranslation = Vector3f.zero;
        return false;
    }
    public static bool IsPerfMetricsSupported(PerfMetrics perfMetrics) => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_IsPerfMetricsSupported(perfMetrics, out var isSupported) == Result.Success ? isSupported == Bool.True : false
        : false;
    public static float? GetPerfMetricsFloat(PerfMetrics perfMetrics) => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_GetPerfMetricsFloat(perfMetrics, out var value) == Result.Success ? value : (float?)null
        : null;
    public static int? GetPerfMetricsInt(PerfMetrics perfMetrics) => version >= OVRP_1_30_0.version
        ? OVRP_1_30_0.ovrp_GetPerfMetricsInt(perfMetrics, out var value) == Result.Success ? value : (int?)null
        : null;
    public static double GetTimeInSeconds() => version >= OVRP_1_31_0.version ? OVRP_1_31_0.ovrp_GetTimeInSeconds(out var value) == Result.Success ? value : 0.0 : 0.0;
    public static bool SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, bool applyToAllLayers) => version >= OVRP_1_31_0.version
        ? OVRP_1_31_0.ovrp_SetColorScaleAndOffset(colorScale, colorOffset, applyToAllLayers ? Bool.True : Bool.False) == Result.Success
        : false;
    public static bool AddCustomMetadata(string name, string param = "") => version >= OVRP_1_32_0.version ? OVRP_1_32_0.ovrp_AddCustomMetadata(name, param) == Result.Success : false;

    const string pluginName = "OVRPlugin";
    static Version _versionZero = new Version(0, 0, 0);

    // Disable all the DllImports when the platform is not supported

    static class OVRP_0_1_0
    {
        public static readonly Version version = new Version(0, 1, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Sizei ovrp_GetEyeTextureSize(Eye eyeId);
    }

    static class OVRP_0_1_1
    {
        public static readonly Version version = new Version(0, 1, 1);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetOverlayQuad2(Bool onTop, Bool headLocked, IntPtr texture, IntPtr device, Posef pose, Vector3f scale);
    }

    static class OVRP_0_1_2
    {
        public static readonly Version version = new Version(0, 1, 2);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodePose(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);
    }

    static class OVRP_0_1_3
    {
        public static readonly Version version = new Version(0, 1, 3);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodeVelocity(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodeAcceleration(Node nodeId);
    }

    static class OVRP_0_5_0
    {
        public static readonly Version version = new Version(0, 5, 0);
    }

    static class OVRP_1_0_0
    {
        public static readonly Version version = new Version(1, 0, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern TrackingOrigin ovrp_GetTrackingOriginType();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetTrackingOriginType(TrackingOrigin originType);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetTrackingCalibratedOrigin();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_RecenterTrackingOrigin(uint flags);
    }

    static class OVRP_1_1_0
    {
        public static readonly Version version = new Version(1, 1, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetInitialized();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")] static extern IntPtr _ovrp_GetVersion();
        public static string ovrp_GetVersion() => Marshal.PtrToStringAnsi(_ovrp_GetVersion());
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetNativeSDKVersion")] static extern IntPtr _ovrp_GetNativeSDKVersion();
        public static string ovrp_GetNativeSDKVersion() => Marshal.PtrToStringAnsi(_ovrp_GetNativeSDKVersion());
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovrp_GetAudioOutId();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern IntPtr ovrp_GetAudioInId();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetEyeTextureScale();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetEyeTextureScale(float value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetTrackingOrientationSupported();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetTrackingOrientationEnabled();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetTrackingOrientationEnabled(Bool value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetTrackingPositionSupported();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetTrackingPositionEnabled();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetTrackingPositionEnabled(Bool value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetNodePresent(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetNodeOrientationTracked(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetNodePositionTracked(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Frustumf ovrp_GetNodeFrustum(Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern ControllerState ovrp_GetControllerState(uint controllerMask);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern int ovrp_GetSystemCpuLevel();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetSystemCpuLevel(int value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern int ovrp_GetSystemGpuLevel();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetSystemGpuLevel(int value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetSystemPowerSavingMode();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetSystemDisplayFrequency();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern int ovrp_GetSystemVSyncCount();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetSystemVolume();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern BatteryStatus ovrp_GetSystemBatteryStatus();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetSystemBatteryLevel();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetSystemBatteryTemperature();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetSystemProductName")] static extern IntPtr _ovrp_GetSystemProductName();
        public static string ovrp_GetSystemProductName() => Marshal.PtrToStringAnsi(_ovrp_GetSystemProductName());

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_ShowSystemUI(PlatformUI ui);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetAppMonoscopic();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetAppMonoscopic(Bool value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetAppHasVrFocus();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetAppShouldQuit();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetAppShouldRecenter();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetAppLatencyTimings")] static extern IntPtr _ovrp_GetAppLatencyTimings();
        public static string ovrp_GetAppLatencyTimings() => Marshal.PtrToStringAnsi(_ovrp_GetAppLatencyTimings());

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetUserPresent();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetUserIPD();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetUserIPD(float value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetUserEyeDepth();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetUserEyeDepth(float value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetUserEyeHeight();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetUserEyeHeight(float value);
    }

    static class OVRP_1_2_0
    {
        public static readonly Version version = new Version(1, 2, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetSystemVSyncCount(int vsyncCount);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrpi_SetTrackingCalibratedOrigin();
    }

    static class OVRP_1_3_0
    {
        public static readonly Version version = new Version(1, 3, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetEyeOcclusionMeshEnabled();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetEyeOcclusionMeshEnabled(Bool value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetSystemHeadphonesPresent();
    }

    static class OVRP_1_5_0
    {
        public static readonly Version version = new Version(1, 5, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern SystemRegion ovrp_GetSystemRegion();
    }

    static class OVRP_1_6_0
    {
        public static readonly Version version = new Version(1, 6, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetTrackingIPDEnabled();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetTrackingIPDEnabled(Bool value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern HapticsState ovrp_GetControllerHapticsState(uint controllerMask);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetOverlayQuad3(uint flags, IntPtr textureLeft, IntPtr textureRight, IntPtr device, Posef pose, Vector3f scale, int layerIndex);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetEyeRecommendedResolutionScale();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetAppCpuStartToGpuEndTime();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern int ovrp_GetSystemRecommendedMSAALevel();
    }

    static class OVRP_1_7_0
    {
        public static readonly Version version = new Version(1, 7, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetAppChromaticCorrection();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetAppChromaticCorrection(Bool value);
    }

    static class OVRP_1_8_0
    {
        public static readonly Version version = new Version(1, 8, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetBoundaryConfigured();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern BoundaryTestResult ovrp_TestBoundaryNode(Node nodeId, BoundaryType boundaryType);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern BoundaryTestResult ovrp_TestBoundaryPoint(Vector3f point, BoundaryType boundaryType);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern BoundaryGeometry ovrp_GetBoundaryGeometry(BoundaryType boundaryType);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Vector3f ovrp_GetBoundaryDimensions(BoundaryType boundaryType);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetBoundaryVisible();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetBoundaryVisible(Bool value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodePose2(int stateId, Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodeVelocity2(int stateId, Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Posef ovrp_GetNodeAcceleration2(int stateId, Node nodeId);
    }

    static class OVRP_1_9_0
    {
        public static readonly Version version = new Version(1, 9, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern SystemHeadset ovrp_GetSystemHeadsetType();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Controller ovrp_GetActiveController();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Controller ovrp_GetConnectedControllers();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern AppPerfStats ovrp_GetAppPerfStats();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_ResetAppPerfStats();
    }

    static class OVRP_1_10_0
    {
        public static readonly Version version = new Version(1, 10, 0);
    }

    static class OVRP_1_11_0
    {
        public static readonly Version version = new Version(1, 11, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_SetDesiredEyeTextureFormat(EyeTextureFormat value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern EyeTextureFormat ovrp_GetDesiredEyeTextureFormat();
    }

    static class OVRP_1_12_0
    {
        public static readonly Version version = new Version(1, 12, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern float ovrp_GetAppFramerate();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern PoseStatef ovrp_GetNodePoseState(Step stepId, Node nodeId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern ControllerState2 ovrp_GetControllerState2(uint controllerMask);
    }

    static class OVRP_1_15_0
    {
        public const int OVRP_EXTERNAL_CAMERA_NAME_SIZE = 32;
        public static readonly Version version = new Version(1, 15, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_InitializeMixedReality();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_ShutdownMixedReality();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetMixedRealityInitialized();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_UpdateExternalCamera();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetExternalCameraCount(out int cameraCount);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetExternalCameraName(int cameraId, [MarshalAs(UnmanagedType.LPArray, SizeConst = OVRP_EXTERNAL_CAMERA_NAME_SIZE)] char[] cameraName);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetExternalCameraIntrinsics(int cameraId, out CameraIntrinsics cameraIntrinsics);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetExternalCameraExtrinsics(int cameraId, out CameraExtrinsics cameraExtrinsics);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_CalculateLayerDesc(OverlayShape shape, LayerLayout layout, ref Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags, ref LayerDesc layerDesc);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_EnqueueSetupLayer(ref LayerDesc desc, IntPtr layerId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_EnqueueDestroyLayer(IntPtr layerId);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetLayerTextureStageCount(int layerId, ref int layerTextureStageCount);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetLayerTexturePtr(int layerId, int stage, Eye eyeId, ref IntPtr textureHandle);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_EnqueueSubmitLayer(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref Posef pose, ref Vector3f scale, int layerIndex);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetNodeFrustum2(Node nodeId, out Frustumf2 nodeFrustum);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_GetEyeTextureArrayEnabled();
    }

    static class OVRP_1_16_0
    {
        public static readonly Version version = new Version(1, 16, 0);
#if _WIN
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_UpdateCameraDevices();
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_IsCameraDeviceAvailable(CameraDevice cameraDevice);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, Sizei preferredColorFrameSize);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_OpenCameraDevice(CameraDevice cameraDevice);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_CloseCameraDevice(CameraDevice cameraDevice);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_HasCameraDeviceOpened(CameraDevice cameraDevice);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Bool ovrp_IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceColorFrameSize(CameraDevice cameraDevice, out Sizei colorFrameSize);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceColorFrameBgraPixels(CameraDevice cameraDevice, out IntPtr colorFrameBgraPixels, out int colorFrameRowPitch);
#endif
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetControllerState4(uint controllerMask, ref ControllerState4 controllerState);
    }

    static class OVRP_1_17_0
    {
        public static readonly Version version = new Version(1, 17, 0);
#if _WIN
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetExternalCameraPose(CameraDevice camera, out Posef cameraPose);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_ConvertPoseToCameraSpace(CameraDevice camera, ref Posef trackingSpacePose, out Posef cameraSpacePose);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceIntrinsicsParameters(CameraDevice camera, out Bool supportIntrinsics, out CameraDeviceIntrinsicsParameters intrinsicsParameters);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_DoesCameraDeviceSupportDepth(CameraDevice camera, out Bool supportDepth);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceDepthSensingMode(CameraDevice camera, out CameraDeviceDepthSensingMode depthSensoringMode);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDevicePreferredDepthQuality(CameraDevice camera, out CameraDeviceDepthQuality depthQuality);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_IsCameraDeviceDepthFrameAvailable(CameraDevice camera, out Bool available);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceDepthFrameSize(CameraDevice camera, out Sizei depthFrameSize);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceDepthFramePixels(CameraDevice cameraDevice, out IntPtr depthFramePixels, out int depthFrameRowPitch);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCameraDeviceDepthConfidencePixels(CameraDevice cameraDevice, out IntPtr depthConfidencePixels, out int depthConfidenceRowPitch);
#endif
    }

    static class OVRP_1_18_0
    {
        public static readonly Version version = new Version(1, 18, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetHandNodePoseStateLatency(double latencyInSeconds);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetHandNodePoseStateLatency(out double latencyInSeconds);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetAppHasInputFocus(out Bool appHasInputFocus);
    }

    static class OVRP_1_19_0
    {
        public static readonly Version version = new Version(1, 19, 0);
    }

    static class OVRP_1_21_0
    {
        public static readonly Version version = new Version(1, 21, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetTiledMultiResSupported(out Bool foveationSupported);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetTiledMultiResLevel(out TiledMultiResLevel level);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetTiledMultiResLevel(TiledMultiResLevel level);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetGPUUtilSupported(out Bool gpuUtilSupported);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetGPUUtilLevel(out float gpuUtil);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetSystemDisplayFrequency2(out float systemDisplayFrequency);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetSystemDisplayAvailableFrequencies(IntPtr systemDisplayAvailableFrequencies, ref int numFrequencies);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetSystemDisplayFrequency(float requestedFrequency);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetAppAsymmetricFov(out Bool useAsymmetricFov);
    }

    static class OVRP_1_28_0
    {
        public static readonly Version version = new Version(1, 28, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetDominantHand(out Handedness dominantHand);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetReorientHMDOnControllerRecenter(out Bool recenter);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetReorientHMDOnControllerRecenter(Bool recenter);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SendEvent(string name, string param);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_EnqueueSetupLayer2(ref LayerDesc desc, int compositionDepth, IntPtr layerId);
    }

    static class OVRP_1_29_0
    {
        public static readonly Version version = new Version(1, 29, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetLayerAndroidSurfaceObject(int layerId, ref IntPtr surfaceObject);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetHeadPoseModifier(ref Quatf relativeRotation, ref Vector3f relativeTranslation);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetHeadPoseModifier(out Quatf relativeRotation, out Vector3f relativeTranslation);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetNodePoseStateRaw(Step stepId, int frameIndex, Node nodeId, out PoseStatef nodePoseState);
    }

    static class OVRP_1_30_0
    {
        public static readonly Version version = new Version(1, 30, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetCurrentTrackingTransformPose(out Posef trackingTransformPose);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetTrackingTransformRawPose(out Posef trackingTransformRawPose);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SendEvent2(string name, string param, string source);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_IsPerfMetricsSupported(PerfMetrics perfMetrics, out Bool isSupported);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetPerfMetricsFloat(PerfMetrics perfMetrics, out float value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetPerfMetricsInt(PerfMetrics perfMetrics, out int value);
    }

    static class OVRP_1_31_0
    {
        public static readonly Version version = new Version(1, 31, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_GetTimeInSeconds(out double value);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, Bool applyToAllLayers);
    }

    static class OVRP_1_32_0
    {
        public static readonly Version version = new Version(1, 32, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_AddCustomMetadata(string name, string param);
    }

    static class OVRP_1_34_0
    {
        public static readonly Version version = new Version(1, 34, 0);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)] public static extern Result ovrp_EnqueueSubmitLayer2(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref Posef pose, ref Vector3f scale, int layerIndex, Bool overrideTextureRectMatrix, ref TextureRectMatrixf textureRectMatrix, Bool overridePerLayerColorScaleAndOffset, ref Vector4 colorScale, ref Vector4 colorOffset);
    }

    static class OVRP_1_35_0
    {
        public static readonly Version version = new Version(1, 35, 0);
    }

    static class OVRP_1_36_0
    {
        public static readonly Version version = new Version(1, 36, 0);
    }

    static class OVRP_1_37_0
    {
        public static readonly Version version = new Version(1, 37, 0);
    }
}
