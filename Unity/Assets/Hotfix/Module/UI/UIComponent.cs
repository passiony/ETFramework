using System;
using System.Collections.Generic;
using System.Diagnostics;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器
/// 容器存储每个UI页面的window，
/// 1.window包含了MVC三层设计，M数据层存储UI页面数据，V视图层用于界面显示，C控制层用于页面的逻辑处理。
/// 2.页面分不同层级，详情查看ELayer设计
/// 3.页面打开和关闭：Open(UIType,参数列表)和Close(UIType)
/// 4.窗口记录栈使用：
///     a)打开页面使用Open，如果是Normal层，统一入栈，默认是ABCD->B->ABCDB，不检测重复模式。
///     b)如果想检测重复ABCD->B->AB，Open参数列表，第一个参数，传OpenType.NoRepeat。
///     c)回退页面使用Back或者PopWindow返回上个Normal页面，并出栈。
///     d)Close页面默认不出栈，如果想出栈调用Close(UIType，True)代表关闭时，会检测栈顶是不是该UI，如果是的话==Back()功能。
/// </summary>
namespace ETHotfix
{
    [ObjectSystem]
    public class UIComponentAwakeSystem : AwakeSystem<UIComponent>
    {
        public override void Awake(UIComponent self)
        {
            self.Awake();
        }
    }

    public class UIComponent : EntitySingleton<UIComponent>
    {
        public Camera UICamera;
        public Transform UIRoot;

        private readonly Dictionary<string, UIWindow> allWindows = new Dictionary<string, UIWindow>();
        private Dictionary<ELayer, UILayer> allLayers = new Dictionary<ELayer, UILayer>();

        private List<UIConfig> windowStack = new List<UIConfig>();
        private UIConfig m_lastOpenUI = null; //上次打开的UI （Normal层）

        public void Awake()
        {
            UICamera = Global.transform.Find("UICamera").GetComponent<Camera>();
            UIRoot = Global.transform.Find("UIRoot").transform;
            InitLayers();
        }

        /// <summary>
        /// 初始化UI层级
        /// </summary>
        void InitLayers()
        {
            for (int i = 0; i < UILayers.Layers.Length; i++)
            {
                var layerInfo = UILayers.Layers[i];

                GameObject go = null;
                var layer = UIRoot.Find(layerInfo.Name);
                if (layer == null)
                    go = CreateLayer(layerInfo);
                else go = layer.gameObject;

                var uilayer = new UILayer();
                uilayer.Init(null, go, layerInfo);
                this.allLayers.Add((ELayer)i, uilayer);
            }
        }

        public void SetScreenMatchValue(float value)
        {
            CanvasScaler cs = allLayers[ELayer.Normal].gameObject.GetComponent<CanvasScaler>();
            cs.matchWidthOrHeight = value;
        }

        GameObject CreateLayer(LayerConfig config)
        {
            GameObject go = new GameObject(config.Name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(UIRoot, false);
            go.transform.localPosition = Vector3.zero;

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;
            canvas.planeDistance = config.PlaneDistance;
            canvas.sortingOrder = config.OrderInLayer;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ConstKey.ResolutionWidth, ConstKey.ResolutionHeight);
            scaler.matchWidthOrHeight = ConstKey.MatchWidthOrHeight;

            GraphicRaycaster graphic = go.AddComponent<GraphicRaycaster>();
            graphic.enabled = true;

            return go;
        }

