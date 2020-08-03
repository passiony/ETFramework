using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public enum EAnimType
    {
        None,
        Fade,
        Scale,
        Move,
    }

    public class UIConfig
    {
        public string Name;
        public string PrefabPath;
        public ELayer Layer;
        public Type Model;
        public Type View;
        public Type Ctrl;

        public EAnimType AnimType = EAnimType.Fade;
        public float Duration = 0.7f;

        public UIConfig(string name, string prefabPath, ELayer layer)
        {
            Name = name;
            PrefabPath = prefabPath;
            Layer = layer;
        }

        public UIConfig(string name, string prefabPath, ELayer layer, Type model, Type view, Type ctrl)
        {
            Name = name;
            PrefabPath = prefabPath;
            Layer = layer;
            Model = model;
            View = view;
            Ctrl = ctrl;
        }

        public UIConfig(string name, string prefabPath, ELayer layer, EAnimType animType, Type model, Type view, Type ctrl)
        {
            Name = name;
            PrefabPath = prefabPath;
            Layer = layer;
            Model = model;
            View = view;
            Ctrl = ctrl;

            AnimType = animType;
        }

        public UIConfig(string name, string prefabPath, ELayer layer, float duration, Type model, Type view, Type ctrl)
        {
            Name = name;
            PrefabPath = prefabPath;
            Layer = layer;
            Model = model;
            View = view;
            Ctrl = ctrl;

            Duration = duration;
        }

        public UIConfig(string name, string prefabPath, ELayer layer, EAnimType animType, float duration, Type model, Type view, Type ctrl)
        {
            Name = name;
            PrefabPath = prefabPath;
            Layer = layer;
            Model = model;
            View = view;
            Ctrl = ctrl;

            AnimType = animType;
            Duration = duration;
        }
    }
}
