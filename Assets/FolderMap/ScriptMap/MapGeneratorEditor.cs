#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện mặc định của các biến (width, height, tile...)
        DrawDefaultInspector();

        MapGenerator mapGen = (MapGenerator)target;

        GUILayout.Space(15); // Tạo khoảng cách cho đẹp mắt

        // Tạo nút Tạo Bản Đồ
        if (GUILayout.Button("🎨 VẼ MAP LUÔN (GENERATE)", GUILayout.Height(40)))
        {
            mapGen.GenerateMapInEditor();
        }

        // Tạo nút Xóa Bản Đồ
        if (GUILayout.Button("❌ XÓA SẠCH MAP (CLEAR)", GUILayout.Height(30)))
        {
            mapGen.ClearMapInEditor();
        }
    }
}
#endif