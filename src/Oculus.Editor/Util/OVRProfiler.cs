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

using Assets.OVR.Scripts;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OVRProfiler : EditorWindow
{
    enum TargetPlatform
    {
        OculusGo,
        GearVR,
        Quest,
        OculusRift
    };

    private static List<RangedRecord> mRecords = new List<RangedRecord>();
    private Vector2 mScrollPosition;
    static private TargetPlatform mTargetPlatform;

    [MenuItem("Oculus/Tools/OVR Profiler")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GetWindow(typeof(OVRProfiler));
        mTargetPlatform = Application.platform == RuntimePlatform.Android ? TargetPlatform.OculusGo : TargetPlatform.OculusRift;
    }

    void OnGUI()
    {
        GUILayout.Label("OVR Profiler", EditorStyles.boldLabel);
        var options = new string[]
        {
            "Oculus Go", "Gear VR", "Oculus Quest", "Oculus Rift",
        };
        mTargetPlatform = (TargetPlatform)EditorGUILayout.Popup("Target Oculus Platform", (int)mTargetPlatform, options);

        if (EditorApplication.isPlaying)
        {
            UpdateRecords();
            DrawResults();
        }
        else ShowCenterAlignedMessageLabel("Click Run in Unity to view stats.");
    }

    void OnInspectorUpdate() => Repaint();

    void DrawResults()
    {
        var lastCategory = string.Empty;
        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
        foreach (var record in mRecords)
        {
            // Add separator and label for new category
            if (!record.category.Equals(lastCategory))
            {
                lastCategory = record.category;
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(lastCategory, EditorStyles.label, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }
            // Draw records
            EditorGUILayout.BeginHorizontal();
            var r = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(r, record.value / (record.max * 2), record.category + " " + record.value.ToString());
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(record.message);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        EditorGUILayout.EndScrollView();
    }

    void UpdateRecords()
    {
        mRecords.Clear();
        if (mTargetPlatform == TargetPlatform.OculusRift)
        {
            AddRecord("Client Frame CPU Time (ms)", "", UnityStats.frameTime * 1000, 0, 11);
            AddRecord("Render Frame CPU Time (ms)", "", UnityStats.renderTime * 1000, 0, 11);
        }
        else
        {
            // Graphics memory
            var memSizeByte = UnityStats.usedTextureMemorySize + UnityStats.vboTotalBytes;
            AddRecord("Graphics Memory (MB)", "Please use less than 1024 MB of vertex and texture memory.", ConvertBytes(memSizeByte, "MB"), 0, 1024);
        }
        var triVertRec = mTargetPlatform == TargetPlatform.OculusRift ? 1000000 : 100000;
        // Triangle count
        AddRecord("Triangles", "Please use less than 100000 triangles.", UnityStats.triangles, 0, triVertRec);
        // Vertices count
        AddRecord("Vertices", "Please use less than 100000 vertices.", UnityStats.vertices, 0, triVertRec);
        var dcRec = mTargetPlatform == TargetPlatform.OculusRift ? 1000 : 100;
        // Draw call count
        AddRecord("Draw Call", "Please use less than 100 draw calls.", UnityStats.drawCalls, 0, dcRec);
    }

    string FormatBytes(long bytes, string target) => string.Format("{0:0.##} {1}", ConvertBytes(bytes, target), target);

    float ConvertBytes(long bytes, string target)
    {
        string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
        double dblSByte = bytes;
        for (var i = 0; i < Suffix.Length; i++, bytes /= 1024)
        {
            if (Suffix[i] == target)
                return (float)dblSByte;
            dblSByte = bytes / 1024.0;
        }
        return 0;
    }

    void ShowCenterAlignedMessageLabel(string message)
    {
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(message, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    void AddRecord(string category, string message, float value, float min, float max) => mRecords.Add(new RangedRecord(category, message, value, min, max));
}
