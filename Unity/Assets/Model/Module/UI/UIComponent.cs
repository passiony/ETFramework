using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    [ObjectSystem]
    public class UIComponentAwakeSystem : AwakeSystem<UIComponent>
    {
        public override void Awake(UIComponent self)
        {
            self.Awake();
        }
    }


    /// <summary>
    /// 管理所有UI
    /// </summary>
    public class UIComponent : EntitySingleton<UIComponent>
    {
        public Camera UICamera;
        public Transform UIRoot;

        private readonly Dictionary<string, UIWindow > allWindows = new Dictionary<string, UIWindow >();
        private Dictionary<ELayer, Canvas> allLayers = new Dictionary<ELayer, Canvas>();
        private List<string> windowStack = new List<string>();

        public void Awake()
        {
            UICamera = Global.transform.Find("UICamera").GetComponent<Camera>();
            UIRoot = Global.transform.Find("UIRoot").transform;
            InitLayers();
            
            Open(UIType.UIFreeze).ViewGO.SetActive(false);
        }

        void InitLayers()
        {
            for (int i = 0; i < UILayers.Layers.Length; i++)
            {
                var layerInfo = UILayers.Layers[i];

                Canvas canvas = null;
                var layer = UIRoot.Find(layerInfo.Name);
                if (layer == null)
                    canvas = CreateLayer(layerInfo);
                else canvas = layer.GetComponent<Canvas>();

                this.allLayers.Add((ELayer)i, canvas);
            }
        }

        Canvas CreateLayer(LayerConfig config)
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

            return canvas;
        }

        UIWindow  CreateWindow(UIConfig config)
        {
            try
            {
                //init UI
                UIWindow  ui = EntityFactory.Create<UIWindow , UIConfig>(Game.Scene, config);

                //init mvc
                if (config.Model != null)
                {
                    ui.Model = (UIBaseModel)Activator.CreateInstance(config.Model);
                }
                if (config.Ctrl != null)
                {
                    ui.Ctrl = (UIBaseCtrl)Activator.CreateInstance(config.Ctrl);
                    ui.Ctrl.Init(ui.Model);
                }
                if (config.View != null)
                {
                    ui.View = (UIBaseView)Activator.CreateInstance(config.View);
                    ui.View.Init(ui, ui.Model, ui.Ctrl);
                }
                return ui;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public UIWindow  Open(UIConfig config, params object[] args)
        {
            try
            {
                UIWindow  ui;
                if (allWindows.ContainsKey(config.Name))
                {
                    ui = allWindows[config.Name];
                }
                else
                {
                    ui = CreateWindow(config);
                    allWindows.Add(config.Name, ui);
                }

                InnerOpen(ui, args);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"{config.Name} UI 错误: {e.ToString()}");
            }
        }

        public void Close(UIConfig config)
        {
            UIWindow  ui;
            if (!allWindows.TryGetValue(config.Name, out ui))
            {
                return;
            }

            InnerClose(ui);
        }

        public void Destroy(UIConfig config)
        {
            UIWindow  ui;
            if (!allWindows.TryGetValue(config.Name, out ui))
            {
                return;
            }

            InnerClose(ui);
            InnerDestroy(ui);
        }

        public void DestroyWindowByLayer(ELayer layer)
        {
            List<UIWindow > keys = new List<UIWindow >(allWindows.Values);
            foreach (UIWindow  ui in keys)
            {
                if (ui.Layer == layer)
                {
                    InnerClose(ui);
                    InnerDestroy(ui);
                }
            }
            this.allWindows.Clear();
        }

        public void DestroyWindowExceptLayer(ELayer layer)
        {
            List<UIWindow > keys = new List<UIWindow >(allWindows.Values);
            foreach (UIWindow  ui in keys)
            {
                if (ui.Layer != layer)
                {
                    InnerClose(ui);
                    InnerDestroy(ui);
                }
            }
            this.allWindows.Clear();
        }

        public void DestroyAllWindow()
        {
            List<UIWindow > keys = new List<UIWindow >(allWindows.Values);
            foreach (UIWindow  ui in keys)
            {
                InnerClose(ui);
                InnerDestroy(ui);
            }
            this.allWindows.Clear();
        }

        void SetViewParent(UIWindow  ui, ELayer layer)
        {
            RectTransform _rt = ui.ViewGO.GetComponent<RectTransform>();
            _rt.SetParent(allLayers[layer].transform, false);
        }

        void InnerOpen(UIWindow  ui, params object[] args)
        {
            if (ui.ViewGO == null)
            {
                var prefab = ResourcesComponent.Load(ui.Config.PrefabPath).asset as GameObject;
                GameObject gameObject = GameObject.Instantiate(prefab);
                ui.InitGo(gameObject);
                SetViewParent(ui, ui.Config.Layer);
                ui.ViewGO.transform.SetAsLastSibling();
                ui.View.Awake();
            }

            ActivateWindow(ui, args);
        }

        void InnerClose(UIWindow  ui)
        {
            if (ui.Model != null)
                ui.Model.Disable();
            ui.ViewGO.SetActive(false);
            ui.View.Disable();
        }

        void ActivateWindow(UIWindow  ui, params object[] args)
        {
            if (ui.Model != null)
                ui.Model.Enable(args);
            ui.ViewGO.SetActive(true);
            ui.View.Enable();
        }

        void InnerDestroy(UIWindow  ui)
        {
            if (ui == null)
                return;

            if (ui.Model != null)
                ui.Model.Destroy();
            if (ui.Ctrl != null)
                ui.Ctrl.Destroy();
            if (ui.View != null)
                ui.View.Destroy();
        }

        public UIWindow Get(UIConfig config)
        {
            UIWindow ui;
            this.allWindows.TryGetValue(config.Name, out ui);
            return ui;
        }
        
        public override void Dispose()
        {
            if (Id == 0)
            {
                return;
            }

            base.Dispose();
            DestroyAllWindow();
        }
    }
}