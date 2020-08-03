using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    public class ClearCacheEditor : Editor
    {
        [MenuItem("Tools/Clear Cache", false, 50)]
        public static void ClearAllPlerPrefs()
        {
            bool check = EditorUtility.DisplayDialog("Clear Cache Warning",
                string.Format("You Will Clear All PlayerPrefs Cache！ \n\nContinue ?"),
                "Confirm", "Cancel");
            if (!check)
            {
                return;
            }

            var start = System.DateTime.Now;
            PlayerPrefs.DeleteAll();

            Debug.Log("Finished Clear All PlayerPrefs! use " + (System.DateTime.Now - start).TotalSeconds + "s");
        }
    }
}
