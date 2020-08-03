using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public class EntitySingleton<T> : Entity where T: Entity
    {
        private static T t;
        public static T Instance
        {
            get
            {
                return Game.Scene.GetComponent<T>();
            }
        }
    }
}