using UnityEngine;
using XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using ETModel;

public class AssetsInit : MonoBehaviour
{
    public string assetPath;

    // Start is called before the first frame update
    void Start()
    {
        /// 初始化
        ResourcesComponent.Initialize(OnInitialized, (error) => { Debug.Log(error); }); 
    }


    private void OnInitialized()
    {
        if (assetPath.EndsWith(".prefab", StringComparison.CurrentCulture))
        {
            ResourcesComponent.LoadAsync<UnityEngine.Object>(assetPath, (a) =>
            {
                var go = Instantiate(a.asset);
                go.name = a.asset.name;
                a.Release();
            });
        }
        else if(assetPath.EndsWith(".unity", StringComparison.CurrentCulture))
        {
            StartCoroutine(LoadSceneAsync());
        }
    } 

    IEnumerator LoadSceneAsync()
    {
        var sceneAsset = ResourcesComponent.LoadScene(assetPath, true, true);
        while(!sceneAsset.isDone)
        {
            Debug.Log(sceneAsset.progress);
            yield return null;
        }
        
        yield return new WaitForSeconds(3);
        ResourcesComponent.Unload(sceneAsset);
    }
}