        /// <summary>
        /// 创建window，初始化window+MVC
        /// </summary>
        UIWindow CreateWindow(UIConfig config)
        {
            try
            {
                //init UI
                UIWindow ui = EntityFactory.Create<UIWindow, UIConfig>(Game.Scene, config);

                //init mvc
                if (config.Model != null)
                {
                    ui.Model = (UIBaseModel)Activator.CreateInstance(config.Model);
                }

                if (config.Ctrl != null)
                {
                    ui.Ctrl = (UIBaseCtrl)Activator.CreateInstance(config.Ctrl);
                    ui.Ctrl._init(ui.Model);
                }

                if (config.View != null)
                {
                    ui.View = (UIBaseView)Activator.CreateInstance(config.View);
                    ui.View._init(ui, ui.Model, ui.Ctrl);
                }

                return ui;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 叠加打开页面,本页面不关闭
        /// </summary>
        /// <param name="config">页面Config</param>
        /// <param name="args">参数列表</param>
        /// <returns>UI页面</returns>
        public UIWindow OpenAdditive(UIConfig config, params object[] args)
        {
            return OpenAdditive(config, OpenType.None, args);
        }

        public UIWindow OpenAdditive(UIConfig config, OpenType openType, params object[] args)
        {
            return OpenWindow(config, openType, args);
        }

        public void OpenAdditiveAsync(UIConfig config, params object[] args)
        {
            OpenAdditiveAsync(config, OpenType.None, null, args);
        }

        public void OpenAdditiveAsync(UIConfig config, Action<UIWindow> complete, params object[] args)
        {
            OpenAdditiveAsync(config, OpenType.None, complete, args);
        }

        public void OpenAdditiveAsync(UIConfig config, OpenType openType, Action<UIWindow> complete, params object[] args)
        {
            OpenWindowAsync(config, openType, (ui) =>
            {
                if (config.Layer == ELayer.Normal)
                {
                    AddToWindowStack(config, openType);
                }
                complete?.Invoke(ui);
            }, args);
        }

        /// <summary>
        /// 返回到页面，相当于NoRepeate模式，ABCD->B = AB
        /// </summary>
        /// <param name="config">页面Config</param>
        /// <param name="args">参数列表</param>
        /// <returns>UI页面</returns>
        public UIWindow BackTo(UIConfig config, params object[] args)
        {
            List<object> list = new List<object>();
            list.Add(OpenType.NoRepeat);
            list.AddRange(args);

            return OpenWindow(config, OpenType.None, list.ToArray());
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="config">页面Config</param>
        /// <param name="args">参数列表</param>
        /// <returns>UI页面</returns>
        public UIWindow Open(UIConfig config, params object[] args)
        {
            return Open(config, OpenType.None, args);
        }

        public UIWindow Open(UIConfig config, OpenType openType, params object[] args)
        {
            if (config.Layer == ELayer.Normal)
            {
                var top = GetTopStackWindow();
                if (top != null)
                {
                    Close(top);
                }
            }
            return OpenWindow(config, openType, args);
        }

        public void OpenAsync(UIConfig config, params object[] args)
        {
            OpenAsync(config, OpenType.None, null, args);
        }

        public void OpenAsync(UIConfig config, Action<UIWindow> callback = null, params object[] args)
        {
            OpenAsync(config, OpenType.None, callback, args);
        }

        public void OpenAsync(UIConfig config, OpenType openType, Action<UIWindow> callback = null, params object[] args)
        {
            OpenWindowAsync(config, openType, (ui) =>
            {

                if (config.Layer == ELayer.Normal)
                {
                    var top = GetTopStackWindow();
                    if (top != null)
                    {
                        Close(top);
                    }

                    AddToWindowStack(config, openType);
                }

                callback?.Invoke(ui);
            }, args);
        }

        void OpenWindowAsync(UIConfig config, OpenType openType, Action<UIWindow> callback, params object[] args)
        {
            try
            {
                UIWindow ui;
                if (allWindows.ContainsKey(config.Name))
                {
                    ui = allWindows[config.Name];
                }
                else
                {
                    ui = CreateWindow(config);
                    allWindows.Add(config.Name, ui);
                }

                InnerOpenAsync(ui,true, callback, args);
            }
            catch (Exception e)
            {
                throw new Exception($"{config.Name} UI 错误: {e.ToStr()}");
            }
        }


        UIWindow OpenWindow(UIConfig config, OpenType openType, params object[] args)
        {
            try
            {
                UIWindow ui;
                if (allWindows.ContainsKey(config.Name))
                {
                    ui = allWindows[config.Name];
                }
                else
                {
                    ui = CreateWindow(config);
                    allWindows.Add(config.Name, ui);
                }

                if (config.Layer == ELayer.Normal)
                {
                    AddToWindowStack(config, openType);
                }

                InnerOpen(ui, true, args);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"{config.Name} UI 错误: {e.ToStr()}");
            }
        }

        /// <summary>
        /// 关闭页面
        /// </summary>
        /// <param name="config">页面Config</param>
        /// <param name="popStack">是否出栈，Normal层UI</param>
        public void Close(UIConfig config, bool exitAnim = false, bool popStack = false)
        {
            UIWindow ui;
            if (!allWindows.TryGetValue(config.Name, out ui))
            {
                return;
            }

            InnerClose(ui, exitAnim);

            if (popStack) //普通close不出栈，pop才出栈
            {
                ui.Model.OnBack();
                ui.View.OnBack();
                RemoveFromWindowStack(config);
            }
        }

        /// <summary>
        /// 销毁UI页面（关闭应用调用）
        /// </summary>
        /// <param name="config">页面配置</param>
        public void Destroy(UIConfig config)
        {
            UIWindow ui;
            if (!allWindows.TryGetValue(config.Name, out ui))
            {
                return;
            }

            InnerClose(ui);
            InnerDestroy(ui);
            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_DESTROY, ui);
        }

        /// <summary>
        /// 删除指定层级UI页面
        /// </summary>
        /// <param name="layer">Layer层级</param>
        public void DestroyWindowByLayer(ELayer layer)
        {
            List<UIWindow> keys = new List<UIWindow>(allWindows.Values);
            foreach (UIWindow ui in keys)
            {
                if (ui.Layer == layer)
                {
                    InnerClose(ui);
                    InnerDestroy(ui);
                }
            }
        }

        /// <summary>
        /// 删除指定层级以外，其他所有层级UI页面
        /// </summary>
        /// <param name="layer">Layer层级</param>
        public void DestroyWindowExceptLayer(ELayer layer)
        {
            List<UIWindow> keys = new List<UIWindow>(allWindows.Values);
            foreach (UIWindow ui in keys)
            {
                if (ui.Layer != layer)
                {
                    InnerClose(ui);
                    InnerDestroy(ui);
                }
            }

            this.allWindows.Clear();
        }

        /// <summary>
        /// 删除所有页面
        /// </summary>
        public void DestroyAllWindow()
        {
            List<UIWindow> keys = new List<UIWindow>(allWindows.Values);
            foreach (UIWindow ui in keys)
            {
                InnerClose(ui);
                InnerDestroy(ui);
            }

            this.allWindows.Clear();
        }

        void InnerOpen(UIWindow ui,bool anim, params object[] args)
        {
            if (ui == null)
            {
                Log.Error("ui not found");
                return;
            }
            FreezeHelper.FreezeUI(ui);

            if (ui.ViewGO == null)
            {
                var asset = ResourcesComponent.Load(ui.Config.PrefabPath);
                var prefab = asset.asset as GameObject;
                GameObject gameObject = GameObjectPool.Instance.GetGameObject(prefab);
                SetViewParent(gameObject, ui.Config.Layer);
                ui.InitGo(gameObject, allLayers[ui.Layer]);
                ui.View.Awake();
            }

            ActivateWindow(ui, anim, args);
        }

        void InnerOpenAsync(UIWindow ui,bool anim, Action<UIWindow> callback, params object[] args)
        {
            FreezeHelper.FreezeUI(ui);

            if (ui.ViewGO != null)
            {
                callback?.Invoke(ui);//先回调，关闭上个页面，然后激活新页面
                ActivateWindow(ui, anim, args);
            }
            else
            {
                ResourcesComponent.LoadAsync(ui.Config.PrefabPath, typeof(UnityEngine.Object),(request) =>
                {
                    var prefab = request.asset as GameObject;
                    GameObject gameObject = GameObjectPool.Instance.GetGameObject(prefab);
                    SetViewParent(gameObject, ui.Config.Layer);
                    ui.InitGo(gameObject, allLayers[ui.Layer]);
                    ui.View.Awake();

                    EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_CREATE, ui);

                    callback?.Invoke(ui);
                    ActivateWindow(ui, anim, args);
                });
            }
        }

