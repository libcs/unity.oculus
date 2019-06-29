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

using UnityEngine;
using System.Collections.Generic;

#if _WIN

internal class OVRCompositionUtil
{

    public static void SafeDestroy(GameObject obj)
    {
        if (Application.isPlaying) Object.Destroy(obj);
        else Object.DestroyImmediate(obj);
    }

    public static void SafeDestroy(ref GameObject obj)
    {
        SafeDestroy(obj);
        obj = null;
    }

    public static OVRPlugin.CameraDevice ConvertCameraDevice(OVRManager.CameraDevice cameraDevice)
    {
        if (cameraDevice == OVRManager.CameraDevice.WebCamera0) return OVRPlugin.CameraDevice.WebCamera0;
        else if (cameraDevice == OVRManager.CameraDevice.WebCamera1) return OVRPlugin.CameraDevice.WebCamera1;
        else if (cameraDevice == OVRManager.CameraDevice.ZEDCamera) return OVRPlugin.CameraDevice.ZEDCamera;
        else return OVRPlugin.CameraDevice.None;
    }

    public static OVRBoundary.BoundaryType ToBoundaryType(OVRManager.VirtualGreenScreenType type)
    {
        if (type == OVRManager.VirtualGreenScreenType.OuterBoundary) return OVRBoundary.BoundaryType.OuterBoundary;
        else if (type == OVRManager.VirtualGreenScreenType.PlayArea) return OVRBoundary.BoundaryType.PlayArea;
        else
        {
            Debug.LogWarning("Unmatched VirtualGreenScreenType");
            return OVRBoundary.BoundaryType.OuterBoundary;
        }
    }

    public static Vector3 GetWorldPosition(Vector3 trackingSpacePosition)
    {
        OVRPose tsPose;
        tsPose.position = trackingSpacePosition;
        tsPose.orientation = Quaternion.identity;
        var wsPose = OVRExtensions.ToWorldSpacePose(tsPose);
        return wsPose.position;
    }

    public static float GetMaximumBoundaryDistance(Camera camera, OVRBoundary.BoundaryType boundaryType)
    {
        if (!OVRManager.boundary.GetConfigured())
            return float.MaxValue;
        var geometry = OVRManager.boundary.GetGeometry(boundaryType);
        if (geometry.Length == 0)
            return float.MaxValue;
        var maxDistance = -float.MaxValue;
        foreach (var v in geometry)
        {
            var pos = GetWorldPosition(v);
            var distance = Vector3.Dot(camera.transform.forward, pos);
            if (maxDistance < distance)
                maxDistance = distance;
        }
        return maxDistance;
    }

    public static Mesh BuildBoundaryMesh(OVRBoundary.BoundaryType boundaryType, float topY, float bottomY)
    {
        if (!OVRManager.boundary.GetConfigured())
            return null;
        var geometry = new List<Vector3>(OVRManager.boundary.GetGeometry(boundaryType));
        if (geometry.Count == 0)
            return null;
        geometry.Add(geometry[0]);
        var numPoints = geometry.Count;
        var vertices = new Vector3[numPoints * 2];
        var uvs = new Vector2[numPoints * 2];
        for (var i = 0; i < numPoints; ++i)
        {
            var v = geometry[i];
            vertices[i] = new Vector3(v.x, bottomY, v.z);
            vertices[i + numPoints] = new Vector3(v.x, topY, v.z);
            uvs[i] = new Vector2((float)i / (numPoints - 1), 0.0f);
            uvs[i + numPoints] = new Vector2(uvs[i].x, 1.0f);
        }
        var triangles = new int[(numPoints - 1) * 2 * 3];
        for (var i = 0; i < numPoints - 1; ++i)
        {
            // the geometry is built clockwised. only the back faces should be rendered in the camera frame mask
            triangles[i * 6 + 0] = i;
            triangles[i * 6 + 1] = i + numPoints;
            triangles[i * 6 + 2] = i + 1 + numPoints;
            triangles[i * 6 + 3] = i;
            triangles[i * 6 + 4] = i + 1 + numPoints;
            triangles[i * 6 + 5] = i + 1;
        }
        return new Mesh
        {
            vertices = vertices,
            uv = uvs,
            triangles = triangles
        };
    }
}

#endif
