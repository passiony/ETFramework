using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
    public interface IPreLoader
    {
        AssetLoader[] GetLoaders();
    }
    
    [ObjectSystem]
    public class PreloadComponentAwakeSystem : AwakeSystem<PreloadComponent>
    {
        public override void Awake(PreloadComponent self)
        {
            self.Awake();
        }
    }
    
    public class PreloadComponent :EntitySingleton<PreloadComponent>
    {
        private List<AssetLoader> preload_assets;
        private int totalCount;
        
        public void Awake()
        {
            preload_assets = new List<AssetLoader>();
            totalCount = 0;
            
            InitLoaders();

            for (int i = 0; i < preload_assets.Count; i++)
            {
                if (!this.preload_assets[i].background)
                {
                    this.totalCount++;
                }
            }
        }

        void InitLoaders()
        {
            List<Type> types = Game.EventSystem.GetTypes(AttributeType.Preload);

            foreach (var type in types)
            {
                object obj = Activator.CreateInstance(type);

                IPreLoader iloader = obj as IPreLoader;
                if (iloader == null)
                {
                    throw new Exception($"class: {type.Name} not inherit from IPreLoader");
                }

                this.preload_assets.AddRange(iloader.GetLoaders());
            }
        }

        public async ETTask LoadAsync(Action<float> progress)
        {
            var ecs = new ETTaskCompletionSource();
            float progress_slice = 1.0f / totalCount;

            if (totalCount == 0)
            {
                progress?.Invoke(1);
                ecs.SetResult();
                return;
            }

            int finishCount = 0;
            foreach (var loader in preload_assets)
            { 
                loader.LoadAsync(()=>{
                    finishCount++;
                    // Log.Warning("Preload Finish : "+finishCount+"/"+this.totalCount);
                    progress?.Invoke(finishCount * progress_slice);

                    if (finishCount >= totalCount)
                        ecs.SetResult();
                });
            }
            await ecs.Task;
        }
        
        // void InitPreload()
        // {
        //     AddPreloadAssets(UIType.UIMainBackground, UIType.UIMainBackground.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UIMainBackground, UIType.UIMainBackground.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UITopBar, UIType.UITopBar.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UIMain, UIType.UIMain.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UICrossGame, UIType.UICrossGame.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UIBookShop, UIType.UIBookShop.PrefabPath, typeof(GameObject));
        //     AddPreloadAssets(UIType.UIBookShop, "UI/UIBookShop/BookShopItems/BookShopItem1.prefab", typeof(GameObject));
        //
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/CharPlay_Normal.png", typeof(Sprite),false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/CharPlay_Pressed.png", typeof(Sprite), false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/CharShow_Normal.png", typeof(Sprite), false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/CharShow_Add.png", typeof(Sprite), false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/CharSearch.png", typeof(Sprite), false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/Theme1_Normal.png", typeof(Sprite), false,true);
        //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/Words/Theme1_Add.png", typeof(Sprite), false,true);
        //     
        //     // //Letters
        //     // string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //     // foreach (var letter in letters)
        //     // {
        //     //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/CharPlay/{letter}_play_normal.png", typeof(Sprite),true);
        //     //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/CharPlay/{letter}_play_pressed.png", typeof(Sprite), true);
        //     //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/CharShow/{letter}_show_normal.png", typeof(Sprite), true);
        //     //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/CharShow/{letter}_show_add.png", typeof(Sprite), true);
        //     //     AddPreloadAssets(UIType.UICrossGame, $"Atlas/CharSearch/{letter}_5.png", typeof(Sprite), true);
        //     // }
        // }
        // public void AddPreloadAssets(UIConfig config, string path, Type type, bool background = false, bool loadAll = false)
        // {
        //     AddPreloadAssets(config.Name, path, type, background, loadAll);
        // }
        //
        // public void AddPreloadAssets(string cacheName, string path, Type type, bool background = false, bool loadAll = false)
        // {
        //     var loader = new AssetLoader(cacheName, path, type, background, loadAll);
        //     preload_assets.Add(loader);
        //
        //     if (!background) 
        //         totalCount++;
        // }
    }
}