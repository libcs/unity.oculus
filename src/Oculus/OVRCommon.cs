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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InputTracking = UnityEngine.XR.InputTracking;
using Node = UnityEngine.XR.XRNode;
using NodeState = UnityEngine.XR.XRNodeState;
using Device = UnityEngine.XR.XRDevice;

/// <summary>
/// Miscellaneous extension methods that any script can use.
/// </summary>
public static class OVRExtensions
{
    /// <summary>
    /// Converts the given world-space transform to an OVRPose in tracking space.
    /// </summary>
    public static OVRPose ToTrackingSpacePose(this Transform transform, Camera camera)
    {
        OVRPose headPose;
        headPose.position = InputTracking.GetLocalPosition(Node.Head);
        headPose.orientation = InputTracking.GetLocalRotation(Node.Head);
        return headPose * transform.ToHeadSpacePose(camera);
    }

    /// <summary>
    /// Converts the given pose from tracking-space to world-space.
    /// </summary>
    public static OVRPose ToWorldSpacePose(OVRPose trackingSpacePose)
    {
        OVRPose headPose;
        headPose.position = InputTracking.GetLocalPosition(Node.Head);
        headPose.orientation = InputTracking.GetLocalRotation(Node.Head);
        // Transform from tracking-Space to head-Space
        var poseInHeadSpace = headPose.Inverse() * trackingSpacePose;
        // Transform from head space to world space
        return Camera.main.transform.ToOVRPose() * poseInHeadSpace;
    }

    /// <summary>
    /// Converts the given world-space transform to an OVRPose in head space.
    /// </summary>
    public static OVRPose ToHeadSpacePose(this Transform transform, Camera camera) => camera.transform.ToOVRPose().Inverse() * transform.ToOVRPose();

    public static OVRPose ToOVRPose(this Transform t, bool isLocal = false)
    {
        OVRPose pose;
        pose.orientation = (isLocal) ? t.localRotation : t.rotation;
        pose.position = (isLocal) ? t.localPosition : t.position;
        return pose;
    }

    public static void FromOVRPose(this Transform t, OVRPose pose, bool isLocal = false)
    {
        if (isLocal)
        {
            t.localRotation = pose.orientation;
            t.localPosition = pose.position;
        }
        else
        {
            t.rotation = pose.orientation;
            t.position = pose.position;
        }
    }

    public static OVRPose ToOVRPose(this OVRPlugin.Posef p) => new OVRPose
    {
        position = new Vector3(p.Position.x, p.Position.y, -p.Position.z),
        orientation = new Quaternion(-p.Orientation.x, -p.Orientation.y, p.Orientation.z, p.Orientation.w)
    };

    public static OVRTracker.Frustum ToFrustum(this OVRPlugin.Frustumf f) => new OVRTracker.Frustum
    {
        nearZ = f.zNear,
        farZ = f.zFar,
        fov = new Vector2
        {
            x = Mathf.Rad2Deg * f.fovX,
            y = Mathf.Rad2Deg * f.fovY
        }
    };

    public static Color FromColorf(this OVRPlugin.Colorf c) => new Color { r = c.r, g = c.g, b = c.b, a = c.a };
    public static OVRPlugin.Colorf ToColorf(this Color c) => new OVRPlugin.Colorf { r = c.r, g = c.g, b = c.b, a = c.a };
    public static Vector3 FromVector3f(this OVRPlugin.Vector3f v) => new Vector3 { x = v.x, y = v.y, z = v.z };
    public static Vector3 FromFlippedZVector3f(this OVRPlugin.Vector3f v) => new Vector3 { x = v.x, y = v.y, z = -v.z };
    public static OVRPlugin.Vector3f ToVector3f(this Vector3 v) => new OVRPlugin.Vector3f { x = v.x, y = v.y, z = v.z };
    public static OVRPlugin.Vector3f ToFlippedZVector3f(this Vector3 v) => new OVRPlugin.Vector3f { x = v.x, y = v.y, z = -v.z };
    public static Quaternion FromQuatf(this OVRPlugin.Quatf q) => new Quaternion { x = q.x, y = q.y, z = q.z, w = q.w };
    public static Quaternion FromFlippedZQuatf(this OVRPlugin.Quatf q) => new Quaternion { x = -q.x, y = -q.y, z = q.z, w = q.w };
    public static OVRPlugin.Quatf ToQuatf(this Quaternion q) => new OVRPlugin.Quatf { x = q.x, y = q.y, z = q.z, w = q.w };
    public static OVRPlugin.Quatf ToFlippedZQuatf(this Quaternion q) => new OVRPlugin.Quatf { x = -q.x, y = -q.y, z = q.z, w = q.w };

