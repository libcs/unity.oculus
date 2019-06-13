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

#if _WIN

public abstract class OVRCameraComposition : OVRComposition
{
    protected GameObject cameraFramePlaneObject;
    protected float cameraFramePlaneDistance;

    protected readonly bool hasCameraDeviceOpened = false;
    protected readonly bool useDynamicLighting = false;

    internal readonly OVRPlugin.CameraDevice cameraDevice = OVRPlugin.CameraDevice.WebCamera0;

    Mesh boundaryMesh = null;
    float boundaryMeshTopY = 0.0f;
    float boundaryMeshBottomY = 0.0f;
    OVRManager.VirtualGreenScreenType boundaryMeshType = OVRManager.VirtualGreenScreenType.Off;

    protected OVRCameraComposition(GameObject parentObject, Camera mainCamera, OVRManager.CameraDevice inCameraDevice, bool inUseDynamicLighting, OVRManager.DepthQuality depthQuality) : base(parentObject, mainCamera)
    {
        cameraDevice = OVRCompositionUtil.ConvertCameraDevice(inCameraDevice);
        Debug.Assert(!hasCameraDeviceOpened);
        Debug.Assert(!OVRPlugin.IsCameraDeviceAvailable(cameraDevice) || !OVRPlugin.HasCameraDeviceOpened(cameraDevice));
        hasCameraDeviceOpened = false;
        useDynamicLighting = inUseDynamicLighting;
        var cameraSupportsDepth = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
        if (useDynamicLighting && !cameraSupportsDepth)
            Debug.LogWarning("The camera device doesn't support depth. The result of dynamic lighting might not be correct");
        if (OVRPlugin.IsCameraDeviceAvailable(cameraDevice))
        {
            if (OVRPlugin.GetExternalCameraCount() > 0 && OVRPlugin.GetMixedRealityCameraInfo(0, out var extrinsics, out var intrinsics))
                OVRPlugin.SetCameraDevicePreferredColorFrameSize(cameraDevice, intrinsics.ImageSensorPixelResolution.w, intrinsics.ImageSensorPixelResolution.h);
            if (useDynamicLighting)
            {
                OVRPlugin.SetCameraDeviceDepthSensingMode(cameraDevice, OVRPlugin.CameraDeviceDepthSensingMode.Fill);
                var quality = OVRPlugin.CameraDeviceDepthQuality.Medium;
                if (depthQuality == OVRManager.DepthQuality.Low) quality = OVRPlugin.CameraDeviceDepthQuality.Low;
                else if (depthQuality == OVRManager.DepthQuality.Medium) quality = OVRPlugin.CameraDeviceDepthQuality.Medium;
                else if (depthQuality == OVRManager.DepthQuality.High) quality = OVRPlugin.CameraDeviceDepthQuality.High;
                else Debug.LogWarning("Unknown depth quality");
                OVRPlugin.SetCameraDevicePreferredDepthQuality(cameraDevice, quality);
            }
            OVRPlugin.OpenCameraDevice(cameraDevice);
            if (OVRPlugin.HasCameraDeviceOpened(cameraDevice))
                hasCameraDeviceOpened = true;
        }
    }

    public override void Cleanup()
    {
        OVRCompositionUtil.SafeDestroy(ref cameraFramePlaneObject);
        if (hasCameraDeviceOpened)
            OVRPlugin.CloseCameraDevice(cameraDevice);
    }

    public override void RecenterPose() => boundaryMesh = null;

    protected void CreateCameraFramePlaneObject(GameObject parentObject, Camera mixedRealityCamera, bool useDynamicLighting)
    {
        Debug.Assert(cameraFramePlaneObject == null);
        cameraFramePlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cameraFramePlaneObject.name = "MRCameraFrame";
        cameraFramePlaneObject.transform.parent = cameraInTrackingSpace ? cameraRig.trackingSpace : parentObject.transform;
        cameraFramePlaneObject.GetComponent<Collider>().enabled = false;
        cameraFramePlaneObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var cameraFrameMaterial = new Material(Shader.Find(useDynamicLighting ? "Oculus/OVRMRCameraFrameLit" : "Oculus/OVRMRCameraFrame"));
        cameraFramePlaneObject.GetComponent<MeshRenderer>().material = cameraFrameMaterial;
        cameraFrameMaterial.SetColor("_Color", Color.white);
        cameraFrameMaterial.SetFloat("_Visible", 0.0f);
        cameraFramePlaneObject.transform.localScale = new Vector3(4, 4, 4);
        cameraFramePlaneObject.SetActive(true);
        var cameraFrameCompositionManager = mixedRealityCamera.gameObject.AddComponent<OVRCameraFrameCompositionManager>();
        cameraFrameCompositionManager.cameraFrameGameObj = cameraFramePlaneObject;
        cameraFrameCompositionManager.composition = this;
    }

