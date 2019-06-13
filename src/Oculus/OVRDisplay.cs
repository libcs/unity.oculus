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

using System.Text.RegularExpressions;
using UnityEngine;
using InputTracking = UnityEngine.XR.InputTracking;
using Node = UnityEngine.XR.XRNode;
using Settings = UnityEngine.XR.XRSettings;

/// <summary>
/// Manages an Oculus Rift head-mounted display (HMD).
/// </summary>
public class OVRDisplay
{
    /// <summary>
    /// Contains full fov information per eye
    /// Under Symmetric Fov mode, UpFov == DownFov and LeftFov == RightFov.
    /// </summary>
    public struct EyeFov
    {
        public float UpFov;
        public float DownFov;
        public float LeftFov;
        public float RightFov;
    }

    /// <summary>
    /// Specifies the size and field-of-view for one eye texture.
    /// </summary>
    public struct EyeRenderDesc
    {
        /// <summary>
        /// The horizontal and vertical size of the texture.
        /// </summary>
        public Vector2 resolution;

        /// <summary>
        /// The angle of the horizontal and vertical field of view in degrees.
        /// For Symmetric FOV interface compatibility
        /// Note this includes the fov angle from both sides
        /// </summary>
        public Vector2 fov;

        /// <summary>
        /// The full information of field of view in degrees.
        /// When Asymmetric FOV isn't enabled, this returns the maximum fov angle
        /// </summary>
        public EyeFov fullFov;
    }

    /// <summary>
    /// Contains latency measurements for a single frame of rendering.
    /// </summary>
    public struct LatencyData
    {
        /// <summary>
        /// The time it took to render both eyes in seconds.
        /// </summary>
        public float render;

        /// <summary>
        /// The time it took to perform TimeWarp in seconds.
        /// </summary>
        public float timeWarp;

        /// <summary>
        /// The time between the end of TimeWarp and scan-out in seconds.
        /// </summary>
        public float postPresent;
        public float renderError;
        public float timeWarpError;
    }

    bool needsConfigureTexture;
    EyeRenderDesc[] eyeDescs = new EyeRenderDesc[2];
    bool recenterRequested = false;
    int recenterRequestedFrameCount = int.MaxValue;

    /// <summary>
    /// Creates an instance of OVRDisplay. Called by OVRManager.
    /// </summary>
    public OVRDisplay() => UpdateTextures();

    /// <summary>
    /// Updates the internal state of the OVRDisplay. Called by OVRManager.
    /// </summary>
    public void Update()
    {
        UpdateTextures();
        if (recenterRequested && Time.frameCount > recenterRequestedFrameCount)
        {
            RecenteredPose?.Invoke();
            recenterRequested = false;
            recenterRequestedFrameCount = int.MaxValue;
        }
    }

    /// <summary>
    /// Occurs when the head pose is reset.
    /// </summary>
    public event System.Action RecenteredPose;

    /// <summary>
    /// Recenters the head pose.
    /// </summary>
    public void RecenterPose()
    {
        InputTracking.Recenter();
        // The current poses are cached for the current frame and won't be updated immediately
        // after UnityEngine.VR.InputTracking.Recenter(). So we need to wait until next frame
        // to trigger the RecenteredPose delegate. The application could expect the correct pose
        // when the RecenteredPose delegate get called.
        recenterRequested = true;
        recenterRequestedFrameCount = Time.frameCount;
#if _WIN
		OVRMixedReality.RecenterPose();
#endif
    }

