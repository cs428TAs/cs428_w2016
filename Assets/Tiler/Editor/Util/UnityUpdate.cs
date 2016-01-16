using UnityEditor;
using UnityEngine;

namespace Tiler
{
    public static class UnityUpdate
    {
        public static void PerformCheck()
        {
            // Tilesets
            var tilesetList = UnityInternal.GetAssetsInProjectOfType<Tileset>().ToArray();

            foreach (var t in tilesetList)
            {
                var path = AssetDatabase.GetAssetPath(t.InstanceID);
                var tileset = (Tileset)AssetDatabase.LoadAssetAtPath(path, typeof(Tileset));

                tileset.VersionCheck();
            }
        }
    }
}
