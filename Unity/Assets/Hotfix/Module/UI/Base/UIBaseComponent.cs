using UnityEngine;

namespace ETHotfix
{
    public class UIBaseComponent
    {
        protected string name;
        public GameObject gameObject { get; protected set; }
        public Transform transform { get; protected set; }
        public UIBaseComponent holder { get; protected set; }
        public UIBaseView view { get; protected set; }

        public virtual void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            this.name = go.name;
            this.holder = _holder;
            this.gameObject = go;
            this.transform = gameObject.transform;

            if (this is UILayer)
            {
                view = null;
            }
            else
            {
                var now_holder = this.holder;
                while (now_holder!=null)
                {
                    if(now_holder is UILayer)
                    {
                        this.view = this as UIBaseView;
                        break;
                    }
                    else if (now_holder.view != null)
                    {
                        this.view = now_holder.view;
                        break;
                    }
                    now_holder = now_holder.holder;
                }

                Log.Assert(view != null, "ui container's view is null");
            }
        }

        public string GetName()
        {
            return name;
        }


        public virtual void Awake()
        {

        }

        public virtual void Enable()
        {

        }

        public virtual void Disable()
        {

        }

        public virtual void Destroy()
        {

        }
    }
}