    /// <summary>
    /// Gets the current linear acceleration of the head.
    /// </summary>
    public Vector3 acceleration => !OVRManager.isHmdPresent
        ? Vector3.zero
        : OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.Head, NodeStatePropertyType.Acceleration, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec)
        ? retVec
        : Vector3.zero;

    /// <summary>
    /// Gets the current angular acceleration of the head in radians per second per second about each axis.
    /// </summary>
    public Vector3 angularAcceleration => !OVRManager.isHmdPresent
        ? Vector3.zero
        : OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.Head, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec)
        ? retVec
        : Vector3.zero;

    /// <summary>
    /// Gets the current linear velocity of the head in meters per second.
    /// </summary>
    public Vector3 velocity => !OVRManager.isHmdPresent
        ? Vector3.zero
        : OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.Head, NodeStatePropertyType.Velocity, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec)
        ? retVec
        : Vector3.zero;

    /// <summary>
    /// Gets the current angular velocity of the head in radians per second about each axis.
    /// </summary>
    public Vector3 angularVelocity => !OVRManager.isHmdPresent
        ? Vector3.zero
        : OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.Head, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec)
        ? retVec
        : Vector3.zero;

    /// <summary>
    /// Gets the resolution and field of view for the given eye.
    /// </summary>
    public EyeRenderDesc GetEyeRenderDesc(Node eye) => eyeDescs[(int)eye];

    /// <summary>
    /// Gets the current measured latency values.
    /// </summary>
    public LatencyData latency
    {
        get
        {
            if (!OVRManager.isHmdPresent)
                return new LatencyData();
            var latency = OVRPlugin.latency;
            var r = new Regex("Render: ([0-9]+[.][0-9]+)ms, TimeWarp: ([0-9]+[.][0-9]+)ms, PostPresent: ([0-9]+[.][0-9]+)ms", RegexOptions.None);
            var ret = new LatencyData();
            var match = r.Match(latency);
            if (match.Success)
            {
                ret.render = float.Parse(match.Groups[1].Value);
                ret.timeWarp = float.Parse(match.Groups[2].Value);
                ret.postPresent = float.Parse(match.Groups[3].Value);
            }
            return ret;
        }
    }

    /// <summary>
    /// Gets application's frame rate reported by oculus plugin
    /// </summary>
    public float appFramerate => !OVRManager.isHmdPresent ? 0 : OVRPlugin.GetAppFramerate();

    /// <summary>
    /// Gets the recommended MSAA level for optimal quality/performance the current device.
    /// </summary>
    public int recommendedMSAALevel
    {
        get
        {
            var result = OVRPlugin.recommendedMSAALevel;
            if (result == 1)
                result = 0;
            return result;
        }
    }

    /// <summary>
    /// Gets the list of available display frequencies supported by this hardware.
    /// </summary>
    public float[] displayFrequenciesAvailable => OVRPlugin.systemDisplayFrequenciesAvailable;

    /// <summary>
    /// Gets and sets the current display frequency.
    /// </summary>
    public float displayFrequency
    {
        get => OVRPlugin.systemDisplayFrequency;
        set => OVRPlugin.systemDisplayFrequency = value;
    }

    void UpdateTextures()
    {
        ConfigureEyeDesc(Node.LeftEye);
        ConfigureEyeDesc(Node.RightEye);
    }

    void ConfigureEyeDesc(Node eye)
    {
        if (!OVRManager.isHmdPresent)
            return;
        var eyeTextureWidth = Settings.eyeTextureWidth;
        var eyeTextureHeight = Settings.eyeTextureHeight;
        eyeDescs[(int)eye] = new EyeRenderDesc
        {
            resolution = new Vector2(eyeTextureWidth, eyeTextureHeight)
        };
        if (OVRPlugin.GetNodeFrustum2((OVRPlugin.Node)eye, out var frust))
        {
            eyeDescs[(int)eye].fullFov.LeftFov = Mathf.Rad2Deg * Mathf.Atan(frust.Fov.LeftTan);
            eyeDescs[(int)eye].fullFov.RightFov = Mathf.Rad2Deg * Mathf.Atan(frust.Fov.RightTan);
            eyeDescs[(int)eye].fullFov.UpFov = Mathf.Rad2Deg * Mathf.Atan(frust.Fov.UpTan);
            eyeDescs[(int)eye].fullFov.DownFov = Mathf.Rad2Deg * Mathf.Atan(frust.Fov.DownTan);
        }
        else
        {
            var frustOld = OVRPlugin.GetEyeFrustum((OVRPlugin.Eye)eye);
            eyeDescs[(int)eye].fullFov.LeftFov = Mathf.Rad2Deg * frustOld.fovX * 0.5f;
            eyeDescs[(int)eye].fullFov.RightFov = Mathf.Rad2Deg * frustOld.fovX * 0.5f;
            eyeDescs[(int)eye].fullFov.UpFov = Mathf.Rad2Deg * frustOld.fovY * 0.5f;
            eyeDescs[(int)eye].fullFov.DownFov = Mathf.Rad2Deg * frustOld.fovY * 0.5f;
        }
        // Symmetric Fov uses the maximum fov angle
        var maxFovX = Mathf.Max(eyeDescs[(int)eye].fullFov.LeftFov, eyeDescs[(int)eye].fullFov.RightFov);
        var maxFovY = Mathf.Max(eyeDescs[(int)eye].fullFov.UpFov, eyeDescs[(int)eye].fullFov.DownFov);
        eyeDescs[(int)eye].fov.x = maxFovX * 2.0f;
        eyeDescs[(int)eye].fov.y = maxFovY * 2.0f;
        if (!OVRPlugin.AsymmetricFovEnabled)
        {
            eyeDescs[(int)eye].fullFov.LeftFov = maxFovX;
            eyeDescs[(int)eye].fullFov.RightFov = maxFovX;
            eyeDescs[(int)eye].fullFov.UpFov = maxFovY;
            eyeDescs[(int)eye].fullFov.DownFov = maxFovY;
        }
    }
}
