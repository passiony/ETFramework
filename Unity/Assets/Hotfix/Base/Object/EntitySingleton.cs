using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    public class EntitySingleton<T>: Entity where T : Entity
    {
        private static T t;

        public static T Instance
        {
            get
            {
                if (t == null)
                {
                    t = Game.Scene.GetComponent<T>();
                }

                return t;
            }
        }
    }
}