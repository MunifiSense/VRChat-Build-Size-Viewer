/**
 * VRC Build Size Viewer
 * Created by MunifiSense
 * https://github.com/MunifiSense/VRChat-Build-Size-Viewer
 */

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildSizeViewer : EditorWindow {

    public class BuildObject {
        public string size;
        public string percent;
        public string path;
    }

    List<BuildObject> buildObjectList;
    List<string> uncompressedList;
    string buildLogPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/Unity/Editor/Editor.log";
    private char[] delimiterChars = { ' ', '\t' };
    float win;
    float w1;
    float w2;
    float w3;
    string totalSize;
    bool buildLogFound = false;
    Vector2 scrollPos;

    [MenuItem("Window/Muni/VRC Build Size Viewer")]

    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(BuildSizeViewer));
    }

    void OnGUI() {
        win = (float)(position.width * 0.6);
        float w1 = (float)(win * 0.15);
        float w2 = (float)(win * 0.15);
        float w3 = (float)(win * 0.35);
        EditorGUILayout.LabelField("VRC Build Size Viewer", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Create a build of your world/avatar and click the button!", EditorStyles.label);
        if (GUILayout.Button("Read Build Log")) {
            buildLogFound = false;
            buildLogFound = getBuildSize();
        }
        if (buildLogFound) {
            if (uncompressedList != null && uncompressedList.Count != 0) {
                EditorGUILayout.LabelField("Total Compressed Build Size: " + totalSize);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
                //EditorGUILayout.LabelField("Uncompressed Build Size by Category: ");
                foreach (string s in uncompressedList) {
                    EditorGUILayout.LabelField(s);
                }
            }
            if (buildObjectList != null && buildObjectList.Count != 0) {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Size%", GUILayout.Width(w1));
                EditorGUILayout.LabelField("Size", GUILayout.Width(w2));
                EditorGUILayout.LabelField("Path", GUILayout.Width(w3));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
                foreach (BuildObject buildObject in buildObjectList) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(buildObject.percent, GUILayout.Width(w1));
                    EditorGUILayout.LabelField(buildObject.size, GUILayout.Width(w2));
                    EditorGUILayout.LabelField(buildObject.path);
                    if(buildObject.path != "Resources/unity_builtin_extra") {
                        if (GUILayout.Button("Go", GUILayout.Width(w1))) {
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(buildObject.path, typeof(UnityEngine.Object));
                            Selection.activeObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }

    private bool getBuildSize() {
        //Read the text from log
        FileUtil.ReplaceFile(buildLogPath, buildLogPath + "copy");
        StreamReader reader = new StreamReader(buildLogPath + "copy");

        if(reader == null) {
            Debug.LogWarning("Could not read build file.");
            FileUtil.DeleteFileOrDirectory(buildLogPath + "copy");
            return false;
        }

        string line = reader.ReadLine();
        while(line != null) {
            if ((line.Contains("scene-") && line.Contains(".vrcw"))
                || (line.Contains("avtr") && line.Contains(".prefab.unity3d"))) {
                //Debug.Log("Build found!");
                buildObjectList = new List<BuildObject>();
                uncompressedList = new List<string>();
                line = reader.ReadLine();
                //Debug.Log(line);
                while (!line.Contains("Compressed Size"))
                {
                    line = reader.ReadLine();
                }
                totalSize = line.Split(':')[1];
                line = reader.ReadLine();
                while (line != "Used Assets and files from the Resources folder, sorted by uncompressed size:") {
                    uncompressedList.Add(line);
                    line = reader.ReadLine();
                }
                line = reader.ReadLine();
                while (line != "-------------------------------------------------------------------------------") {
                    string[] splitLine = line.Split(delimiterChars);
                    BuildObject temp = new BuildObject();
                    temp.size = splitLine[1]+splitLine[2];
                    temp.percent = splitLine[4];
                    temp.path = splitLine[5];
                    for (int i=6; i<splitLine.Length; i++) {
                        temp.path += (" " + splitLine[i]);
                    }
                    buildObjectList.Add(temp);
                    line = reader.ReadLine();
                }
            }
            line = reader.ReadLine();
        }
        FileUtil.DeleteFileOrDirectory(buildLogPath + "copy");
        reader.Close();
        return true;
    }
}
#endif