    bool nullcameraRigWarningDisplayed = false;
    protected void UpdateCameraFramePlaneObject(Camera mainCamera, Camera mixedRealityCamera, RenderTexture boundaryMeshMaskTexture)
    {
        var hasError = false;
        var cameraFrameMaterial = cameraFramePlaneObject.GetComponent<MeshRenderer>().material;
        var colorTexture = Texture2D.blackTexture;
        var depthTexture = Texture2D.whiteTexture;
        if (OVRPlugin.IsCameraDeviceColorFrameAvailable(cameraDevice))
            colorTexture = OVRPlugin.GetCameraDeviceColorFrameTexture(cameraDevice);
        else
        {
            Debug.LogWarning("Camera: color frame not ready");
            hasError = true;
        }
        var cameraSupportsDepth = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
        if (useDynamicLighting && cameraSupportsDepth)
        {
            if (OVRPlugin.IsCameraDeviceDepthFrameAvailable(cameraDevice))
                depthTexture = OVRPlugin.GetCameraDeviceDepthFrameTexture(cameraDevice);
            else
            {
                Debug.LogWarning("Camera: depth frame not ready");
                hasError = true;
            }
        }
        if (!hasError)
        {
            var offset = mainCamera.transform.position - mixedRealityCamera.transform.position;
            var distance = Vector3.Dot(mixedRealityCamera.transform.forward, offset);
            cameraFramePlaneDistance = distance;
            cameraFramePlaneObject.transform.position = mixedRealityCamera.transform.position + mixedRealityCamera.transform.forward * distance;
            cameraFramePlaneObject.transform.rotation = mixedRealityCamera.transform.rotation;
            var tanFov = Mathf.Tan(mixedRealityCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            cameraFramePlaneObject.transform.localScale = new Vector3(distance * mixedRealityCamera.aspect * tanFov * 2.0f, distance * tanFov * 2.0f, 1.0f);
            var worldHeight = distance * tanFov * 2.0f;
            var worldWidth = worldHeight * mixedRealityCamera.aspect;
            var cullingDistance = float.MaxValue;
            if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off)
                RefreshBoundaryMesh(mixedRealityCamera, out cullingDistance);
            cameraFrameMaterial.mainTexture = colorTexture;
            cameraFrameMaterial.SetTexture("_DepthTex", depthTexture);
            cameraFrameMaterial.SetVector("_FlipParams", new Vector4((OVRManager.instance.flipCameraFrameHorizontally ? 1.0f : 0.0f), (OVRManager.instance.flipCameraFrameVertically ? 1.0f : 0.0f), 0.0f, 0.0f));
            cameraFrameMaterial.SetColor("_ChromaKeyColor", OVRManager.instance.chromaKeyColor);
            cameraFrameMaterial.SetFloat("_ChromaKeySimilarity", OVRManager.instance.chromaKeySimilarity);
            cameraFrameMaterial.SetFloat("_ChromaKeySmoothRange", OVRManager.instance.chromaKeySmoothRange);
            cameraFrameMaterial.SetFloat("_ChromaKeySpillRange", OVRManager.instance.chromaKeySpillRange);
            cameraFrameMaterial.SetVector("_TextureDimension", new Vector4(colorTexture.width, colorTexture.height, 1.0f / colorTexture.width, 1.0f / colorTexture.height));
            cameraFrameMaterial.SetVector("_TextureWorldSize", new Vector4(worldWidth, worldHeight, 0, 0));
            cameraFrameMaterial.SetFloat("_SmoothFactor", OVRManager.instance.dynamicLightingSmoothFactor);
            cameraFrameMaterial.SetFloat("_DepthVariationClamp", OVRManager.instance.dynamicLightingDepthVariationClampingValue);
            cameraFrameMaterial.SetFloat("_CullingDistance", cullingDistance);
            if (OVRManager.instance.virtualGreenScreenType == OVRManager.VirtualGreenScreenType.Off || boundaryMesh == null || boundaryMeshMaskTexture == null)
                cameraFrameMaterial.SetTexture("_MaskTex", Texture2D.whiteTexture);
            else
            {
                if (cameraRig == null)
                {
                    if (!nullcameraRigWarningDisplayed)
                    {
                        Debug.LogWarning("Could not find the OVRCameraRig/CenterEyeAnchor object. Please check if the OVRCameraRig has been setup properly. The virtual green screen has been temporarily disabled");
                        nullcameraRigWarningDisplayed = true;
                    }
                    cameraFrameMaterial.SetTexture("_MaskTex", Texture2D.whiteTexture);
                }
                else
                {
                    if (nullcameraRigWarningDisplayed)
                    {
                        Debug.Log("OVRCameraRig/CenterEyeAnchor object found. Virtual green screen is activated");
                        nullcameraRigWarningDisplayed = false;
                    }
                    cameraFrameMaterial.SetTexture("_MaskTex", boundaryMeshMaskTexture);
                }
            }
        }
    }

