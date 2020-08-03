using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BatchRenameEditor : EditorWindow
{
    private static string folder = string.Empty;
    private static string replaceName = string.Empty;
    private static string toName = string.Empty;

    [MenuItem("Tools/批量修改文件名")]
    static void Init()
    {
        GetWindow(typeof(BatchRenameEditor));
        ReadPath();
    }

    static private void SavePath()
    {
        EditorPrefs.SetString("folder", folder);
    }

    static private void ReadPath()
    {
        folder = EditorPrefs.GetString("folder");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("rename path : ", EditorStyles.boldLabel, GUILayout.Width(100));
        folder = GUILayout.TextField(folder, GUILayout.Width(300));
        if (GUILayout.Button("...", GUILayout.Width(40)))
        {
            SelectFileFolder();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("oldName : ", EditorStyles.boldLabel, GUILayout.Width(80));
        replaceName = GUILayout.TextField(replaceName, GUILayout.Width(100));
        GUILayout.Space(10);

        GUILayout.Label("replaceName : ", EditorStyles.boldLabel, GUILayout.Width(120));
        toName = GUILayout.TextField(toName, GUILayout.Width(100));
        GUILayout.Space(10);


        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("---------------------");
        if (GUILayout.Button("Rename", GUILayout.Width(100)))
        {
            Batch();
        }
        GUILayout.Label("---------------------");
        GUILayout.EndHorizontal();
    }

    public void SelectFileFolder()
    {
        var selProtoPath = EditorUtility.OpenFolderPanel("Select folder", "", "");

        folder = selProtoPath;
        SavePath();
    }

    static List<string> filePaths = new List<string>();

    static void RecursiveFiles(string path)
    {
        var files = Directory.GetFiles(path);
        var dirs = Directory.GetDirectories(path);

        filePaths.AddRange(files);
        
        foreach (var item in dirs)
        {
            RecursiveFiles(item);
        }
    }

    public static void Batch()
    {
        if (!Directory.Exists(folder))
        {
            Debug.Log("目录不存在: " + folder);
            return;
        }

        filePaths.Clear();
        RecursiveFiles(folder);
        int count = 0;

        foreach (var path in filePaths)
        {
            if (path.EndsWith(".meta"))
                continue;

            if(!path.Contains(replaceName))
                continue;

            string replace = path.Replace(replaceName, toName);
            Debug.Log(path + "=》" + replace);
            File.Move(path, replace);
            count++;
        }

        AssetDatabase.ImportAsset(folder);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("提示",string.Format("{0}个文件重命名成功！", count),"确定");
    }
}