    public static OVR.OpenVR.HmdMatrix34_t ConvertToHMDMatrix34(this Matrix4x4 m) => new OVR.OpenVR.HmdMatrix34_t
    {
        m0 = m[0, 0],
        m1 = m[0, 1],
        m2 = -m[0, 2],
        m3 = m[0, 3],
        m4 = m[1, 0],
        m5 = m[1, 1],
        m6 = -m[1, 2],
        m7 = m[1, 3],
        m8 = -m[2, 0],
        m9 = -m[2, 1],
        m10 = m[2, 2],
        m11 = -m[2, 3]
    };
}

// 4 types of node state properties that can be queried with UnityEngine.XR
public enum NodeStatePropertyType
{
    Acceleration,
    AngularAcceleration,
    Velocity,
    AngularVelocity,
    Position,
    Orientation
}

public static class OVRNodeStateProperties
{
    static List<NodeState> nodeStateList = new List<NodeState>();

    public static bool IsHmdPresent() => Device.isPresent;

    public static bool GetNodeStatePropertyVector3(Node nodeType, NodeStatePropertyType propertyType, OVRPlugin.Node ovrpNodeType, OVRPlugin.Step stepType, out Vector3 retVec)
    {
        retVec = Vector3.zero;
        switch (propertyType)
        {
            case NodeStatePropertyType.Acceleration:
                if (GetUnityXRNodeStateVector3(nodeType, NodeStatePropertyType.Acceleration, out retVec))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retVec = OVRPlugin.GetNodeAcceleration(ovrpNodeType, stepType).FromFlippedZVector3f();
                    return true;
                }
                break;
            case NodeStatePropertyType.AngularAcceleration:
                if (GetUnityXRNodeStateVector3(nodeType, NodeStatePropertyType.AngularAcceleration, out retVec))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retVec = OVRPlugin.GetNodeAngularAcceleration(ovrpNodeType, stepType).FromFlippedZVector3f();
                    return true;
                }
                break;
            case NodeStatePropertyType.Velocity:
                if (GetUnityXRNodeStateVector3(nodeType, NodeStatePropertyType.Velocity, out retVec))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retVec = OVRPlugin.GetNodeVelocity(ovrpNodeType, stepType).FromFlippedZVector3f();
                    return true;
                }
                break;
            case NodeStatePropertyType.AngularVelocity:
                if (GetUnityXRNodeStateVector3(nodeType, NodeStatePropertyType.AngularVelocity, out retVec))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retVec = OVRPlugin.GetNodeAngularVelocity(ovrpNodeType, stepType).FromFlippedZVector3f();
                    return true;
                }
                break;
            case NodeStatePropertyType.Position:
                if (GetUnityXRNodeStateVector3(nodeType, NodeStatePropertyType.Position, out retVec))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retVec = OVRPlugin.GetNodePose(ovrpNodeType, stepType).ToOVRPose().position;
                    return true;
                }
                break;
        }
        return false;
    }

    public static bool GetNodeStatePropertyQuaternion(Node nodeType, NodeStatePropertyType propertyType, OVRPlugin.Node ovrpNodeType, OVRPlugin.Step stepType, out Quaternion retQuat)
    {
        retQuat = Quaternion.identity;
        switch (propertyType)
        {
            case NodeStatePropertyType.Orientation:
                if (GetUnityXRNodeStateQuaternion(nodeType, NodeStatePropertyType.Orientation, out retQuat))
                    return true;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
                {
                    retQuat = OVRPlugin.GetNodePose(ovrpNodeType, stepType).ToOVRPose().orientation;
                    return true;
                }
                break;
        }
        return false;
    }

    private static bool ValidateProperty(Node nodeType, ref NodeState requestedNodeState)
    {
        InputTracking.GetNodeStates(nodeStateList);
        if (nodeStateList.Count == 0)
            return false;
        var nodeStateFound = false;
        requestedNodeState = nodeStateList[0];
        for (var i = 0; i < nodeStateList.Count; i++)
            if (nodeStateList[i].nodeType == nodeType)
            {
                requestedNodeState = nodeStateList[i];
                nodeStateFound = true;
                break;
            }
        return nodeStateFound;
    }

    static bool GetUnityXRNodeStateVector3(Node nodeType, NodeStatePropertyType propertyType, out Vector3 retVec)
    {
        retVec = Vector3.zero;
        var requestedNodeState = default(NodeState);
        if (!ValidateProperty(nodeType, ref requestedNodeState))
            return false;
        if (propertyType == NodeStatePropertyType.Acceleration) { if (requestedNodeState.TryGetAcceleration(out retVec)) return true; }
        else if (propertyType == NodeStatePropertyType.AngularAcceleration) { if (requestedNodeState.TryGetAngularAcceleration(out retVec)) return true; }
        else if (propertyType == NodeStatePropertyType.Velocity) { if (requestedNodeState.TryGetVelocity(out retVec)) return true; }
        else if (propertyType == NodeStatePropertyType.AngularVelocity) { if (requestedNodeState.TryGetAngularVelocity(out retVec)) return true; }
        else if (propertyType == NodeStatePropertyType.Position) { if (requestedNodeState.TryGetPosition(out retVec)) return true; }
        return false;
    }

    static bool GetUnityXRNodeStateQuaternion(Node nodeType, NodeStatePropertyType propertyType, out Quaternion retQuat)
    {
        retQuat = Quaternion.identity;
        var requestedNodeState = default(NodeState);
        if (!ValidateProperty(nodeType, ref requestedNodeState))
            return false;
        if (propertyType == NodeStatePropertyType.Orientation)
            if (requestedNodeState.TryGetRotation(out retQuat))
                return true;
        return false;
    }
}

