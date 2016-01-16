using UnityEditor;

namespace LevelEditor
{
    public class Menu
    {
        [MenuItem(@"Window/Tiler", false, 0)]
        public static void Tiler()
        {
            TilerWindow.Create();
        }
    }
}