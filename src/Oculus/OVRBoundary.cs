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
using System.Runtime.InteropServices;
using UnityEngine;
using Boundary = UnityEngine.Experimental.XR.Boundary;

/// <summary>
/// Provides access to the Oculus boundary system.
/// </summary>
public class OVRBoundary
{
    /// <summary>
    /// Specifies a tracked node that can be queried through the boundary system.
    /// </summary>
    public enum Node
    {
        HandLeft = OVRPlugin.Node.HandLeft,  ///< Tracks the left hand node.
		HandRight = OVRPlugin.Node.HandRight, ///< Tracks the right hand node.
		Head = OVRPlugin.Node.Head,      ///< Tracks the head node.
	}

    /// <summary>
    /// Specifies a boundary type surface.
    /// </summary>
    public enum BoundaryType
    {
        OuterBoundary = OVRPlugin.BoundaryType.OuterBoundary, ///< Outer boundary that closely matches the user's configured walls.
		PlayArea = OVRPlugin.BoundaryType.PlayArea,      ///< Smaller convex area inset within the outer boundary.
	}

    /// <summary>
    /// Provides test results of boundary system queries.
    /// </summary>
    public struct BoundaryTestResult
    {
        public bool IsTriggering;                              ///< Returns true if the queried test would violate and/or trigger the tested boundary types.
		public float ClosestDistance;                          ///< Returns the distance between the queried test object and the closest tested boundary type.
		public Vector3 ClosestPoint;                           ///< Returns the closest point to the queried test object.
		public Vector3 ClosestPointNormal;                     ///< Returns the normal of the closest point to the queried test object.
	}

    /// <summary>
    /// Returns true if the boundary system is currently configured with valid boundary data.
    /// </summary>
    public bool GetConfigured() => OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus ? OVRPlugin.GetBoundaryConfigured() : Boundary.configured;

    /// <summary>
    /// Returns the results of testing a tracked node against the specified boundary type.
    /// All points are returned in local tracking space shared by tracked nodes and accessible through OVRCameraRig's trackingSpace anchor.
    /// </summary>
    public BoundaryTestResult TestNode(Node node, BoundaryType boundaryType)
    {
        var ovrpRes = OVRPlugin.TestBoundaryNode((OVRPlugin.Node)node, (OVRPlugin.BoundaryType)boundaryType);
        return new BoundaryTestResult
        {
            IsTriggering = ovrpRes.IsTriggering == OVRPlugin.Bool.True,
            ClosestDistance = ovrpRes.ClosestDistance,
            ClosestPoint = ovrpRes.ClosestPoint.FromFlippedZVector3f(),
            ClosestPointNormal = ovrpRes.ClosestPointNormal.FromFlippedZVector3f(),
        };
    }

    /// <summary>
    /// Returns the results of testing a 3d point against the specified boundary type.
    /// The test point is expected in local tracking space.
    /// All points are returned in local tracking space shared by tracked nodes and accessible through OVRCameraRig's trackingSpace anchor.
    /// </summary>
    public BoundaryTestResult TestPoint(Vector3 point, BoundaryType boundaryType)
    {
        var ovrpRes = OVRPlugin.TestBoundaryPoint(point.ToFlippedZVector3f(), (OVRPlugin.BoundaryType)boundaryType);
        return new BoundaryTestResult
        {
            IsTriggering = ovrpRes.IsTriggering == OVRPlugin.Bool.True,
            ClosestDistance = ovrpRes.ClosestDistance,
            ClosestPoint = ovrpRes.ClosestPoint.FromFlippedZVector3f(),
            ClosestPointNormal = ovrpRes.ClosestPointNormal.FromFlippedZVector3f(),
        };
    }

    static int cachedVector3fSize = Marshal.SizeOf(typeof(OVRPlugin.Vector3f));
    static OVRNativeBuffer cachedGeometryNativeBuffer = new OVRNativeBuffer(0);
    static float[] cachedGeometryManagedBuffer = new float[0];
    List<Vector3> cachedGeometryList = new List<Vector3>();
    /// <summary>
    /// Returns an array of 3d points (in clockwise order) that define the specified boundary type.
    /// All points are returned in local tracking space shared by tracked nodes and accessible through OVRCameraRig's trackingSpace anchor.
    /// </summary>
    public Vector3[] GetGeometry(BoundaryType boundaryType)
    {
        if (OVRManager.loadedXRDevice != OVRManager.XRDevice.Oculus)
        {
            if (Boundary.TryGetGeometry(cachedGeometryList, (boundaryType == BoundaryType.PlayArea) ? Boundary.Type.PlayArea : Boundary.Type.TrackedArea))
                return cachedGeometryList.ToArray();
            Debug.LogError("This functionality is not supported in your current version of Unity.");
            return null;
        }
        var pointsCount = 0;
        if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, IntPtr.Zero, ref pointsCount) && pointsCount > 0)
        {
            var requiredNativeBufferCapacity = pointsCount * cachedVector3fSize;
            if (cachedGeometryNativeBuffer.GetCapacity() < requiredNativeBufferCapacity)
                cachedGeometryNativeBuffer.Reset(requiredNativeBufferCapacity);
            var requiredManagedBufferCapacity = pointsCount * 3;
            if (cachedGeometryManagedBuffer.Length < requiredManagedBufferCapacity)
                cachedGeometryManagedBuffer = new float[requiredManagedBufferCapacity];
            if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, cachedGeometryNativeBuffer.GetPointer(), ref pointsCount))
            {
                Marshal.Copy(cachedGeometryNativeBuffer.GetPointer(), cachedGeometryManagedBuffer, 0, requiredManagedBufferCapacity);
                var points = new Vector3[pointsCount];
                for (var i = 0; i < pointsCount; i++)
                    points[i] = new OVRPlugin.Vector3f()
                    {
                        x = cachedGeometryManagedBuffer[3 * i + 0],
                        y = cachedGeometryManagedBuffer[3 * i + 1],
                        z = cachedGeometryManagedBuffer[3 * i + 2],
                    }.FromFlippedZVector3f();
                return points;
            }
        }
        return new Vector3[0];
    }

    /// <summary>
    /// Returns a vector that indicates the spatial dimensions of the specified boundary type. (x = width, y = height, z = depth)
    /// </summary>
    public Vector3 GetDimensions(BoundaryType boundaryType) => OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus
        ? OVRPlugin.GetBoundaryDimensions((OVRPlugin.BoundaryType)boundaryType).FromVector3f()
        : Boundary.TryGetDimensions(out var dimensions, (boundaryType == BoundaryType.PlayArea) ? Boundary.Type.PlayArea : Boundary.Type.TrackedArea)
            ? dimensions
            : Vector3.zero;

    /// <summary>
    /// Returns true if the boundary system is currently visible.
    /// </summary>
    public bool GetVisible() => OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus ? OVRPlugin.GetBoundaryVisible() : Boundary.visible;

    /// <summary>
    /// Requests that the boundary system visibility be set to the specified value.
    /// The actual visibility can be overridden by the system (i.e., proximity trigger) or by the user (boundary system disabled)
    /// </summary>
    public void SetVisible(bool value)
    {
        if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus) OVRPlugin.SetBoundaryVisible(value);
        else Boundary.visible = value;
    }
}