/// <summary>
/// An affine transformation built from a Unity position and orientation.
/// </summary>
[System.Serializable]
public struct OVRPose
{
    /// <summary>
    /// A pose with no translation or rotation.
    /// </summary>
    public static OVRPose identity => new OVRPose()
    {
        position = Vector3.zero,
        orientation = Quaternion.identity
    };

    public override bool Equals(object obj) => obj is OVRPose && this == (OVRPose)obj;

    public override int GetHashCode() => position.GetHashCode() ^ orientation.GetHashCode();

    public static bool operator ==(OVRPose x, OVRPose y) => x.position == y.position && x.orientation == y.orientation;

    public static bool operator !=(OVRPose x, OVRPose y) => !(x == y);

    /// <summary>
    /// The position.
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// The orientation.
    /// </summary>
    public Quaternion orientation;

    /// <summary>
    /// Multiplies two poses.
    /// </summary>
    public static OVRPose operator *(OVRPose lhs, OVRPose rhs) => new OVRPose
    {
        position = lhs.position + lhs.orientation * rhs.position,
        orientation = lhs.orientation * rhs.orientation
    };

    /// <summary>
    /// Computes the inverse of the given pose.
    /// </summary>
    public OVRPose Inverse()
    {
        OVRPose ret;
        ret.orientation = Quaternion.Inverse(orientation);
        ret.position = ret.orientation * -position;
        return ret;
    }

    /// <summary>
    /// Converts the pose from left- to right-handed or vice-versa.
    /// </summary>
    public OVRPose flipZ()
    {
        var ret = this;
        ret.position.z = -ret.position.z;
        ret.orientation.z = -ret.orientation.z;
        ret.orientation.w = -ret.orientation.w;
        return ret;
    }

    public OVRPlugin.Posef ToPosef() => new OVRPlugin.Posef()
    {
        Position = position.ToVector3f(),
        Orientation = orientation.ToQuatf()
    };
}

/// <summary>
/// Encapsulates an 8-byte-aligned of unmanaged memory.
/// </summary>
public class OVRNativeBuffer : IDisposable
{
    bool disposed = false;
    int m_numBytes = 0;
    IntPtr m_ptr = IntPtr.Zero;

    /// <summary>
    /// Creates a buffer of the specified size.
    /// </summary>
    public OVRNativeBuffer(int numBytes) => Reallocate(numBytes);

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the <see cref="OVRNativeBuffer"/> is
    /// reclaimed by garbage collection.
    /// </summary>
    ~OVRNativeBuffer() { Dispose(false); }

    /// <summary>
    /// Reallocates the buffer with the specified new size.
    /// </summary>
    public void Reset(int numBytes) => Reallocate(numBytes);

    /// <summary>
    /// The current number of bytes in the buffer.
    /// </summary>
    public int GetCapacity() => m_numBytes;

    /// <summary>
    /// A pointer to the unmanaged memory in the buffer, starting at the given offset in bytes.
    /// </summary>
    public IntPtr GetPointer(int byteOffset = 0) => byteOffset < 0 || byteOffset >= m_numBytes
            ? IntPtr.Zero
            : (byteOffset == 0) ? m_ptr : new IntPtr(m_ptr.ToInt64() + byteOffset);

    /// <summary>
    /// Releases all resource used by the <see cref="OVRNativeBuffer"/> object.
    /// </summary>
    /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="OVRNativeBuffer"/>. The <see cref="Dispose"/>
    /// method leaves the <see cref="OVRNativeBuffer"/> in an unusable state. After calling <see cref="Dispose"/>, you must
    /// release all references to the <see cref="OVRNativeBuffer"/> so the garbage collector can reclaim the memory that
    /// the <see cref="OVRNativeBuffer"/> was occupying.</remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing)
    {
        if (disposed)
            return;
        Release();
        disposed = true;
    }

    void Reallocate(int numBytes)
    {
        Release();
        if (numBytes > 0)
        {
            m_ptr = Marshal.AllocHGlobal(numBytes);
            m_numBytes = numBytes;
        }
        else
        {
            m_ptr = IntPtr.Zero;
            m_numBytes = 0;
        }
    }

    void Release()
    {
        if (m_ptr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(m_ptr);
            m_ptr = IntPtr.Zero;
            m_numBytes = 0;
        }
    }
}