    protected void RefreshBoundaryMesh(Camera camera, out float cullingDistance)
    {
        var depthTolerance = OVRManager.instance.virtualGreenScreenApplyDepthCulling ? OVRManager.instance.virtualGreenScreenDepthTolerance : float.PositiveInfinity;
        cullingDistance = OVRCompositionUtil.GetMaximumBoundaryDistance(camera, OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType)) + depthTolerance;
        if (boundaryMesh == null || boundaryMeshType != OVRManager.instance.virtualGreenScreenType || boundaryMeshTopY != OVRManager.instance.virtualGreenScreenTopY || boundaryMeshBottomY != OVRManager.instance.virtualGreenScreenBottomY)
        {
            boundaryMeshTopY = OVRManager.instance.virtualGreenScreenTopY;
            boundaryMeshBottomY = OVRManager.instance.virtualGreenScreenBottomY;
            boundaryMesh = OVRCompositionUtil.BuildBoundaryMesh(OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType), boundaryMeshTopY, boundaryMeshBottomY);
            boundaryMeshType = OVRManager.instance.virtualGreenScreenType;
            // Creating GameObject for testing purpose only
            //GameObject boundaryMeshObject = new GameObject("BoundaryMeshObject");
            //boundaryMeshObject.AddComponent<MeshFilter>().mesh = boundaryMesh;
            //boundaryMeshObject.AddComponent<MeshRenderer>();
        }
    }

    public class OVRCameraFrameCompositionManager : MonoBehaviour
    {
        public GameObject cameraFrameGameObj;
        public OVRCameraComposition composition;
        public RenderTexture boundaryMeshMaskTexture;
        Material cameraFrameMaterial;
        Material whiteMaterial;

        void Start()
        {
            var shader = Shader.Find("Oculus/Unlit");
            if (!shader)
            {
                Debug.LogError("Oculus/Unlit shader does not exist");
                return;
            }
            whiteMaterial = new Material(shader) { color = Color.white };
        }

        void OnPreRender()
        {
            if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off && boundaryMeshMaskTexture != null && composition.boundaryMesh != null)
            {
                var oldRT = RenderTexture.active;
                RenderTexture.active = boundaryMeshMaskTexture;
                // The camera matrices haven't been setup when OnPreRender() is executed. Load the projection manually
                GL.PushMatrix();
                GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);
                GL.Clear(false, true, Color.black);
                for (var i = 0; i < whiteMaterial.passCount; ++i)
                    if (whiteMaterial.SetPass(i))
                        Graphics.DrawMeshNow(composition.boundaryMesh, composition.cameraRig.ComputeTrackReferenceMatrix());
                GL.PopMatrix();
                RenderTexture.active = oldRT;
            }
            if (cameraFrameGameObj)
            {
                if (cameraFrameMaterial == null)
                    cameraFrameMaterial = cameraFrameGameObj.GetComponent<MeshRenderer>().material;
                cameraFrameMaterial.SetFloat("_Visible", 1.0f);
            }
        }

        void OnPostRender()
        {
            if (cameraFrameGameObj)
            {
                Debug.Assert(cameraFrameMaterial);
                cameraFrameMaterial.SetFloat("_Visible", 0.0f);
            }
        }
    }
}

#endif
