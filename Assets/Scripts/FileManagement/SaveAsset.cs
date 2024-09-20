#if UNITY_EDITOR
// Save object
using UnityEditor;
using UnityEngine;

public static class SaveAsset
{
    public static void SaveGameObjectAsPrefab(GameObject obj)
    {
        if (obj != null)
        {
            var savePath = "Assets/savedPrefab.prefab";
            // Save the GameObject as a prefab
            PrefabUtility.SaveAsPrefabAsset(obj, savePath);
            Debug.Log("Saved GameObject to: " + savePath);
            
            if (obj.TryGetComponent<MeshFilter>(out var mf))
            {
                savePath = "Assets/thing.mesh";
                AssetDatabase.CreateAsset(mf.mesh, savePath);
            }
        }
        else
        {
            Debug.LogError("GameObject is null, cannot save.");
        }
    }
}

#endif
