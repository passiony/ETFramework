﻿using System;
 using ETModel;

 namespace ETHotfix
{
	public partial class Entity
	{
		private Entity CreateWithComponentParent(Type type)
		{
			Entity component;
			if (type.IsDefined(typeof (NoObjectPool), false))
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			
			this.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component);
			return component;
		}

        private Entity CreateWithComponentParent<A>(Type type,A a)
        {
            Entity component;
            if (type.IsDefined(typeof(NoObjectPool), false))
            {
                component = (Entity)Activator.CreateInstance(type);
            }
            else
            {
                component = Game.ObjectPool.Fetch(type);
            }

            this.Domain = this.Domain;
            component.Id = this.Id;
            component.ComponentParent = this;

            Game.EventSystem.Awake(component, a);
            return component;
        }

        private Entity CreateWithComponentParent<A,B>(Type type, A a, B b)
        {
            Entity component;
            if (type.IsDefined(typeof(NoObjectPool), false))
            {
                component = (Entity)Activator.CreateInstance(type);
            }
            else
            {
                component = Game.ObjectPool.Fetch(type);
            }

            this.Domain = this.Domain;
            component.Id = this.Id;
            component.ComponentParent = this;

            Game.EventSystem.Awake(component, a, b);
            return component;
        }

        private Entity CreateWithComponentParent<A, B, C>(Type type, A a, B b, C c)
        {
            Entity component;
            if (type.IsDefined(typeof(NoObjectPool), false))
            {
                component = (Entity)Activator.CreateInstance(type);
            }
            else
            {
                component = Game.ObjectPool.Fetch(type);
            }

            this.Domain = this.Domain;
            component.Id = this.Id;
            component.ComponentParent = this;

            Game.EventSystem.Awake(component, a, b, c);
            return component;
        }

        private T CreateWithComponentParent<T>(bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A>(A a, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A, B>(A a, B b, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a, b);
			return (T)component;
		}

		private T CreateWithComponentParent<T, A, B, C>(A a, B b, C c, bool isFromPool = true) where T : Entity
		{
			Type type = typeof (T);
			Entity component;
			if (!isFromPool)
			{
				component = (Entity)Activator.CreateInstance(type);
			}
			else
			{
				component = Game.ObjectPool.Fetch(type);	
			}
			component.Domain = this.Domain;
			component.Id = this.Id;
			component.ComponentParent = this;
			
			Game.EventSystem.Awake(component, a, b, c);
			return (T)component;
		}
	}
}