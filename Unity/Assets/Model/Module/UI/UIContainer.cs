using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public class UIContainer
    {
        private string name;
        protected GameObject gameObject;
        protected Transform transform;
        protected RectTransform rectTransform;

        protected UIContainer holder;
        protected ReferenceCollector rc;
        protected Dictionary<string, Dictionary<Type, UIContainer>> components;

        public virtual void Init(UIContainer _holder, GameObject go, params object[] args)
        {
            this.holder = _holder;
            this.gameObject = go;
            this.transform = gameObject.transform;
            this.rectTransform = this.transform as RectTransform;
            this.rc = gameObject.GetComponent<ReferenceCollector>();
            this.name = go.name;
            this.components = new Dictionary<string, Dictionary<Type, UIContainer>>();
        }

        public virtual void Awake()
        {

        }

        public virtual void Enable()
        {
            Walk<UIContainer>((comp) => {
                comp.Enable();
            });
        }

        public virtual void Disable()
        {
            Walk<UIContainer>((comp) => {
                comp.Disable();
            });
        }

        public virtual void Destroy()
        {
            Walk<UIContainer>((comp)=> {
                comp.Destroy();
            });
            this.components = null;
        }

        public virtual T AddComponent<T>(GameObject go, params object[] args) where T:UIContainer,new()
        {
            T t = new T();
            t.Init(this,go, args);
            t.Awake();

            if(!components.ContainsKey(t.GetName()))
            {
                components.Add(t.GetName(),new Dictionary<Type, UIContainer>());
            }

            if(this.components[t.GetName()].ContainsKey(typeof(T)))
            {
                //同一个Transform不能挂两个同类型的组件
                Log.Error(string.Format("已经存在组件 component:{0} | name:{1}", typeof(T).Name, t.GetName()));
            }

            this.components[t.GetName()].Add(typeof(T), t);
            return t;
        }

        public virtual T GetComponent<T>(string name) where T:UIContainer
        {
            Dictionary<Type, UIContainer> comps;
            this.components.TryGetValue(name,out comps);
            if(comps == null)
            {
                return null;
            }

            UIContainer container;
            comps.TryGetValue(typeof(T),out container);
            return container as T;
        }

        public virtual T[] GetComponents<T>() where T:UIContainer
        {
            List<T> list = new List<T>();
            Walk<T>((t)=> {
                list.Add(t);
            });

            return list.ToArray();
        }

        public virtual void RemoveComponent<T>(string name) where T:UIContainer
        {
            T comp = GetComponent<T>(name);
            if(comp!=null)
            {
                Type type = comp.GetType();
                comp.Destroy();
                this.components[name].Remove(type);
            }
        }

        public virtual void RemoveComponents<T>() where T:UIContainer
        {
            T[] comps = GetComponents<T>();
            foreach (var comp in comps)
            {
                var name = comp.GetName();
                var type = comp.GetType();

                comp.Destroy();
                this.components[name].Remove(type);
            }
        }

        public string GetName()
        {
            return name;
        }

        public GameObject GetChild(string key)
        {
            return rc.Get<GameObject>(key);
        }

        public T GetChild<T>(string key) where T : Component
        {
            return GetChild(key).GetComponent<T>();
        }

        protected void Walk<T>(Action<T> callback) where T : UIContainer
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