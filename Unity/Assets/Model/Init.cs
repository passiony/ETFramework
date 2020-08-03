using System;
using System.Threading;
using UnityEngine;

namespace ETModel
{
	public class Init : MonoBehaviour
	{
		private void Start()
		{
            this.StartAsync().Coroutine();
		}
		
		private async ETVoid StartAsync()
		{
			try
			{
				SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

				DontDestroyOnLoad(gameObject);
				Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

                //Game.Scene.AddComponent<GlobalConfigComponent>();
                //Game.Scene.AddComponent<NetOuterComponent>();
                Game.Scene.AddComponent<PlayerComponent>();
                Game.Scene.AddComponent<UnitComponent>();
                Game.Scene.AddComponent<GameObjectPool>();

                Game.Scene.AddComponent<TimerComponent>();
                Game.Scene.AddComponent<ResourcesComponent>();
                var start = DateTime.Now;
                bool result = await ResourcesComponent.InitAsync();
                if (result)
                {
                    Log.Debug(string.Format("init Resources Component success use {0}ms", (DateTime.Now - start).Milliseconds));
                }
                else
                {
                    Log.Error("init Resources Component failed");
                    return;
                }

                start = DateTime.Now;
                Game.Scene.AddComponent<UIComponent>();
                Log.Debug(string.Format("init UIComponent success use {0}ms", (DateTime.Now - start).Milliseconds));
                AsyncLaunchUI().Coroutine();

                // 下载ab包
                start = DateTime.Now;
                await BundleHelper.DownloadBundle();
                Log.Debug(string.Format("init DownloadBundle use {0}ms", (DateTime.Now - start).Milliseconds));

                start = DateTime.Now;
                await Game.Hotfix.LoadHotfixAssembly();
                Log.Debug(string.Format("init LoadHotfixAssembly use {0}ms", (DateTime.Now - start).Milliseconds));

				Game.Hotfix.GotoHotfix();
                Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
                UIComponent.Instance.Close(UIType.UILaunch);
                
            }
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

        private async ETVoid AsyncLaunchUI()
        {
            UIComponent.Instance.Open(UIType.UILaunch);
            await TimerComponent.Instance.WaitAsync(1);
        }

        private void Update()
		{
			OneThreadSynchronizationContext.Instance.Update();
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}