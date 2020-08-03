using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using ETModel;

public class MVCToolEditor : EditorWindow
{
    [MenuItem("Tools/MVC Tools #q")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MVCToolEditor));
    }

    public static string author = "passion";
    public static string newUIStr = "UIName";
    public static string newFileStr = "FileName";
    static bool isCrtl = true;
    static bool isModel = true;
    static bool isView = true;
    static UILayers_TYPE mLayerType;

    public void OnGUI()
    {
        GUILayout.Label("创建新的UI，并同时创建脚本文件夹和脚本");
        GUILayout.Space(5);
        GUILayout.Label("作者");
        author = GUILayout.TextArea(author, 100);

        GUILayout.Space(5);
        GUILayout.Label("UI名字  命名格式:UIXXX");
        newUIStr = GUILayout.TextArea(newUIStr, 100);
        GUILayout.Label("文件夹名名字");
        newFileStr = GUILayout.TextArea(newFileStr, 100);
        GUILayout.Space(10);
        isCrtl = GUILayout.Toggle(isCrtl, "IsController 是否需要创建Controller");
        GUILayout.Space(10);
        isModel = GUILayout.Toggle(isModel, "isModel 是否需要创建Moel");
        GUILayout.Space(10);
        isView = GUILayout.Toggle(isView, "isView 是否需要创建View");
        GUILayout.Space(5);
        GUILayout.Label("选择UILayers");
        mLayerType = (UILayers_TYPE)EditorGUILayout.EnumPopup(mLayerType);
        GUILayout.Space(15);

        if (GUILayout.Button("Creat UI Data"))
        {
            if (newUIStr == "UIName")
            {
                Debug.LogError("UIName is deafult");
                return;
            }
            if (newFileStr == "FileName")
            {
                Debug.LogError("FileName is deafult");
                return;
            }

            CreatUIScript();
            if (isModel)
            {
                CreatModellLua();
            }
            if (isView)
            {
                CreatViewlLua();
            }
            if (isCrtl)
            {
                CreatCrtlLua();
            }

            CreaWriteWindowsName();
            AssetDatabase.SaveAssets();

            if(EditorUtility.DisplayDialog("MVC Tool", newUIStr + "页面生成成功!!!", "确定"))
            {
                AssetDatabase.Refresh();
            }
        }
    }

    public enum UILayers_TYPE
    {
        SceneLayer,//用于场景UI
        BackgroudLayer,//背景UI
        NormalLayer,//普通一级、二级、三级UI
        InfoLayer,//信息UI
        TipLayer,//提示UI
        TopLayer,//顶层UI
    }
    #region 创建Crtl
    private static void CreatCrtlLua()
    {
        string luaData = "";
        luaData = "/**\r\n"
            + $"* added by {author}  @ {System.DateTime.Now} \r\n"
            + $"* {newUIStr}控制层\r\n"
            + "* \r\n"
            + "*/\r\n"
            + "using System;\r\n"
            + "using UnityEngine;\r\n"
            + "using UnityEngine.UI;\r\n"
            + "using ETModel;\r\n"
            + "\r\n"
            + "namespace ETHotfix\r\n"
            + "{\r\n"
            + "    \r\n"
            + $"    public class {newUIStr}Ctrl: UIBaseCtrl\r\n"
            + "{\r\n"
            + (isModel?$"        private new {newUIStr}Model model;\r\n":"")
            + (isModel?$"        public {newUIStr}Model Model {{ get {{ if (model == null) model = base.model as {newUIStr}Model; return model; }} }}\r\n":"")
            + "\r\n"
            + "        public override void Awake()\r\n"
            + "        {\r\n"
            + "\r\n"
            + "        }\r\n"
            + "    }\r\n"
            + "};\r\n";

        Debug.Log(luaData);
        string path = string.Format("{0}{1}{2}Component{2}{1}Ctrl.cs", DirectoryName, newFileStr, Path.DirectorySeparatorChar);

        CreatUIScript(path, luaData);
    }
    #endregion
    #region 创建Model
    private static void CreatModellLua()
    {
        string luaData = "";
        luaData = "/**\r\n"
            + $"* added by {author}  @ {System.DateTime.Now} \r\n"
            + $"* {newUIStr}模型层\r\n"
            + "* \r\n"
            + "*/\r\n"
            + "using System;\r\n"
            + "using ETModel;\r\n"
            + "using UnityEngine;\r\n"
            + "\r\n"
            + "namespace ETHotfix\r\n"
            + "{\r\n"
            + "    \r\n"
            + $"    public class {newUIStr}Model: UIBaseModel\r\n"
            + "	{\r\n"
            + "        public override void Enable(object[] args)\r\n"
            + "		{\r\n"
            + "		    base.Enable(args);\r\n"
            + "        }\r\n"
            + "	}\r\n"
            + "}\r\n";

        Debug.Log(luaData);
        string path = string.Format("{0}{1}{2}Component{2}{1}Model.cs", DirectoryName, newFileStr, Path.DirectorySeparatorChar);
        CreatUIScript(path, luaData);

    }
    #endregion
    #region 创建View
    private static void CreatViewlLua()
    {
        string luaData = "";
        luaData = "/**\r\n"
            + $"* added by {author}  @ {System.DateTime.Now} \r\n"
            + $"* {newUIStr}视图层\r\n"
            + "* \r\n"
            + "*/\r\n" 
            + "using System;\r\n"
            + "using ETModel;\r\n"
            + "using UnityEngine;\r\n"
            + "using UnityEngine.UI;\r\n"
            + "\r\n"
            + "namespace ETHotfix\r\n"
            + "{\r\n"
            + "    \r\n"
            + $"    public class {newUIStr}View: UIBaseView\r\n"
            + "	{\r\n"
            + (isCrtl?$"        private new {newUIStr}Ctrl ctrl;\r\n":"")
            + (isModel?$"        private new {newUIStr}Model model;\r\n":"")
            + (isCrtl?$"        public {newUIStr}Ctrl Ctrl {{ get {{ if (ctrl == null) ctrl = base.ctrl as {newUIStr}Ctrl; return ctrl; }} }}\r\n":"")
            + (isModel?$"        public {newUIStr}Model Model {{ get {{ if (model == null) model = base.model as {newUIStr}Model; return model; }} }}\r\n":"")
            + "\r\n"
            + "        public override void Awake()\r\n"
            + "		{\r\n"
            + "		    base.Awake();\r\n"
            + "		}\r\n"
            + "        public override void Enable()\r\n"
            + "		{\r\n"
            + "		    base.Enable();\r\n"
            + "		}\r\n"
            + "        public override void Disable()\r\n"
            + "		{\r\n"
            + "		    base.Disable();\r\n"
            + "		}\r\n"
            + "        public override void Destroy()\r\n"
            + "		{\r\n"
            + "		    base.Destroy();\r\n"
            + "		}\r\n"
            + "\r\n"
            + "	}\r\n"
            + "}\r\n";

        Debug.Log(luaData);
        string path = string.Format("{0}{1}{2}Component{2}{1}View.cs", DirectoryName, newFileStr, Path.DirectorySeparatorChar);
        CreatUIScript(path, luaData);
    }
    #endregion

    public const string appendText = "//*AppendCode";

    #region 添加WindowsName
    public static string GetUIWindowsPath = string.Format("Assets{0}Hotfix{0}Module{0}UI{0}UIType.cs", Path.DirectorySeparatorChar);
    private static void CreaWriteWindowsName()
    {
        if (File.Exists(GetUIWindowsPath))
        {
            string str = ReadUITemp(GetUIWindowsPath);
            if (str.Contains(newUIStr))
            {
                Debug.LogError(newUIStr + " is had");
                return;
            }

            string layer = "ELayer.Normal";
            switch (mLayerType)
            {
                case UILayers_TYPE.SceneLayer:
                    layer = "ELayer.Scene";
                    break;
                case UILayers_TYPE.BackgroudLayer:
                    layer = "ELayer.Backgroud";
                    break;
                case UILayers_TYPE.NormalLayer:
                    layer = "ELayer.Normal";
                    break;
                case UILayers_TYPE.InfoLayer:
                    layer = "ELayer.Info";
                    break;
                case UILayers_TYPE.TipLayer:
                    layer = "ELayer.Tip";
                    break;
                case UILayers_TYPE.TopLayer:
                    layer = "ELayer.Top";
                    break;
                default:
                    break;
            }
            string CodeContent = "\r\n\r\n\t"
                + "/// <summary>\r\n"
                + "/// \r\n"
                + "/// </summary>\r\n"
                + $"public static ETModel.UIConfig {newUIStr} = new ETModel.UIConfig(\r\n"
                + $"    \"{ newUIStr}\",\r\n"
                + $"    \"UI/{newUIStr}/{newUIStr}.prefab\",\r\n"
                + $"    {layer},\r\n"
                + (isModel?$"    typeof({newUIStr}Model),": "    null") +"\r\n"
                + (isView?$"    typeof({newUIStr}View),": "    null") +"\r\n"
                + (isCrtl?$"    typeof({newUIStr}Ctrl)": "    null") +");\r\n"
                + appendText;

            if (str.Contains(appendText))
            {
                str = str.Replace(appendText, CodeContent);
            }

            FileStream fs = File.Create(GetUIWindowsPath);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(str);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
    }

    #endregion
    #region CreatNewUI

    static string DirectoryName = string.Format("Assets{0}Hotfix{0}UI{0}", Path.DirectorySeparatorChar);
    private static void CreatUIScript()
    {
        string filePath = DirectoryName + Path.DirectorySeparatorChar + newFileStr;
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        string componentPath = filePath + Path.DirectorySeparatorChar + "Component";
        if (!Directory.Exists(componentPath))
        {
            Directory.CreateDirectory(componentPath);
        }

        //string systemPath = filePath + Path.DirectorySeparatorChar + "System";
        //if (!Directory.Exists(systemPath))
        //{
        //    Directory.CreateDirectory(systemPath);
        //}
    }

    private static void CreatUIScript(string BaseFilePath, string str)
    {
        if (!File.Exists(BaseFilePath))
        {
            FileStream fs = File.Create(BaseFilePath);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(str);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
    }

    public static string ReadUITemp(string Path)
    {
        string str = "";
        if (File.Exists(Path))
        {
            str = File.ReadAllText(Path);
        }
        return str;
    }

    #endregion CreatNewUI
}
