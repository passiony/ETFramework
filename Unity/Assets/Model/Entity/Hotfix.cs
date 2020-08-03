using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if !ILRuntime
using System.Reflection;
#endif

namespace ETModel
{
	public sealed class Hotfix: Object
	{
#if ILRuntime
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private MemoryStream dllStream;
		private MemoryStream pdbStream;
#else
		private Assembly assembly;
#endif

		private IStaticMethod start;
		private List<Type> hotfixTypes;

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		public void GotoHotfix()
		{
#if ILRuntime
			ILHelper.InitILRuntime(this.appDomain);
#endif
			this.start.Run();
		}

		public List<Type> GetHotfixTypes()
		{
			return this.hotfixTypes;
		}

		public async ETTask LoadHotfixAssembly()
		{
#if LOGGER_ON
			var dllBytes = await LoadAsync("Code/Hotfix.dll.bytes");
			var pdbBytes = await LoadAsync("Code/Hotfix.pdb.bytes");

			LoadDLL(EncryptHelper.DecryptBytes(dllBytes), EncryptHelper.DecryptBytes(pdbBytes));
#else
				var dllBytes = await LoadAsync("Code/Hotfix.dll.bytes");
				LoadDLL(EncryptHelper.DecryptBytes(dllBytes), null);
#endif
		}

		ETTask<byte[]> LoadAsync(string path)
		{
			var tcs = new ETTaskCompletionSource<byte[]>();
			
			ResourcesComponent.LoadAsync(path, typeof(TextAsset), (request) =>
			{
				byte[] bytes = (request.asset as TextAsset).bytes;
				
				tcs.SetResult(bytes);
				request.Release();
			});

			return tcs.Task;
		}
		
		void LoadDLL(byte[] assBytes, byte[] pdbBytes)
		{
#if ILRuntime
            Log.Debug($"当前使用的是ILRuntime模式");
            this.appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

            this.dllStream = new MemoryStream(assBytes);
            if (pdbBytes == null)
            {
                this.appDomain.LoadAssembly(this.dllStream);
            }
            else
            {
                this.pdbStream = new MemoryStream(pdbBytes);
                this.appDomain.LoadAssembly(this.dllStream,this.pdbStream,new Mono.Cecil.Pdb.PdbReaderProvider());
            }


            this.start = new ILStaticMethod(this.appDomain, "ETHotfix.Init", "Start", 0);

            this.hotfixTypes = this.appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
#else
			Log.Debug($"当前使用的是Mono模式");

			this.assembly = Assembly.Load(assBytes, pdbBytes);

			Type hotfixInit = this.assembly.GetType("ETHotfix.Init");
			this.start = new MonoStaticMethod(hotfixInit, "Start");
			
			this.hotfixTypes = this.assembly.GetTypes().ToList();
#endif
		}
	}
}