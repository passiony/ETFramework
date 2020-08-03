using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    public class UIBaseContainer : UIBaseComponent
    {
        public RectTransform rectTransform { get; private set; }

        public ReferenceCollector rc { get; private set; }
        public Dictionary<string, Dictionary<Type, UIBaseComponent>> components { get; private set; }

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            this.rectTransform = this.transform as RectTransform;
            this.rc = gameObject.GetComponent<ReferenceCollector>();
            this.components = new Dictionary<string, Dictionary<Type, UIBaseComponent>>();
        }

        public override void Awake()
        {

        }

        public override void Enable()
        {
            Walk<UIBaseComponent>((comp) =>
            {
                comp.Enable();
            });
        }

        public override void Disable()
        {
            Walk<UIBaseComponent>((comp) =>
            {
                comp.Disable();
            });
        }

        public override void Destroy()
        {
            Walk<UIBaseContainer>((comp) =>
            {
                comp.Destroy();
            });
            this.components = null;
        }

        public virtual T AddComponent<T>(GameObject go, params object[] args) where T : UIBaseComponent, new()
        {
            T t = new T();
            t.Init(this, go, args);
            t.Awake();

            if (!components.ContainsKey(t.GetName()))
            {
                components.Add(t.GetName(), new Dictionary<Type, UIBaseComponent>());
            }

            if (this.components[t.GetName()].ContainsKey(typeof(T)))
            {
                //同一个Transform不能挂两个同类型的组件
                Log.Error(string.Format("已经存在组件 component:{0} | name:{1}", typeof(T).Name, t.GetName()));
            }

            this.components[t.GetName()].Add(typeof(T), t);
            return t;
        }

        public virtual T GetComponent<T>(string name) where T : UIBaseComponent
        {
            Dictionary<Type, UIBaseComponent> comps;
            this.components.TryGetValue(name, out comps);
            if (comps == null)
            {
                return null;
            }

            UIBaseComponent container;
            comps.TryGetValue(typeof(T), out container);
            return container as T;
        }

        public virtual T[] GetComponents<T>() where T : UIBaseComponent
        {
            List<T> list = new List<T>();
            Walk<T>((t) =>
            {
                list.Add(t);
            });

            return list.ToArray();
        }

        public virtual void RemoveComponent<T>(string name) where T : UIBaseComponent
        {
            T comp = GetComponent<T>(name);
            if (comp != null)
            {
                Type type = comp.GetType();
                comp.Destroy();
                this.components[name].Remove(type);
            }
        }

        public virtual void RemoveComponents<T>() where T : UIBaseComponent
        {
            T[] comps = GetComponents<T>();
            foreach (var comp in comps)
            {
                var name = comp.GetName();
                var type = comp.GetType();
                //Log.Debug("removecomponents type = " + type);
                comp.Destroy();
                this.components[name].Remove(type);
            }
        }

        public GameObject GetChild(string key)
        {
            return rc.Get<GameObject>(key);
        }

        public T GetChild<T>(string key) where T : Component
        {
            return GetChild(key).GetComponent<T>();
        }

        protected void Walk<T>(Action<T> callback) where T : UIBaseComponent
        {
            foreach (var comps in components)
            {
                foreach (var cp in comps.Value)
                {
                    if (cp.Value is T)
                    {
                        callback(cp.Value as T);
                    }
                }
            }
        }
    }
}