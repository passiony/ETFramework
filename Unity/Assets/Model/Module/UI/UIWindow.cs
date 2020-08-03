using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    [ObjectSystem]
    public class UiAwakeSystem : AwakeSystem<UIWindow , UIConfig>
    {
        public override void Awake(UIWindow  self, UIConfig config)
        {
            self.Awake(config);
        }
    }

	[HideInHierarchy]
	public sealed class UIWindow : Entity
	{
		public string Name { get; private set; }
		public ELayer Layer { get; private set; }
		public UIConfig Config { get; private set; }

        public UIBaseModel Model;
        public UIBaseView View;
        public UIBaseCtrl Ctrl;

        public int MaxOderPerWindow = 10;
        public int topWindowOrder;

        public Dictionary<string, UIWindow > children = new Dictionary<string, UIWindow >();

        public void Awake(UIConfig config)
		{
			this.children.Clear();
            this.Name = config.Name;
            this.Layer = config.Layer;
			this.Config = config;
            this.topWindowOrder = UILayers.GetLayer(Layer).OrderInLayer;
        }

		public void InitGo(GameObject go)
		{
			go.AddComponent<ComponentView>().Component = this;
			go.layer = LayerMask.NameToLayer(LayerNames.UI);
			this.ViewGO = go;
			this.View.Init(null, go, null);
		}

        // pop window order
        public int PopWindowOder()
        {
            var cur = this.topWindowOrder;
            this.topWindowOrder += MaxOderPerWindow;
            return cur;
        }

        // push window order
        public void PushWindowOrder()
        {
            this.topWindowOrder -= MaxOderPerWindow;
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (UIWindow  ui in this.children.Values)
			{
				ui.Dispose();
			}
			
			UnityEngine.Object.Destroy(this.ViewGO);
			children.Clear();
		}

		public void SetAsFirstSibling()
		{
			this.ViewGO.transform.SetAsFirstSibling();
		}

		public void Add(UIWindow  ui)
		{
			this.children.Add(ui.Name, ui);
			ui.Parent = this;
		}

		public void Remove(string name)
		{
			UIWindow  ui;
			if (!this.children.TryGetValue(name, out ui))
			{
				return;
			}
			this.children.Remove(name);
			ui.Dispose();
		}

		public UIWindow  Get(string name)
		{
			UIWindow  child;
			if (this.children.TryGetValue(name, out child))
			{
				return child;
			}
			GameObject childGameObject = this.ViewGO.transform.Find(name)?.gameObject;
			if (childGameObject == null)
			{
				return null;
			}
			child = EntityFactory.Create<UIWindow , string, GameObject>(this.Domain, name, childGameObject);
			this.Add(child);
			return child;
		}

	}
}
