using System;

namespace ETModel
{
	public interface IEnableSystem
	{
		Type Type();
		void Run(object o);
	}

	public abstract class EnableSystem<T> : IEnableSystem
    {
		public void Run(object o)
		{
			this.Enable((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Enable(T self);
	}
}