        [Conditional("LOG_DEBUG")]
        void LogWindows()
        {
            string windowsMsg = "";
            for (int i = 0; i < windowStack.Count; i++)
            {
                windowsMsg += windowStack[i].Name + "\n";
            }
            Log.Debug($"Window Count = {windowStack.Count} \n [{windowsMsg}]");
        }

        void InnerClose(UIWindow ui, bool exitAnim = false)
        {
            if (!ui.Active) return;
            ui.Active = false;

            if (ui.Config.Layer == ELayer.Normal) //normal 记录上次打开的界面UI
            {
                m_lastOpenUI = ui.Config;
            }

            if (exitAnim)
            {
                StartExitAnim(ui, () =>
                {
                    DeactivateWindow(ui);
                });
            }
            else DeactivateWindow(ui);
        }

        void DeactivateWindow(UIWindow ui)
        {
            if (ui.Model != null)
                ui.Model.Disable();
            ui.ViewGO.SetActive(false);
            ui.View.Disable();

            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_DEACTIVE, ui);
            //release
            Resources.UnloadUnusedAssets();
        }

        void InnerDestroy(UIWindow ui)
        {
            if (ui == null)
                return;

            if (ui.Model != null)
                ui.Model.Destroy();
            if (ui.Ctrl != null)
                ui.Ctrl.Destroy();
            if (ui.View != null)
                ui.View.Destroy();

            GameObject.Destroy(ui.ViewGO);
            allWindows.Remove(ui.Config.Name);
            //release
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 析构，应用自动调用
        /// </summary>
        public override void Dispose()
        {
            if (Id == 0)
            {
                return;
            }

            base.Dispose();
            DestroyAllWindow();
        }

        void ActivateWindow(UIWindow ui,bool withAnim, params object[] args)
        {
            ui.ViewGO.SetActive(true);
            ui.ViewGO.transform.SetAsLastSibling();

            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_ACTIVE, ui);
            LogWindows();

            if (ui.Active)
            {
                FreezeHelper.UnFreezeUI(ui);
                return;
            };

            ui.Active = true;
            ui.Model?.Enable(args);
            ui.View?.Enable();

            if(withAnim)
            {
                StartEnterAnim(ui, () =>
                {
                    FreezeHelper.UnFreezeUI(ui);
                });
            }
            else
            {
                FreezeHelper.UnFreezeUI(ui);
            }
        }

