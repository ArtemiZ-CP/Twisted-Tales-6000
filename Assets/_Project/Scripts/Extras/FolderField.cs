using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[System.Serializable]
public class FolderField
{
    [SerializeField] private DefaultAsset _folderAsset;

    public FolderField(Object folderObject)
    {
        _folderAsset = folderObject as DefaultAsset;
    }

    public string ScenesFolderPath
    {
        get
        {
            if (_folderAsset == null) return string.Empty;

            string path = AssetDatabase.GetAssetPath(_folderAsset);

            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }

            return string.Empty;
        }
    }
}
#endif