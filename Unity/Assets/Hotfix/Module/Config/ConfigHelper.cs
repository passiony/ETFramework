using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
	public static class ConfigHelper
	{
		public static string GetText(string key)
		{
			try
			{
                string path = string.Format("Config/{0}.txt", key);
                TextAsset config = ResourcesComponent.Load(path, typeof(TextAsset)).asset as TextAsset;

                string configStr = config.text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}

        public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}
	}
}