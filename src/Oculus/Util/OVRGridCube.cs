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

/// <summary>
/// Diagnostic display with a regular grid of cubes for visual testing of
/// tracking and distortion.
/// </summary>
public class OVRGridCube : MonoBehaviour
{
    /// <summary>
    /// The key that toggles the grid of cubes.
    /// </summary>
    public KeyCode GridKey = KeyCode.G;

    GameObject CubeGrid = null;

    bool CubeGridOn = false;
    bool CubeSwitchColorOld = false;
    bool CubeSwitchColor = false;

    int gridSizeX = 6;
    int gridSizeY = 4;
    int gridSizeZ = 6;
    float gridScale = 0.3f;
    float cubeScale = 0.03f;

    // Handle to OVRCameraRig
    OVRCameraRig CameraController = null;

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update() => UpdateCubeGrid();

    /// <summary>
    /// Sets the OVR camera controller.
    /// </summary>
    /// <param name="cameraController">Camera controller.</param>
    public void SetOVRCameraController(ref OVRCameraRig cameraController) => CameraController = cameraController;

    void UpdateCubeGrid()
    {
        // Toggle the grid cube display on 'G'
        if (Input.GetKeyDown(GridKey))
        {
            if (!CubeGridOn)
            {
                CubeGridOn = true;
                Debug.LogWarning("CubeGrid ON");
                if (CubeGrid != null) CubeGrid.SetActive(true);
                else CreateCubeGrid();
            }
            else
            {
                CubeGridOn = false;
                Debug.LogWarning("CubeGrid OFF");
                if (CubeGrid != null) CubeGrid.SetActive(false);
            }
        }
        if (CubeGrid != null)
        {
            // Set cube colors to let user know if camera is tracking
            CubeSwitchColor = !OVRManager.tracker.isPositionTracked;
            if (CubeSwitchColor != CubeSwitchColorOld)
                CubeGridSwitchColor(CubeSwitchColor);
            CubeSwitchColorOld = CubeSwitchColor;
        }
    }

    void CreateCubeGrid()
    {
        Debug.LogWarning("Create CubeGrid");
        // Create the visual cube grid
        CubeGrid = new GameObject("CubeGrid")
        {
            // Set a layer to target a specific camera
            layer = CameraController.gameObject.layer
        };
        for (var x = -gridSizeX; x <= gridSizeX; x++)
            for (var y = -gridSizeY; y <= gridSizeY; y++)
                for (var z = -gridSizeZ; z <= gridSizeZ; z++)
                {
                    // Set the cube type:
                    // 0 = non-axis cube
                    // 1 = axis cube
                    // 2 = center cube
                    var CubeType = 0;
                    if ((x == 0 && y == 0) || (x == 0 && z == 0) || (y == 0 && z == 0))
                    {
                        if (x == 0 && y == 0 && z == 0) CubeType = 2;
                        else CubeType = 1;
                    }
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var bc = cube.GetComponent<BoxCollider>();
                    bc.enabled = false;
                    cube.layer = CameraController.gameObject.layer;
                    // No shadows
                    var r = cube.GetComponent<Renderer>();
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = false;
                    // Cube line is white down the middle
                    if (CubeType == 0) r.material.color = Color.red;
                    else if (CubeType == 1) r.material.color = Color.white;
                    else r.material.color = Color.yellow;
                    cube.transform.position = new Vector3(x * gridScale, y * gridScale, z * gridScale);
                    var s = 0.7f;
                    // Axis cubes are bigger
                    if (CubeType == 1) s = 1.0f;
                    // Center cube is the largest
                    if (CubeType == 2) s = 2.0f;
                    cube.transform.localScale = new Vector3(cubeScale * s, cubeScale * s, cubeScale * s);
                    cube.transform.parent = CubeGrid.transform;
                }
    }

    /// <summary>
    /// Switch the Cube grid color.
    /// </summary>
    /// <param name="CubeSwitchColor">If set to <c>true</c> cube switch color.</param>
    void CubeGridSwitchColor(bool CubeSwitchColor)
    {
        var c = Color.red;
        if (CubeSwitchColor == true)
            c = Color.blue;
        foreach (Transform child in CubeGrid.transform)
        {
            var m = child.GetComponent<Renderer>().material;
            // Cube line is white down the middle
            if (m.color == Color.red || m.color == Color.blue)
                m.color = c;
        }
    }
}