        /// <summary>
        /// 回退UI，Normal层
        /// </summary>
        public void Back()
        {
            if (windowStack.Count < 2)
            {
                Log.Warning("There is No UI Popup");
                return;
            }

            UIConfig closeConfig = windowStack[windowStack.Count - 1];
            UIConfig openConfig = windowStack[windowStack.Count - 2];

            //关闭当前页面
            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_BACK, closeConfig);
            Close(closeConfig, false, true);

            //打开上个页面
            InnerOpen(Get(openConfig), false);
        }

        void AddToWindowStack(UIConfig config, OpenType openType = OpenType.None)
        {
            //如果是NoRepeat模式，ABCDB->AB
            //如果是None模式，ABCDB->ABCDB
            if (openType == OpenType.NoRepeat)
            {
                int index = windowStack.IndexOf(config);
                if (index != -1)
                {
                    windowStack.RemoveRange(index, windowStack.Count - index);
                }
            }

            this.windowStack.Add(config);
        }

        void RemoveFromWindowStack(UIConfig config)
        {
            int index = windowStack.LastIndexOf(config);
            if (index == -1)
                return;

            if (index != windowStack.Count - 1)
                return;

            windowStack.RemoveAt(index);
        }

        public UIConfig GetTopStackWindow()
        {
            if (windowStack.Count > 0)
            {
                return windowStack[windowStack.Count - 1];
            }

            return null;
        }

        public UIConfig GetLastOpenUI()
        {
            return m_lastOpenUI;
        }
        
        public bool IsUIOpen(UIConfig config)
        {
            var ui = Get(config);
            if (ui != null)
            {
                return ui.Active;
            }
            return false;
        }
        
        /// <summary>
        /// 获取指定UI页面
        /// </summary>
        /// <param name="config">UI配置</param>
        /// <returns></returns>
        public UIWindow Get(UIConfig config)
        {
            UIWindow ui;
            this.allWindows.TryGetValue(config.Name, out ui);
            return ui;
        }

        void SetViewParent(GameObject go, ELayer layer)
        {
            RectTransform _rt = go.GetComponent<RectTransform>();
            _rt.SetParent(allLayers[layer].transform, false);
        }

        //开始调用进入动画
        void StartEnterAnim(UIWindow window, Action callback)
        {
            if (window.Config.AnimType == EAnimType.None)
            {
                callback?.Invoke();
                return;
            }

            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_ENTER_ANIM, window);
            window.View.EnterAnim(() =>
            {
                window.View.OnCompleteEnterAnim();
                callback?.Invoke();
            });
        }

        //进入动画播放完毕回调
        void StartExitAnim(UIWindow window, Action callback)
        {
            if (window.Config.AnimType == EAnimType.None)
            {
                callback?.Invoke();
                return;
            }

            EventCenter.Dispatch(EventID.UIFRAME_ON_WINDOW_EXIT_ANIM, window);
            window.View.ExitAnim(() =>
            {
                window.View.OnCompleteExitAnim();
                callback?.Invoke();
            });
        }
    }
}